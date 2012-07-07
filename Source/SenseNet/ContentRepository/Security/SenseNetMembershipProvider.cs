using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Security.ADSync;

namespace SenseNet.ContentRepository.Security
{
    public class SenseNetMembershipProvider : MembershipProvider
    {
        public SenseNetMembershipProvider()
        {
            Logger.WriteInformation("MembershipProvider instantitaed: " + typeof(SenseNetMembershipProvider).FullName);
        }

        private string _path = null;
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(name))
                name = "SenseNetMembershipProvider";
            if (!String.IsNullOrEmpty(config["path"]))
                _path = config["path"];
            config.Remove("path");

            base.Initialize(name, config);

            Logger.WriteVerbose("SenseNetMembershipProvider initialized.", Logger.GetDefaultProperties, this);
        }


        public override string ApplicationName
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool EnablePasswordReset
        {
            get { return false; }
        }


        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <param name="pageIndex">Index of the page. (currently ignored)</param>
        /// <param name="pageSize">Size of the page. (currently ignored)</param>
        /// <param name="totalRecords">The total records.</param>
        /// NOTE: paging is not yet implemented
        /// <returns></returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();

            //create NodeQuery
            NodeQuery query = new NodeQuery();
            query.Add(new TypeExpression(ActiveSchema.NodeTypes[typeof(User).Name], false));
            if (_path != null)
                query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, _path));
            query.Orders.Add(new SearchOrder(StringAttribute.Name));
            query.PageSize = pageSize;
            query.StartIndex = pageIndex * pageSize + 1;

            //get paged resultlist
            var resultList = query.Execute();
            foreach (Node node in resultList.Nodes)
            {
                User user = (User)node;
                users.Add(GetMembershipUser(user));
            }

            //get total number of users
            totalRecords = resultList.Count;

            return users;
        }



        public override int GetNumberOfUsersOnline()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string GetPassword(string username, string answer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (userIsOnline)
                throw new NotSupportedException("UserIsOnline must be false.");

            int nodeId;

            try
            {
                nodeId = (int)providerUserKey;
            }
            catch (Exception ex) //rethrow
            {
                throw new ArgumentException("Cannot convert the user primary key.", "providerUserKey", ex);
            }

            User user = (User)User.LoadNode(nodeId);

            return new MembershipUser(
                this.Name, //providerName
                user.Name,
                providerUserKey,
                user.Email,
                string.Empty,
                string.Empty,
                user.Enabled,
                !user.Enabled,
                user.CreationDate,
                DateTime.MinValue,
                DateTime.MinValue,
                DateTime.MinValue,
                DateTime.MinValue);
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool UnlockUser(string userName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        [Obsolete("Use RepositoryConfiguration.DefaultDomain instead.", true)]
        protected string DefaultDomain
        {
            get
            {
                return RepositoryConfiguration.DefaultDomain;
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            int indexBackSlash = username.IndexOf("\\");
            string domain =
                indexBackSlash > 0 ? username.Substring(0, indexBackSlash) : RepositoryConfiguration.DefaultDomain;

            username = username.Substring(username.IndexOf("\\") + 1);

            if (string.IsNullOrEmpty(username))
                return false;

            // if forms AD auth is configured, authenticate user with AD
            var ADProvider = DirectoryProvider.Current;
            if (ADProvider != null)
            {
                if (ADProvider.IsADAuthEnabled(domain))
                {
                    return ADProvider.IsADAuthenticated(domain, username, password);
                }
            }

            //we need to load the user with admin account here
            using (new SystemAccount())
            {
                var user = User.Load(domain, username);
                if (user == null || !user.Enabled) 
                    return false;

                return User.CheckPasswordMatch(password, user.PasswordHash);
            }
        }

        private MembershipUser GetMembershipUser(User portalUser)
        {
            MembershipUser membershipUser = new MembershipUser(
                                    Name,                       // Provider name
                                    portalUser.Username,                   // Username
                                    portalUser.Username,                   // providerUserKey
                                    portalUser.Email,                      // Email
                                    String.Empty,               // passwordQuestion
                                    String.Empty,               // Comment
                                    true,                       // isApproved
                                    false,                      // isLockedOut
                                    DateTime.Now,               // creationDate
                                    DateTime.Now,                  // lastLoginDate
                                    DateTime.Now,               // lastActivityDate
                                    DateTime.Now,               // lastPasswordChangedDate
                                    new DateTime(1980, 1, 1)    // lastLockoutDate
                                );
            return membershipUser;
        }
    }
}
