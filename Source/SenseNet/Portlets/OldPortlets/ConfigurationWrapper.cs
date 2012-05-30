using System;
using SenseNet.ContentRepository.Storage;
using SNC = SenseNet.ContentRepository;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.Portlets
{
    /// <summary>
    /// This class provides an accessing mechanizm to the frequently used field content properties.
    /// </summary>
    internal class ConfigurationWrapper
    {
        private readonly SNC.Content _configContent;

        // Constructors ////////////////////////////////////////////
        public ConfigurationWrapper(SNC.Content configContent)
        {
            if (configContent == null) 
                throw new ArgumentNullException("configContent");
            
            _configContent = configContent;
        }

        public ConfigurationWrapper(string path)
        {
            if (String.IsNullOrEmpty(path)) 
                throw new ArgumentException("path");

            _configContent = SNC.Content.Load(path);
        }

        // Properteies /////////////////////////////////////////////
        public string DefaultDomainPath
        {
            get
            {
                var domainValue = (IEnumerable<Node>)_configContent["DefaultDomainPath"];
                return domainValue.First().Path;
            }
        }
        public string DefaultDomainName
        {
            get
            {
                var domainValue = (IEnumerable<Node>)_configContent["DefaultDomainPath"];
                return domainValue.First().Name;
            }
        }
        public IEnumerable<Node> SecurityGroups
        {
            get { return _configContent["SecurityGroups"] as IEnumerable<Node>; }
        }
        public string NewRegistrationContentView
        {
            get { return _configContent.Fields["NewRegistrationContentView"].GetData() as string; }
        }
        public string UserTypeName
        {
            get { return _configContent.Fields["UserTypeName"].GetData() as string; }
        }
        public string RegistrationSuccessTemplate
        {
            get { return _configContent.Fields["RegistrationSuccessTemplate"].GetData() as string; }
        }
        public string DuplicateErrorMessage
        {
            get
            {
                // return(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "DuplicateErrorMessage") as string);
                return _configContent.Fields["DuplicateErrorMessage"].GetData() as string;
            }
        }
        public string MailSubjectTemplate
        {
            get { return _configContent.Fields["MailSubjectTemplate"].GetData() as string; }
        }
        public string MailFrom
        {
            get { return _configContent.Fields["MailFrom"].GetData() as string; }
        }
        public string ActivationEmailTemplate
        {
            get { return _configContent.Fields["ActivationEmailTemplate"].GetData() as string; }
        }
        public string ChangePasswordUserInterfacePath
        {
            get { return _configContent.Fields["ChangePasswordUserInterfacePath"].GetData() as string; }
        }
        public string ForgottenPasswordUserInterfacePath
        {
            get { return _configContent.Fields["ForgottenPasswordUserInterfacePath"].GetData() as string; }
        }
        public bool ActivationEnabled
        {
            get
            {
                return Convert.ToBoolean(_configContent.Fields["ActivationEnabled"].GetData());
            }
        }
        public bool IsBodyHtml
        {
            get
            {
                return Convert.ToBoolean(_configContent.Fields["IsBodyHtml"].GetData());
            }
        }
        public bool DisableCreatedUser
        {
            get
            {
                return Convert.ToBoolean(_configContent.Fields["DisableCreatedUser"].GetData());
            }
        }
        public string ActivationSuccessTemplate
        {
            get
            {
                // return(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "ActivationSuccessful") as string);
                return _configContent.Fields["ActivationSuccessTemplate"].GetData() as string;
            }
        }
        public string ResetPasswordSubjectTemplate
        {
            get { return _configContent.Fields["ResetPasswordSubjectTemplate"].GetData() as string; }
        }
        public string ResetPasswordTemplate
        {
            get { return _configContent.Fields["ResetPasswordTemplate"].GetData() as string; }
        }
        public string EditProfileContentView
        {
            get { return _configContent.Fields["EditProfileContentView"].GetData() as string; }
        }
        public string AlreadyActivatedMessage
        {
            get
            {
                // return(HttpContext.GetGlobalResourceObject("PublicRegistrationPortlet", "AlreadyActivated") as string);
                return _configContent.Fields["AlreadyActivatedMessage"].GetData() as string;
            }
        }
        public string ResetPasswordSuccessfulTemplate
        {
            get { return _configContent.Fields["ResetPasswordSuccessfulTemplate"].GetData() as string; }
        }
        public string ChangePasswordSuccessfulMessage
        {
            get { return _configContent.Fields["ChangePasswordSuccessfulMessage"].GetData() as string; }
        }
        public string ChangePasswordPagePath
        {
            get
            {
                var refs = _configContent["ChangePasswordPagePath"] as IEnumerable<Node>;
                var node = refs.FirstOrDefault();
                if (node == null)
                    return string.Empty;
                return node.Path;
            }
        }
        public string ChangePasswordRestrictedText
        {
            get { return _configContent.Fields["ChangePasswordRestrictedText"].GetData() as string; }
        }
        public string AlreadyRegisteredUserMessage
        {
            get { return _configContent["AlreadyRegisteredUserMessage"] as string; }
        }
        public string UpdateProfileSuccessTemplate
        {
            get { return _configContent["UpdateProfileSuccessTemplate"] as string; }
        }
        public string EmailNotValid
        {
            get { return _configContent["EmailNotValid"] as string; }
        }
        public string NoEmailGiven
        {
            get { return _configContent["NoEmailGiven"] as string; }
        }
        public IEnumerable<Node> ActivateAdmins
        {
            get
            {
                return _configContent["ActivateAdmins"] as IEnumerable<Node>;
            }
        }
        public bool ActivateByAdmin
        {
            get
            {
                return Convert.ToBoolean(_configContent.Fields["ActivateByAdmin"].GetData());
            }
        }
        public string ActivateEmailTemplate
        {
            get { return _configContent.Fields["ActivateEmailTemplate"].GetData() as string; }
        }
        public string ActivateEmailSubject
        {
            get { return _configContent.Fields["ActivateEmailSubject"].GetData() as string; }
        }
        public bool IsUniqueEmail
        {
            get
            {
                var isUniqueEmail = _configContent.Fields["IsUniqueEmail"].GetData() as string;
                bool result;
                var parseSuccess = Boolean.TryParse(isUniqueEmail, out result);
                return parseSuccess ? result : true;   // default is true
            }
        }

    }
}
