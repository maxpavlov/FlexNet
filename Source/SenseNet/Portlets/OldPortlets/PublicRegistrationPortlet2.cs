using System;
using System.ComponentModel;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.Portlets.Controls;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using SenseNet.Services.Instrumentation;
using SNC = SenseNet.ContentRepository;
using SNP = SenseNet.Portal;
using System.Linq;
using System.Collections.Generic;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Portlets
{
    /// <summary>
    /// Public registration portlet.
    /// Feel free to change it, but consider that there might be some cases when it is only sufficient to override some methods.
    /// TODO: ResetPassword and ForgottenPassword terms should be simplified.
    /// </summary>
    public class PublicRegistrationPortlet : PortletBase
    {
        private const string MailHostAppsettingKey = "SMTP";
        private const string DefaultEmailSenderAppsettingKey = "DefaultEmailSender";

        public enum PortletState
        {
            Registration,       // also known as Join
            ResetPassword,
            UpdateProfile,
            ChangePassword
        }
        // Members and properties ///////////////////////////////////////
        private static readonly Regex CheckGuid = InitCheckGuid();
        private static Regex InitCheckGuid()
        {
            return new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$",RegexOptions.Compiled);
        }       
        
        private SNC.Content _content;
        private ContentView _contentView;
        private string _errorMessage;
        private string _configurationPath;
        private PortletState _portletMode;
        private ConfigurationWrapper Configuration { get; set; }

        private bool HasErrorInternal
        {
            get { return !String.IsNullOrEmpty(_errorMessage); }
        }

        // Personalization properties ///////////////////////////////////
        [WebBrowsable(true), Personalizable(true)]
        [DefaultValue(PortletState.Registration)]
        [WebDisplayName("Portlet mode")]
        [WebDescription("Defines portlet layout. 'Registration' brings up a registration form, 'ResetPassword' and 'ChangePassword' are used for password manipulation and 'UpdateProfile' brings up user's profile in edit mode")]
        [WebCategory(EditorCategory.PublicRegistration, EditorCategory.PublicRegistration_Order)]
        public PortletState PortletMode
        {
            get { return _portletMode; }
            set { _portletMode = value; }
        }
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Configuration path")]
        [WebDescription("Path of the public registration configuration file")]
        [WebCategory(EditorCategory.PublicRegistration, EditorCategory.PublicRegistration_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions()]
        public string ConfigurationPath
        {
            get { return _configurationPath; }
            set { _configurationPath = value; }
        }

        #region yet another hack to change current user to God mode.

        public IUser _originalUser;
        private static bool IsCurrentUserAnAdministrator()
        {
            return AccessProvider.Current.GetCurrentUser().Id == User.Administrator.Id;
        }
        public void ChangeToAdminAccount()
        {
            _originalUser = AccessProvider.Current.GetCurrentUser();
            if (IsCurrentUserAnAdministrator() == true)
                return;
            AccessProvider.Current.SetCurrentUser(User.Administrator);
        }
        public void RestoreOriginalUser()
        {
            if (IsCurrentUserAnAdministrator() == false)
                return;
            AccessProvider.Current.SetCurrentUser(_originalUser);
        }

        #endregion

        // Constructors /////////////////////////////////////////////////
        public PublicRegistrationPortlet()
        {
            this.Name = "Public registration";
            this.Description = "Users can register, confirm registration, change profile and password";
            this.Category = new PortletCategory(PortletCategoryType.Portal);
        }


        // Events ///////////////////////////////////////////////////////
        protected override void CreateChildControls()
        {
            this.Controls.Clear();

			// read configuration setting from Repository
            var isConfigurationRead = ReadConfiguration();
            if (!isConfigurationRead)
            {
                if (HasErrorInternal)
                    WriteErrorMessageOnly(this._errorMessage);
                else
                    WriteErrorMessageOnly(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "NoConfiguration") as string);

                return;
            }
            var isProcessActivation = ProcessActivation();
            if (isProcessActivation)
                return;

            if (this.HasErrorInternal)  // any error occured during processing activation?
            {
                this._errorMessage = string.Empty;
                return;
            }

            switch (this.PortletMode)
            {
                case PortletState.Registration:
                    ProcessRegistration();
                    break;
                case PortletState.ResetPassword:
                    ProcessResetPassword();
                    break;
                case PortletState.UpdateProfile:
                    ProcessUpdateProfile();
                    break;
                case PortletState.ChangePassword:
                    ProcessChangePassword();
                    break;
                default:
                    break;
            }

            ChildControlsCreated = true;

        }
        
        protected void ContentView_UserAction_Update(object sender, UserActionEventArgs e)
        {
            OnUserAction(e, false);
            if (e.ContentView.IsUserInputValid && e.ContentView.Content.IsValid)
                WriteMessage(Configuration.UpdateProfileSuccessTemplate);

        }
        protected void ContentView_UserAction_New(object sender, UserActionEventArgs e)
        {
            OnUserAction(e, true);
        }
        protected void ContentHandler_Created(object sender, NodeEventArgs e)
        {
            OnCreatedUser(e);
        }
        protected void ResetPassword_Click(object sender, EventArgs e)
        {
            ResetPasswordHandler(sender);
        }
        protected void ChangePassword_Click(object sender, EventArgs e)
        {
            ChangePasswordHandler(sender);
        }

        protected virtual void OnCreatedUser(NodeEventArgs e)
        {
            this.Controls.Clear();  // we must work with clean "screen" :)
            //
            //  TODO: implement template usage (only simple text is used yet.)
            //
            try
            {
                SendActivationEmail(e);
                Controls.Add(new LiteralControl(Configuration.RegistrationSuccessTemplate));
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                e.SourceNode.Delete();
                WriteErrorMessageOnly(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "ActivationEmailError") as string);
            }
        }
        protected virtual void OnUserAction(UserActionEventArgs e, bool createNew)
        {
            var actionName = e.ActionName.ToLower();
            var actionName2 = Enum.GetName(typeof(ActionNames), ActionNames.Save).ToLower();
            if (!actionName.Equals(actionName2, StringComparison.InvariantCulture))
                return;
            
            ChangeToAdminAccount();

            if (createNew)
                SetDefaultDomainValue(e);

            e.ContentView.UpdateContent();
            if (e.ContentView.IsUserInputValid && _content.IsValid)
            {
                if (createNew && ChecksDuplicatedUser(e))
                {
                    WriteDuplactedUserErrorMessage();
                    
                    RestoreOriginalUser();
                    
                    return;
                }

                SaveUser(e, createNew);
            }
            this.RestoreOriginalUser();
        }

        // Internals ////////////////////////////////////////////////////
        private void SaveUser(UserActionEventArgs e, bool createNew)
        {
            try
            {
                if (createNew)
                    GenerateActivationId();

                if (!Configuration.ActivationEnabled)
                    EnableAndActivateUser();

                _content.Save();

                if (createNew)
                    AddToSecurityGroups(e);
            }
            catch (InvalidOperationException ex) //logged
            {
                Logger.WriteException(ex);
                //TODO: Biztos, hogy UserAlreadyExists?
                WriteErrorMessageOnly(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "UserAlreadyExists") as string);
            }
        }
        private void ProcessUpdateProfile()
        {
            _content = SNC.Content.Load(User.Current.Id);
            if (_content == null)
                return; // todo: could not load current user content.
            var currentUserNodeTypeName = ((Node)User.Current).NodeType.Name;
            if (currentUserNodeTypeName != Configuration.UserTypeName)
            {
                this.Controls.Clear();
                this.Controls.Add(new LiteralControl(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "UserTypeDoesNotMatch") as string));
                return; // todo: update only the correct UserType. It prevents that user updates his/her profile with another user type inlinenew contentview.
            }
            this.ChangeToAdminAccount();
            try
            {
                _contentView = String.IsNullOrEmpty(Configuration.EditProfileContentView)
                    ? ContentView.Create(this._content, this.Page, ViewMode.InlineEdit)
                    : ContentView.Create(this._content, this.Page, ViewMode.InlineEdit, Configuration.EditProfileContentView);
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                WriteErrorMessageOnly(String.Format(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "ErrorCreatingContentView") as string, Configuration.EditProfileContentView));
                this.RestoreOriginalUser();
                return;
            }
            this._contentView.UserAction += new EventHandler<UserActionEventArgs>(ContentView_UserAction_Update);
            this.Controls.Add(this._contentView);
            this.RestoreOriginalUser();

        }
        private void AddToSecurityGroups(UserActionEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            foreach (var n in Configuration.SecurityGroups)
            {
                try
                {
                    var g = Node.Load<SNC.Group>(n.Id);
                    if (g != null && g.Id != RepositoryConfiguration.EveryoneGroupId) // Only add to a group if it is not an empty group or the everyone group
                    {
                        var u = Node.LoadNode(e.ContentView.ContentHandler.Id) as User;
                        if (u != null)
                        {
							g.AddMember(u);
                            g.Save();
                        }
                    }
                }
                catch(Exception ee) //logged
                {
                    Logger.WriteException(ee);
                }
            }
        }
        private void EnableAndActivateUser()
        {
            this._content.Fields["Activated"].SetData(true);
            this._content.Fields["Enabled"].SetData(true);
        }
        private void SetDefaultDomainValue(UserActionEventArgs e)
        {
            if (e == null) 
                throw new ArgumentNullException("e");
            var domain = Node.LoadNode(Configuration.DefaultDomainPath);
            e.ContentView.Content.Fields["Domain"].SetData(domain.Name);
        }
        private void CreateUserChangePasswordUI()
        {
            var changePassword = this.Page.LoadControl(Configuration.ChangePasswordUserInterfacePath) as UserChangePassword;
            if (changePassword == null)
                return;
            changePassword.Click += new EventHandler(ChangePassword_Click);
            this.Controls.Add(changePassword);
        }
        private void ResetPasswordHandler(object sender)
        {
            var control = sender as EmailForgottenPassword;
            var resetEmail = control.ResetEmailAddress;
            
            if (String.IsNullOrEmpty(resetEmail))
                control.Message = Configuration.NoEmailGiven;
            else
            {
                if (SendResetPasswordEmail(resetEmail))
                     control.Message = Configuration.ResetPasswordSuccessfulTemplate;
                else
                     control.Message = Configuration.EmailNotValid;
            }
        }
        private void ChangePasswordHandler(object sender)
        {
            var control = sender as UserChangePassword;
            var pwd = control.NewPassword;
            var newPwd = control.ReenteredNewPassword;

            if (!IsValidPassword(pwd, newPwd, control))
                return;

            var result = ChangeCurrentUserPassword(pwd, newPwd, control);
            if (result)
                control.Message = Configuration.ChangePasswordSuccessfulMessage;
        }
        private void CheckMailSetting()
        {
            if (System.Web.Configuration.WebConfigurationManager.AppSettings["SMTP"] == null)
                throw new InvalidOperationException("SMTP section is not defined in the web.config.");

            if (String.IsNullOrEmpty(Configuration.MailSubjectTemplate))
                throw new InvalidOperationException("Couldn't send email without subject.");
        }
        private void SendActivateByAdminEmails(IEnumerable<Node> listOfAdmins, Node registeredUser)
        {
            if (registeredUser == null) 
                throw new ArgumentNullException("registeredUser");

            var registeredUserPath = registeredUser.Path;

            foreach (var adminNodeUser in listOfAdmins)
            {
                var adminUser = adminNodeUser as User;
                if (adminUser == null)
                    continue;   // perhaps it's a trouble

                SendActivateByAdminEmailInternal(adminUser, registeredUserPath);
            }
        }

        private void SendActivateByAdminEmailInternal(User adminUser, string registeredUserPath)
        {
            if (adminUser == null)
                throw new ArgumentNullException("adminUser");
            
            if (String.IsNullOrEmpty(registeredUserPath)) 
                throw new ArgumentException("registeredUserPath");

            CheckMailSetting();

            var mailMessage = GetActivateByAdminMailMessage(adminUser, registeredUserPath);

            if (mailMessage == null)
                return; 

            mailMessage.Priority = MailPriority.Normal;

            SendEmail(mailMessage);
        }
        
        private bool GetUserChangePasswordState()
        {
            User u = GetUserFromQueryString();
            if (u == null)
                return false; // couldn't find user or querystring contains incorrect values or key does not match with the stored key of the user.

            var fullName = String.Concat(Configuration.DefaultDomainName, "\\", u.Name);

            FormsAuthentication.SetAuthCookie(fullName, true);
            CreateUserChangePasswordUI();
            return true;
        }     
        private string GetDomainName()
        {
            var d = Node.LoadNode(Configuration.DefaultDomainPath) as Domain;
            return d == null ? string.Empty : d.Name;
        }
        private string GetChangePasswordUrl(SNP.Page changePwdPage, string siteRepositoryPath)
        {
            string changePwdPageUrl;
            if (string.IsNullOrEmpty(changePwdPage.SmartUrl))
            {
                changePwdPageUrl = Configuration.ChangePasswordPagePath;
                changePwdPageUrl = changePwdPageUrl.Replace(VirtualPathUtility.AppendTrailingSlash(siteRepositoryPath), string.Empty);
            }
            else
                changePwdPageUrl = changePwdPage.SmartUrl;
            changePwdPageUrl = String.Concat(VirtualPathUtility.AppendTrailingSlash(PortalContext.Current.SiteUrl), changePwdPageUrl);
            return changePwdPageUrl;
        }
        private string GetMailFromValue()
        {
            if (!String.IsNullOrEmpty(Configuration.MailFrom))
                return Configuration.MailFrom;
            var from = System.Web.Configuration.WebConfigurationManager.AppSettings[DefaultEmailSenderAppsettingKey];
            if (!String.IsNullOrEmpty(from))
                return from;
            throw new ApplicationException("Configuration.MailFrom is not configured");
        }
        
        private SNC.Content CreateNewUserContent(string parentPath)
        {
            if (parentPath == null)
                throw new ArgumentNullException("parentPath");

            SNC.Content result = null;

            // load parent node
            this.ChangeToAdminAccount();

            Node parent = Node.LoadNode(parentPath);

            if (parent == null)
            {
                //HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "DuplicateErrorMessage") as string
                _errorMessage = String.Format(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "ErrorLoadingContent") as string, parentPath);
                this.RestoreOriginalUser();
                return null;
            }

            // create new empty registered user content
            try
            {
                result = SNC.Content.CreateNew(Configuration.UserTypeName, parent, Guid.NewGuid().ToString());
                if (result == null)
                {
                    WriteErrorMessageOnly(String.Format(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "ErrorCreatingUser") as string, parentPath));
                    this.RestoreOriginalUser();
                    return null;
                }
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                WriteErrorMessageOnly(String.Format(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "ErrorCreatingUser") as string, parentPath));
                this.RestoreOriginalUser();
                return null;
            }

            this.RestoreOriginalUser();
            return result;
        }
        private User GetRegisteredUser(string resetEmail, string domain)
        {
            if (String.IsNullOrEmpty(resetEmail))
                throw new ArgumentNullException("resetEmail");
            if (String.IsNullOrEmpty(domain))
                throw new ArgumentNullException("domain");

            var query = new NodeQuery();
            var expressionList = new ExpressionList(ChainOperator.And);
            expressionList.Add(new TypeExpression(ActiveSchema.NodeTypes[Configuration.UserTypeName], false));
            expressionList.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, string.Concat(Repository.ImsFolderPath, RepositoryPath.PathSeparator, domain, RepositoryPath.PathSeparator)));
            expressionList.Add(new StringExpression(ActiveSchema.PropertyTypes["Email"], StringOperator.Equal, resetEmail));
            query.Add(expressionList);
            AccessProvider.ChangeToSystemAccount();
            var resultList = query.Execute();
            AccessProvider.RestoreOriginalUser();

            // no user has beeen found
            if (resultList.Count == 0)
                return null;

            var u = resultList.Nodes.First() as User;
            return u;
        }
        private Node GetUserByActivationId()
        {
            var activationId = HttpContext.Current.Request.Params["ActivationId"];

            if (String.IsNullOrEmpty(activationId))
                return null;

            if (!IsGuid(activationId))
                return null;

            AccessProvider.ChangeToSystemAccount();

            List<Node> result = GetUserByActivationIdInternal(activationId);

            AccessProvider.RestoreOriginalUser();

            if (result.Count == 0)
            {
                WriteAlreadyActivatedMessage();
                return null;    // write message: user is already activated
            }

            return result.First();
        }
        private List<Node> GetUserByActivationIdInternal(string activationId)
        {
            var query = new NodeQuery();
            query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, "/Root/IMS"));
            query.Add(new TypeExpression(ActiveSchema.NodeTypes[Configuration.UserTypeName]));
            query.Add(new StringExpression(ActiveSchema.PropertyTypes["ActivationId"], StringOperator.Equal, activationId));
            return query.Execute().Nodes.ToList();
        }
        private MailMessage GetActivationMailMessage()
        {
            var mailFrom = GetMailFromValue();
            var mailTo = this._content.Fields["Email"].GetData() as string;
            var mailSubject = Configuration.MailSubjectTemplate;
            var mailDefinition = Configuration.ActivationEmailTemplate;
            var originalUri = PortalContext.Current.RequestedUri.ToString();
            var activationId = this._content.Fields["ActivationId"].GetData() as string;
            var activationLink = String.Concat(originalUri, "?ActivationId=", activationId);

            // puts the ActivationLink into mail body
            mailDefinition = String.Format(mailDefinition,
                                           activationLink.Replace(HtmlTextWriter.SpaceChar.ToString(), "%20"));

            return new MailMessage(mailFrom, mailTo, mailSubject, mailDefinition);
        }
        private MailMessage GetResetMailMessage(string resetEmail, User u, string resetKeyGuid)
        {
            var mailTo = resetEmail;
            var mailSubject = Configuration.ResetPasswordSubjectTemplate;
            var mailDefinition = Configuration.ResetPasswordTemplate;
            var mailFrom = GetMailFromValue();
            // prepare uri
            var siteRepositoryPath = PortalContext.Current.Site.Path;

            var changePwdPageUrl = string.Empty;

			var changePwdPage = Node.Load<SNP.Page>(Configuration.ChangePasswordPagePath);
            if (changePwdPage != null)
                changePwdPageUrl = GetChangePasswordUrl(changePwdPage, siteRepositoryPath);

            var resetLink = String.Format("{3}://{0}?uid={1}&key={2}", 
                changePwdPageUrl, 
                u.Id, 
                resetKeyGuid, 
                HttpContext.Current.Request.Url.GetComponents(UriComponents.Scheme, UriFormat.SafeUnescaped));

            mailDefinition = String.Format(mailDefinition, resetLink);
            
            return new MailMessage(mailFrom, mailTo, mailSubject, mailDefinition);
        }
        private MailMessage GetActivateByAdminMailMessage(User adminUser, string registeredUserPath)
        {
            if (String.IsNullOrEmpty(registeredUserPath)) 
                throw new ArgumentException("registeredUserPath");

            try
            {
                var mailFrom = GetMailFromValue();
                var mailTo = adminUser.Email;
                var mailSubject = Configuration.ActivateEmailSubject;
                var mailDefinition = Configuration.ActivateEmailTemplate;
                mailDefinition = String.Format(mailDefinition, registeredUserPath);

                return new MailMessage(mailFrom, mailTo, mailSubject, mailDefinition);
                
            } 
            catch(Exception exc) //logged
            {
                Logger.WriteException(exc);
            }
            return null;
        }
        private User GetUserFromQueryString()
        {
            var sUid = HttpContext.Current.Request.Params["uid"];
            var uid = String.IsNullOrEmpty(sUid) ? 0 : Convert.ToInt32(sUid); ;
            var key = HttpContext.Current.Request.Params["key"] as string;

            if (uid == 0 || String.IsNullOrEmpty(key))
                return null;

            // loads the user by uid
            this.ChangeToAdminAccount();
            var u = Node.LoadNode(uid) as User;
            if (u == null)
            {
                this.RestoreOriginalUser();
                return null;
            }

			var uResetKey = u.GetProperty<string>("ResetKey");
			if (String.IsNullOrEmpty(uResetKey))
                return null;

            if (!uResetKey.Equals(key))
            {
                this.RestoreOriginalUser();
                return null;
            }
            u["ResetKey"] = string.Empty;
            u.Save();
            this.RestoreOriginalUser();

            return u;
        }
        
        // Virtuals /////////////////////////////////////////////////////
        protected virtual void WriteMessage(string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            this.Controls.Add(new LiteralControl(message));
        }
        protected virtual void WriteErrorMessageOnly(string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            this.Controls.Clear();
            this.Controls.Add(new LiteralControl(message));
        }
        protected virtual void WriteErrorMessageInsideView(string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            HtmlGenericControl errMsg = null;

            if (this._contentView != null)
                errMsg = this._contentView.FindControl("CustomErrMsg") as HtmlGenericControl;
            else
                errMsg = this.FindControl("CustomErrMsg") as HtmlGenericControl; // tries to find it in another naming container

            if (errMsg != null) errMsg.InnerText = message;
        }
        protected virtual void WriteAlreadyActivatedMessage()
        {
            this.Controls.Clear();
            _errorMessage = Configuration.AlreadyActivatedMessage;
            this.Controls.Add(new LiteralControl(_errorMessage));
        }
        protected virtual void WriteDuplactedUserErrorMessage()
        {
            WriteErrorMessageInsideView(Configuration.DuplicateErrorMessage);
        }
        protected virtual void WriteActivationSuccessfull()
        {
            this.Controls.Clear(); // the message is the only text on the page, so we clears out the ctlcoll.
            this.Controls.Add(new LiteralControl(Configuration.ActivationSuccessTemplate));
        }

        protected virtual bool ProcessActivation()
        {
            var registeredUser = GetUserByActivationId();
            if (registeredUser == null)
                return false;

            registeredUser["ActivationId"] = string.Empty;
            registeredUser["Activated"] = 1;
            
            var activateByAdmin = Configuration.ActivateByAdmin;
            if (activateByAdmin)
            {
                // FIX: BUG 1829
                try
                {
                    ProcessActivateByAdmin(registeredUser);    
                } catch(Exception exception)
                {
                    Logger.WriteException(exception);
                    Logger.WriteInformation("Couldn't send Activation email to administrator. Check the log.");
                }
            } else
                registeredUser["Enabled"] = Configuration.DisableCreatedUser ? 0 : 1;

            this.ChangeToAdminAccount();
            try
            {
                registeredUser.Save();
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                WriteErrorMessageOnly(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "ErrorWhileSaving") as string);
                this.RestoreOriginalUser();
                return true;
            }

            WriteActivationSuccessfull();

            this.RestoreOriginalUser();
            return true;
        }
        protected virtual void ProcessActivateByAdmin(Node registeredUser)
        {
            if (registeredUser == null) 
                throw new ArgumentNullException("registeredUser");

            var listOfAdmins = Configuration.ActivateAdmins;
            if (listOfAdmins.ToList().Count == 0) 
                return;
            
            SendActivateByAdminEmails(listOfAdmins, registeredUser);
        }
        
        protected virtual void ProcessRegistration()
        {
            if (User.Current.Id != User.Visitor.Id)
            {
                WriteErrorMessageOnly(Configuration.AlreadyRegisteredUserMessage);
                return;
            }

            // creates an empty user
            _content = CreateNewUserContent(Configuration.DefaultDomainPath);
            if (_content == null)
                return;
            _content.ContentHandler.Created += ContentHandler_Created;
            // loads the publicregistration contentviews 
            // and exception raises when registration contentview has not been found.)
            ChangeToAdminAccount();
            try
            {
                _contentView = String.IsNullOrEmpty(Configuration.NewRegistrationContentView)
                    ? ContentView.Create(_content, Page, ViewMode.InlineNew)
                    : ContentView.Create(_content, Page, ViewMode.New, Configuration.NewRegistrationContentView);
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                WriteErrorMessageOnly(String.Format(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "ErrorCreatingContentView") as string, Configuration.NewRegistrationContentView));
                RestoreOriginalUser();
                return;
            }
            _contentView.UserAction += ContentView_UserAction_New;
            Controls.Add(_contentView);
            RestoreOriginalUser();
        }
        protected virtual void ProcessResetPassword()
        {
            var resetPasswordUI = this.Page.LoadControl(Configuration.ForgottenPasswordUserInterfacePath) as EmailForgottenPassword;
            if (resetPasswordUI == null) 
                return; 

            resetPasswordUI.Click += new EventHandler(ResetPassword_Click);
            Controls.Add(resetPasswordUI);
        }        
        protected virtual void ProcessChangePassword()
        {
            if (User.Current.IsAuthenticated)
            {
                CreateUserChangePasswordUI();
            }
            else
            {
                if (!GetUserChangePasswordState())
                    WriteMessage(Configuration.ChangePasswordRestrictedText);
            }
        }    
        protected virtual void GenerateActivationId()
        {
            var activationId = Guid.NewGuid();
            this._content.Fields["ActivationId"].SetData(activationId.ToString());
        }
        protected virtual void SendActivationEmail(NodeEventArgs e)
        {
            if (!Configuration.ActivationEnabled)
                return;

            CheckMailSetting();

            MailMessage mailMessage = GetActivationMailMessage();

            if (Configuration.IsBodyHtml)
                mailMessage.IsBodyHtml = true;

            mailMessage.Priority = MailPriority.Normal;

            SendEmail(mailMessage);
        }

        protected virtual bool SendResetPasswordEmail(string resetEmail)
        {
            var resetKeyGuid = Guid.NewGuid().ToString();
            var domainName = GetDomainName();
            
            var u = GetRegisteredUser(resetEmail, domainName);
            if (u == null)
                return false;

            try
            {
                var activatedValue = u["Activated"];
                var activated = Convert.ToInt32(activatedValue);
                if (activated != 1)
                    return false;

                u["ResetKey"] = resetKeyGuid;

                this.ChangeToAdminAccount();
                u.Save();
                this.RestoreOriginalUser();
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                WriteErrorMessageOnly(ex.Message);
                return false;
            }

            // puts the ResetLink into mail body
            try
            {
                MailMessage mailMessage = GetResetMailMessage(resetEmail, u, resetKeyGuid);
                SendEmail(mailMessage);
            }
            catch (Exception exc) //logged
            {
                // It seems, the mailDefinition does not contain the proper formatting string.
                Logger.WriteException(exc);
                WriteErrorMessageOnly(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "EmailSendError") as string);
                return false;
            }

            return true;

        }
        protected virtual bool ChecksDuplicatedUser(UserActionEventArgs e)
        {
            if (Configuration.IsUniqueEmail)
            {
                var query = new NodeQuery();
                query.Add(new StringExpression(PropertyType.GetByName("Email"),StringOperator.Equal,(string)e.ContentView.Content.Fields["Email"].GetData()));
                var result = query.Execute();
                return result.Count > 0 ? true : false;
            }
            var path = e.ContentView.Content.ContentHandler.ParentPath;
            var newUserName = e.ContentView.Content.Fields["Name"].GetData() as string;
            var t = RepositoryPath.Combine(VirtualPathUtility.AppendTrailingSlash(path), newUserName);
            return NodeHead.Get(t) != null;
        }
        protected virtual bool ChangeCurrentUserPassword(string pwd, string newPwd, UserChangePassword control)
        {
            if (pwd.Equals(newPwd))
            {
                var u = User.LoadNode(User.Current.Id) as User;

                #region from changeset #16856

                u.Password = pwd;

                #endregion
                
                u.PasswordHash = User.EncodePassword(pwd);
                this.ChangeToAdminAccount();
                u.Save();
                this.RestoreOriginalUser();
                return true;
            }
            else
                control.Message = HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "PasswordsDontMatch") as string;

            return false;
        }
        protected virtual bool ReadConfiguration()
        {
            if (String.IsNullOrEmpty(this.ConfigurationPath))
                return false;

            //PathInfoRemove:
            //if (RepositoryPathInfo.GetPathInfo(this.ConfigurationPath) == null)
            if (NodeHead.Get(this.ConfigurationPath) == null)
            {
                _errorMessage = String.Format(CultureInfo.InvariantCulture, HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "ConfigSettingNotFound") as string, this.ConfigurationPath);
                return false;
            }
            // Load config settings.
            var _configContent = SNC.Content.Load(this._configurationPath);
            Configuration = new ConfigurationWrapper(_configContent);
            return true;
        }
        
        // Tools ////////////////////////////////////////////////////////
        public void SendEmail(MailMessage mailMessage)
        {
            if (mailMessage == null)
                throw new ArgumentNullException("mailMessage");

            try
            {
                var smtpClient = new SmtpClient();
                smtpClient.Host = System.Web.Configuration.WebConfigurationManager.AppSettings["SMTP"].ToString();
                smtpClient.Send(mailMessage);
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                WriteErrorMessageOnly(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "EmailSendError2") as string);
            }
        }
        
        // Static ///////////////////////////////////////////////////////
        internal static bool IsGuid(string source)
        {
            if (String.IsNullOrEmpty(source))
                throw new ArgumentException("source");
            return CheckGuid.IsMatch(source);
        }
        internal static bool IsValidPassword(string pwd, string newPwd, UserChangePassword control)
        {
            if (String.IsNullOrEmpty(pwd) ||
                String.IsNullOrEmpty(newPwd))
            {
                control.Message = HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "PasswordNotValid") as string;
                return false;
            }
            return true;
        }

    }
}
