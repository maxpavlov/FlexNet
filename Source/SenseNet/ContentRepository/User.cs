using System;
using System.Linq;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using System.Text;
using SenseNet.ContentRepository.Security.ADSync;
using System.Collections.Generic;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Fields;
using System.Security.Principal;
using SenseNet.Search;
using System.Xml.Serialization;
using System.IO;

namespace SenseNet.ContentRepository
{

	[ContentHandler]
	public class User : GenericContent, IUser, IADSyncable
	{
        [Obsolete("Use typeof(User).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(User).Name;
        
		//=================================================================================== Static Members


		public static User Administrator
		{
			get
			{
                AccessProvider.ChangeToSystemAccount();
				User admin = Node.LoadNode(RepositoryConfiguration.AdministratorUserId) as User;
                AccessProvider.RestoreOriginalUser();
				if (admin == null)
					throw new ApplicationException("Administrator cannot be found.");
				return admin;
			}
		}

        private static User _visitor;
        private static object _visitorLock = new object();
		public static User Visitor
		{
			get
			{
                if (_visitor == null)
                {
                    lock (_visitorLock)
                    {
                        if (_visitor == null)
                        {
                            Node visitor = Node.LoadNode(RepositoryConfiguration.VisitorUserId);
                            _visitor = visitor as User;
                        }
                    }
                }
				return _visitor;
			}
		}

		public static IUser Current
		{
			get
			{
				return (IUser)AccessProvider.Current.GetCurrentUser();
			}
			set // [Explicit SignIn]
			{
				if (value == null)
					throw new ArgumentNullException("value"); // Logout: set User.Visitor rather than null
                if (value.Id == 0)
                    throw new SenseNetSecurityException("Cannot log in with a non-saved (non-existing) user.");

				AccessProvider.Current.SetCurrentUser(value);
			}
		}

        //=================================================================================== Private Properties
        private string _password;
        private bool _syncObject = true;

		//=================================================================================== Public Properties

        private WindowsIdentity _windowsIdentity;
        public WindowsIdentity WindowsIdentity
        {
            get { return _windowsIdentity; }
            set { _windowsIdentity = value; }
        }

		[RepositoryProperty("Enabled", RepositoryDataType.Int)]
		public bool Enabled
		{
			get { return this.GetProperty<int>("Enabled") != 0; }
			set { this["Enabled"] = value ? 1 : 0; }
		}

        
        [RepositoryProperty("Domain", RepositoryDataType.String)]
        public string Domain
        {
            get { return this.GetProperty<string>("Domain"); }
            private set { this["Domain"] = value; }
        }

		[RepositoryProperty("Email")]
		public string Email
		{
			get { return this.GetProperty<string>("Email"); }
			set { this["Email"] = value; }
		}
		[RepositoryProperty("FullName")]
		public string FullName
		{
			get { return this.GetProperty<string>("FullName"); }
			set { this["FullName"] = value; }
		}

        private const string OLDPASSWORDS = "OldPasswords";
        [RepositoryProperty(OLDPASSWORDS, RepositoryDataType.Text)]
        public string OldPasswords
        {
            get { return base.GetProperty<string>(OLDPASSWORDS); }
            set { base.SetProperty(OLDPASSWORDS, value); }
        }

        internal List<PasswordField.OldPasswordData> GetOldPasswords()
        {
            if (this.OldPasswords == null)
                return new List<PasswordField.OldPasswordData>();

            var serializer = new XmlSerializer(typeof(List<PasswordField.OldPasswordData>));
            using (var reader = new StringReader(this.OldPasswords))
            {
                var oldPasswords = serializer.Deserialize(reader) as List<PasswordField.OldPasswordData>;
                return oldPasswords;
            }
        }

        private void SetOldPasswords(List<PasswordField.OldPasswordData> oldPasswords)
        {
            if (oldPasswords == null)
                return;

            var serializer = new XmlSerializer(typeof(List<PasswordField.OldPasswordData>));
            using (var writer = new StringWriter()) {
                serializer.Serialize(writer, oldPasswords);
                this.OldPasswords = writer.ToString();
            }
        }

        // user's fullname is displayed on UI wherever it is filled. This display logic is used by content picker, label picker and explore lists.
        public override string DisplayName
        {
            get
            {
                return string.IsNullOrEmpty(this.FullName) ? base.DisplayName : this.FullName;
            }
            set
            {
                this.FullName = value;
            }
        }

	    public virtual string AvatarUrl
	    {
	        get
	        {
                // avatar is either image reference or image binary
	            Image imageRef = null;
                var nodeList = this["ImageRef"] as IEnumerable<Node>;
                if (nodeList != null)
                    imageRef = nodeList.FirstOrDefault() as Image;

	            var imageData = this["ImageData"] as BinaryData;


                // use imagefield static methods to get url for avatar
	            var imageFieldData = new ImageField.ImageFieldData(imageRef, imageData);
	            var imageRequestMode = ImageField.GetImageMode(imageFieldData);
	            var imageUrl = ImageField.GetImageUrl(imageRequestMode, imageFieldData, this.Id, "ImageData");
	            return imageUrl;
	        }
	    }
        //[RepositoryProperty("Avatar", RepositoryDataType.Reference)]
        //public Node Avatar
        //{
        //    get
        //    {
        //        Node value = this.GetReference<Node>("Avatar");
        //        if (value == null)
        //            value = Node.LoadNode(this.Path + ".jpg");
        //        return value;
        //    }
        //    set { this.SetReference("Avatar", value); }
        //}
		[RepositoryProperty("PasswordHash")]
		public string PasswordHash
		{
			get { return this.GetProperty<string>("PasswordHash"); }
			set { this["PasswordHash"] = value; }
		}

		public string Username
		{
			get
            {
                // Domain hack - needed by the WebPI IIS7 Integrated mode
                string domain;
                if (PropertyTypes["Domain"] != null)
                {
                    domain = Domain;
                }
                else
                {
                    domain = 
                        System.Web.Configuration.WebConfigurationManager.AppSettings["DefaultDomain"] ?? "BuiltIn";
                }

                return String.Concat(domain, @"\", Name);
            }
		}

	    private UserProfile _profile;
	    public UserProfile Profile
	    {
	        get
	        {
                if (_profile == null)
                {
                    var upPath = GetProfilePath();
                    if (!string.IsNullOrEmpty(upPath))
                        _profile = Node.Load<UserProfile>(upPath);
                }

	            return _profile;
	        }
	    }

		//=================================================================================== Construction

        public User(Node parent) : this(parent, null) { }
		public User(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected User(NodeToken token) : base(token) { }

		//=================================================================================== Methods

        public static User Load(string domain, string name)
        {
            return Load(domain, name, ExecutionHint.None);
        }

		public static User Load(string domain, string name, ExecutionHint hint)
		{
			if (domain == null)
				throw new ArgumentNullException("domain");
			if (name == null)
				throw new ArgumentNullException("name");

            var path = String.Concat(Repository.ImsFolderPath, RepositoryPath.PathSeparator, domain);
            var type = ActiveSchema.NodeTypes[typeof(User).Name];

            IEnumerable<Node> users;
            switch (hint)
            {
                case ExecutionHint.None:
                    if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
                        users = SearchInLucene(path, type.Name, name);
                    else
                        users = NodeQuery.QueryNodesByTypeAndPathAndName(type, false, path, false, name).Nodes;
                    break;
                case ExecutionHint.ForceRelationalEngine:
                    users = NodeQuery.QueryNodesByTypeAndPathAndName(type, false, path, false, name).Nodes;
                    break;
                case ExecutionHint.ForceIndexedEngine:
                    users = SearchInLucene(path, type.Name, name);
                    break;
                default:
                    throw new NotImplementedException();
            }

            var count = users.Count();
            if (count == 0)
                return null;

            if (count > 1)
                throw new ApplicationException(string.Format("The Username (='Domain\\Name', in this case '{0}\\{1}') should be unique. {2} matching users have been found.", domain, name, count));

            return users.First() as User;
		}

        private static IEnumerable<User> SearchInLucene(string path, string typeName, string name)
        {
            var cquery = String.Format("+TypeIs:{0} +InTree:\"{1}\" +Name:\"{2}\"", typeName, path, name);
            var result = ContentQuery.Query(cquery);
            return result.Nodes.Cast<User>();
        }

        public virtual PasswordCheckResult CheckPassword(string password, List<PasswordField.OldPasswordData> oldPasswords)
        {
            return CheckPassword(this.GetContentType(), "Password", password, oldPasswords);
        }

        public static PasswordCheckResult CheckPassword(ContentType contentType, string fieldName, string password, List<PasswordField.OldPasswordData> oldPasswords)
        {
            var pwFieldSetting = contentType.GetFieldSettingByName(fieldName) as PasswordFieldSetting;
            if (pwFieldSetting != null)
                throw new NotSupportedException(string.Format("Cannot check password if the field is not a PasswordField. ContentType: ", contentType, ", field: ", fieldName));
            return pwFieldSetting.CheckPassword(password, oldPasswords);
        }

		public static bool CheckPasswordMatch(string passwordInClearText, string encodedPassword)
		{
			return EncodePassword(passwordInClearText) == encodedPassword;
		}
		public static string EncodePassword(string passwordInClearText)
		{
            if (string.IsNullOrEmpty(passwordInClearText))
                return String.Empty;

			byte[] data = System.Text.Encoding.UTF8.GetBytes(passwordInClearText);
			using (System.Security.Cryptography.HashAlgorithm sha256 = new System.Security.Cryptography.SHA256Managed())
			{
				byte[] encryptedBytes = sha256.TransformFinalBlock(data, 0, data.Length);
				return Convert.ToBase64String(sha256.Hash);
			}
		}

        public static void Reset()
        {
            _visitor = null;
        }

        public static User RegisterUser(string fullUserName)
        {
            if (string.IsNullOrEmpty(fullUserName))
                return null;

            var slashIndex = fullUserName.IndexOf('\\');
            var domain = fullUserName.Substring(0, slashIndex);
            var username = fullUserName.Substring(slashIndex + 1);

            var user = User.Load(domain, username);

            if (user != null)
                return user;

            try
            {
                AccessProvider.Current.SetCurrentUser(User.Administrator);

                var dom = Node.Load<Domain>(RepositoryPath.Combine(Repository.ImsFolderPath, domain));

                if (dom == null)
                {
                    //create domain
                    dom = new Domain(Repository.ImsFolder) { Name = domain };
                    dom.Save();
                }

                //create user
                user = new User(dom) { Name = username, Enabled = true, FullName = username };
                user.Save();

                Group.Administrators.AddMember(user);
            }
            finally
            {
                if (user != null)
                    AccessProvider.Current.SetCurrentUser(user);
            }

            return user;
        }

        //=================================================================================== Profile

        private string GetProfileParentPath()
        {
            return RepositoryPath.Combine(Repository.UserProfilePath, this.Domain ??  ContentRepository.Domain.BuiltInDomainName);
        }

        private string GetProfileName()
        {
            return this.Name;
        }

        public string GetProfilePath()
        {
            return RepositoryPath.Combine(GetProfileParentPath(), GetProfileName());
        }

        public void CreateProfile()
        {
            this.CreateProfile(null);
        }

        public void CreateProfile(Node template)
        {
            if (!Repository.UserProfilesEnabled)
                return;

            var upPath = GetProfilePath();

            using (new SystemAccount())
            {
                if (Node.Exists(upPath))
                    return;

                var uDomainPath = GetProfileParentPath();
                var profiles = Node.LoadNode(Repository.UserProfilePath);
                if (profiles == null) {
                    profiles = Content.CreateNew("Profiles", Repository.Root, "Profiles").ContentHandler;
                    profiles.Save();
                }

                Content profile = null;
                var profileDomain = Node.LoadNode(uDomainPath);
                if (profileDomain == null)
                {
                    //create domain if not present
                    var domName = this.Domain ?? ContentRepository.Domain.BuiltInDomainName;
                    var dom = Content.CreateNew("ProfileDomain", profiles, domName);

                    dom.DisplayName = domName;
                    dom.Save();

                    profileDomain = dom.ContentHandler;
                }

                if (template == null)
                    template = ContentTemplate.GetTemplate("UserProfile");

                if (template == null)
                {
                    profile = Content.CreateNew("UserProfile", profileDomain, GetProfileName());
                }
                else
                {
                    var profNode = ContentTemplate.CreateFromTemplate(profileDomain, template, GetProfileName());
                    if (profNode != null)
                        profile = Content.Create(profNode);
                }

                if (profile != null)
                {
                    try
                    {
                        //profile["CreatedBy"] = this;
                        profile.ContentHandler.CreatedBy = this;
                        profile.ContentHandler.NodeCreatedBy = this;
                        profile.DisplayName = this.Name;
                        profile.Save();
                    }
                    catch (Exception ex)
                    {
                        //error during user profile creation
                        Logger.WriteException(ex);
                    }
                }
            }
        }

        public bool IsProfileExist()
        {
            return Repository.UserProfilesEnabled && Node.Exists(GetProfilePath());
        }

	    //=================================================================================== IUser Members

		public bool IsInGroup(IGroup group)
		{
			return Security.IsInGroup(group.Id);
		}
        public bool IsInOrganizationalUnit(IOrganizationalUnit orgUnit)
        {
            return Security.IsInGroup(orgUnit.Id);
        }
        public bool IsInContainer(ISecurityContainer container)
        {
            return Security.IsInGroup(container.Id);
        }

        public string Password
		{
            set { _password = value; }
		}
        public MembershipExtension MembershipExtension { get; set; }

        //=================================================================================== 
        
        public List<int> GetPrincipals()
        {
            return Security.GetPrincipals();
        }

		//=================================================================================== IIdentity Members

		string System.Security.Principal.IIdentity.AuthenticationType
		{
			get { return "Portal"; }
		}
		bool System.Security.Principal.IIdentity.IsAuthenticated
		{
			get
			{
				if (this.Id == Visitor.Id || this.Id == 0) return false;
				return true;
			}
		}
		string System.Security.Principal.IIdentity.Name
		{
			get
			{
				return Username;
			}
		}

        private void SaveCurrentPassword()
        {
            var oldPasswords = this.GetOldPasswords();
            if (oldPasswords != null && oldPasswords.Count > 0)
            {
                // set oldpasswords if last password does not equal to current password
                if (oldPasswords.OrderBy(k => k.ModificationDate).Last().Hash != this.PasswordHash)
                    oldPasswords.Add(new PasswordField.OldPasswordData { ModificationDate = DateTime.Now, Hash = this.PasswordHash });
            }
            else
            {
                if (this.PasswordHash != null)
                {
                    oldPasswords = new List<PasswordField.OldPasswordData>();
                    oldPasswords.Add(new PasswordField.OldPasswordData { ModificationDate = DateTime.Now, Hash = this.PasswordHash });
                }
            }

            var passwordHistoryFieldMaxLength = RepositoryConfiguration.PasswordHistoryFieldMaxLength;
            while (passwordHistoryFieldMaxLength + 1 < oldPasswords.Count)
                oldPasswords.RemoveAt(0);

            this.SetOldPasswords(oldPasswords);

        }

        public override void Save(NodeSaveSettings settings)
        {
            // Check uniqueness first
            CheckUniqueUser();

            Domain = GenerateDomain();

            var originalId = this.Id;

            // save current password to the list of old passwords
            this.SaveCurrentPassword();

            base.Save(settings);

            // AD Sync
            SynchUser(originalId);

            // set creator for performant self permission setting
            // creator of the user will always be the user itself. this way setting permissions to the creators group on /Root/IMS will be adequate for user permissions
            // if you need the original creator, use the auditlog
            if (originalId == 0)
            {
                this.CreatedBy = this;
                this.NodeCreatedBy = this;
                base.SaveSameVersion();
            }
            
            // create profiles
            if (originalId == 0 && Repository.UserProfilesEnabled)
                CreateProfile();
        }

        private string GenerateDomain()
        {
            var cutImsPath = Path.Substring(Repository.ImsFolderPath.Length + 1);

            return cutImsPath.Substring(0, cutImsPath.IndexOf('/'));
        }

        private void SynchUser(int originalId)
        {
            if (_syncObject)
            {
                var ADProvider = DirectoryProvider.Current;
                if (ADProvider != null)
                {
                    if (originalId == 0)
                        ADProvider.CreateNewADUser(this, _password);
                    else
                        ADProvider.UpdateADUser(this, this.Path, _password);
                }
            }
            // default: object should be synced. if it was not synced now (sync properties updated only) next time it should be.
            _syncObject = true;
        }

        private void CheckUniqueUser()
        {
            var path = Path;

            if (!path.StartsWith(string.Concat(Repository.ImsFolderPath, RepositoryPath.PathSeparator)) || Parent.Path == Repository.ImsFolderPath)
            {
                throw new InvalidOperationException("Invalid path: user nodes can only be saved under a /Root/IMS/[DomainName] folder.");
            }

            string domainPath = path.Substring(0, Repository.ImsFolderPath.Length + 1 + path.Substring(Repository.ImsFolderPath.Length + 1).IndexOf('/') + 1);

            //We validate here the uniqueness of the user. The constraint is the user name itself and that in Active Directory
            //there must not exist two users and/or groups with the same name under a domain. Organizational units may have
            //the same name as a user.

            //CONDITIONAL EXECUTE
            IEnumerable<int> identifiers;
            int count;
            if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
            {
                var query = new NodeQuery();
                var nameExpression = new StringExpression(StringAttribute.Name, StringOperator.Equal, Name);
                var pathExpression = new StringExpression(StringAttribute.Path, StringOperator.StartsWith, domainPath);
                var orTypes = new ExpressionList(ChainOperator.Or);
                orTypes.Add(new TypeExpression(ActiveSchema.NodeTypes["User"]));
                orTypes.Add(new TypeExpression(ActiveSchema.NodeTypes["Group"]));

                query.Add(pathExpression);
                query.Add(nameExpression);
                query.Add(orTypes);
                var result = query.Execute();
                identifiers = result.Identifiers;
                count = result.Count;
            }
            else
            {
                var nodes = NodeQuery.QueryNodesByTypeAndPathAndName(new List<NodeType> { ActiveSchema.NodeTypes["User"], ActiveSchema.NodeTypes["Group"] }, false, domainPath, false, Name).Nodes;

                var nodeList = nodes as NodeList<Node>;
                if (nodeList != null)
                {
                    identifiers = nodeList.GetIdentifiers();
                    count = nodeList.Count;
                }
                else
                {
                    identifiers = nodes.Select(x => x.Id);
                    count = identifiers.Count();
                }
            }

            if (count > 1 || (count == 1 && identifiers.First() != this.Id))
            {
                var ids = String.Join(", ", (from x in identifiers select x.ToString()).ToArray());
                throw GetUniqueUserException(domainPath, ids);
            }
        }
        private Exception GetUniqueUserException(string domainPath, string ids)
        {
            return new InvalidOperationException(String.Concat(
                "There is/are already user(s) called {"
                , Name
                , "} under the domain {"
                , domainPath
                , "} (NodeId(s): {"
                , ids
                , "}). The 'Domain + Name' must be unique. {"
                , Path
                , "} cannot be saved."));
        }

        public override void ForceDelete()
        {
            base.ForceDelete();

            // AD Sync
            var ADProvider = DirectoryProvider.Current;
            if (ADProvider != null)
            {
                ADProvider.DeleteADObject(this);
            }
        }

        public override bool IsTrashable
        {
            get
            {
                return false;
            }
        }

        public override void MoveTo(Node target)
        {
            base.MoveTo(target);

            // AD Sync
            var ADProvider = DirectoryProvider.Current;
            if (ADProvider != null)
            {
                ADProvider.UpdateADUser(this, RepositoryPath.Combine(target.Path, this.Name), _password);
            }
        }

        //=================================================================================== Events
        protected override void OnMoving(object sender, SenseNet.ContentRepository.Storage.Events.CancellableNodeOperationEventArgs e)
        {
            // AD Sync check
            var ADProvider = DirectoryProvider.Current;
            if (ADProvider != null)
            {
                var targetNodePath = RepositoryPath.Combine(e.TargetNode.Path, this.Name);
                var allowMove = ADProvider.AllowMoveADObject(this, targetNodePath);
                if (!allowMove)
                {
                    e.CancelMessage = "Moving of synced nodes is only allowed within AD server bounds!";
                    e.Cancel = true;
                }
            }

            base.OnMoving(sender, e);
        }

        //=================================================================================== IADSyncable Members
        public void UpdateLastSync(Guid? guid)
        {
            if (guid.HasValue)
                this["SyncGuid"] = ((Guid)guid).ToString();
            this["LastSync"] = DateTime.Now;

            // update object without syncing to AD
            _syncObject = false;

            this.Save();
        }

		//=================================================================================== Generic Property handlers

		public override object GetProperty(string name)
		{
			switch (name)
			{
				case "Enabled":
					return this.Enabled;
				case "Email":
					return this.Email;
				case "FullName":
					return this.FullName;
				case "PasswordHash":
					return this.PasswordHash;
                case "Domain":
                    return this.Domain;
                case OLDPASSWORDS:
                    return this.OldPasswords;
                default:
					return base.GetProperty(name);
			}
		}
		public override void SetProperty(string name, object value)
		{
            switch (name)
            {
				case "Enabled":
					this.Enabled = (bool)value;
					break;
				case "Email":
					this.Email = (string)value;
					break;
				case "FullName":
					this.FullName = (string)value;
					break;
				case "PasswordHash":
					this.PasswordHash = (string)value;
					break;
                case "Domain":
                    this.Domain = (string)value;
                    break;
                case OLDPASSWORDS:
                    this.OldPasswords = (string)value;
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
		}

	}
}
