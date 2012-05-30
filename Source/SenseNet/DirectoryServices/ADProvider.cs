using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.Xml.Serialization;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Schema;
using System.Threading;
using SenseNet.ContentRepository.Security.ADSync;
using SenseNet.Configuration;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Search;

namespace SenseNet.DirectoryServices
{
    public class ADProvider : DirectoryProvider
    {
        /*=========================================================================== Members */
        private const string _actionPath = "/Root/System/SystemPlugins/Tools/DirectoryServices/Actions";
        private const string _actionName = "ADACTION_{0}.xml";


        /*=========================================================================== Static methods */
        private static string GetExceptionMsg(Exception ex)
        {
            var innerEx = ex.InnerException == null ? string.Empty : string.Concat(Environment.NewLine, "InnerException: ", ex.InnerException.Message, Environment.NewLine, ex.InnerException.StackTrace, Environment.NewLine);
            return string.Concat(ex.Message, Environment.NewLine, ex.StackTrace, Environment.NewLine, innerEx);
        }
        private static void SaveAction(ADAction adAction, File origFile)
        {
            // serialize this object and save it to a node
            var serializer = new XmlSerializer(typeof(ADAction));

            // serialize password?
            var config = Portal2ADConfiguration.Current;
            if (!config.SaveFailedPassword)
                adAction.PassWd = null;

            //save the action in elevated mode
            using (new SystemAccount())
            {
                Common.EnsurePath(_actionPath);

                var failFile = origFile;
                if (failFile == null)
                {
                    failFile = new File(Node.LoadNode(_actionPath));
                    var fileName = string.Format(_actionName, Guid.NewGuid().ToString());
                    failFile.Name = fileName;
                }

                using (System.IO.Stream inputStream = new System.IO.MemoryStream())
                {
                    using (System.IO.TextWriter writer = new System.IO.StreamWriter(inputStream))
                    {
                        serializer.Serialize(writer, adAction);
                        writer.Flush();

                        failFile.Binary.SetStream(inputStream);
                        failFile.Save();
                    }
                }
            }
        }
        public static void RetryAllFailedActions()
        {
            using (new SystemAccount())
            {
                var qs = new QuerySettings {EnableLifespanFilter = false, EnableAutofilters = false};
                var queryText = string.Format("TypeIs:File AND InTree:\"{0}\" .SORT:CreationDate", _actionPath);
                var q = ContentQuery.CreateQuery(queryText, qs);
                var result = q.Execute();

                foreach (Node node in result.Nodes)
                {
                    var failFile = node as File;
                    if (failFile == null)
                        continue;

                    // restore ADAction object
                    var stream = failFile.Binary.GetStream();
                    var serializer = new XmlSerializer(typeof (ADAction));
                    var adAction = serializer.Deserialize(stream) as ADAction;
                    if (adAction == null)
                        continue;

                    try
                    {
                        adAction.Execute();
                        failFile.Delete();
                    }
                    catch (Exception ex) //TODO: catch block
                    {
                        // re-execution failed again, save exception
                        adAction.LastException = GetExceptionMsg(ex);
                        SaveAction(adAction, failFile);
                    }
                }
            }
        }
        private static AuthSettingElement GetAuthADPathFromDomain(string domain)
        {
            var formsADAuthSettings = SenseNet.Configuration.FormsAuthenticationFromADSection.Current.AuthSettings;
            foreach (AuthSettingElement authSetting in formsADAuthSettings)
            {
                if (authSetting.Domain == domain)
                {
                    return authSetting;
                }
            }
            return null;
        }


        /*=========================================================================== Async pattern */
        private static void ProcessAction(ActionType actionType, Node node, string newPath, string passWd)
        {
            var adAction = new ADAction(actionType, node, newPath, passWd);

            // no immediate background async action executed
            // -> user of app pool cannot have elevated AD rights
            var syncPortal2AD = new SyncPortal2AD();
            if (syncPortal2AD.IsSyncedObject(node.Path))
                SaveAction(adAction, null);
        }


        /*=========================================================================== DirectoryProvider methods */
        public override void CreateNewADUser(User user, string passwd)
        {
            ProcessAction(ActionType.CreateNewADUser, user, user.Path, passwd);
        }
        public override void UpdateADUser(User user, string newPath, string passwd)
        {
            ProcessAction(ActionType.UpdateADUser, user, newPath, passwd);
        }
        public override void CreateNewADContainer(Node node)
        {
            ProcessAction(ActionType.CreateNewADContainer, node, node.Path, null);
        }
        public override void UpdateADContainer(Node node, string newPath)
        {
            ProcessAction(ActionType.UpdateADContainer, node, newPath, null);
        }
        public override void DeleteADObject(Node node)
        {
            ProcessAction(ActionType.DeleteADObject, node, null, null);
        }
        public override bool AllowMoveADObject(Node node, string newPath)
        {
            var directoryServices = new SyncPortal2AD();
            return directoryServices.AllowMoveADObject(node, newPath);
        }
        /// <summary>
        /// be van-e kapcsolva hogy portal helyett az AD-val autentikaltassuk a usereket ?
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public override bool IsADAuthEnabled(string domain)
        {
            var authSetting = GetAuthADPathFromDomain(domain);

            if (authSetting == null)
                return false;

            return true;
        }
        /// <summary>
        /// be van-e kapcsolva, hogy az AD-ban autentikált userek a portálon csak virtuálisan vannak jelen?
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public override bool IsVirtualADUserEnabled(string domain)
        {
            var authSetting = GetAuthADPathFromDomain(domain);

            if (authSetting == null)
                return false;

            if (!authSetting.VirtualADUser)
                return false;

            return true;
        }

        /// <summary>
        /// AD-val autentikaljuk a usert
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public override bool IsADAuthenticated(string domain, string username, string pwd)
        {
            var authSetting = GetAuthADPathFromDomain(domain);

            if (authSetting == null)
                return false;

            if (!string.IsNullOrEmpty(authSetting.CustomLoginProperty))
            {
                return DirectoryServices.Common.IsADCustomAuthenticated(authSetting.ADServer, username, pwd, authSetting.CustomLoginProperty, authSetting.CustomADAdminAccountName, authSetting.CustomADAdminAccountPwd);
            }
            else
            {
                return DirectoryServices.Common.IsADAuthenticated(authSetting.ADServer, domain, username, pwd, AuthSettingElement.LoginProperty);
            }
        }
        /// <summary>
        /// a megadott user propertyjeit AD-bol updateljuk. a user csak virtual user, a propertyket
        /// nem mentjuk ra a node-ra
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="virtualUser"></param>
        public override bool SyncVirtualUserFromAD(string domain, string username, User virtualUser)
        {
            var authSetting = GetAuthADPathFromDomain(domain);

            if (authSetting == null)
                return false;

            var config = AD2PortalConfiguration.Current;
            var propertyMappings = config.GetPropertyMappings();
            var success = Common.SyncVirtualUserFromAD(authSetting.ADServer, username, virtualUser, propertyMappings, config.CustomADAdminAccountName, config.CustomADAdminAccountPwd, config.NovellSupport, config.GuidProp, config.SyncUserName);
            return success;
        }
        /// <summary>
        /// a megadott user propertyjeit AD-bol updateljuk. a user csak virtual user, a propertyket
        /// nem mentjuk ra a node-ra
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="virtualUser"></param>
        public override bool SyncVirtualUserFromAD(Guid guid, User virtualUser)
        {
            var formsADAuthSettings = SenseNet.Configuration.FormsAuthenticationFromADSection.Current.AuthSettings;

            var config = AD2PortalConfiguration.Current;
            var propertyMappings = config.GetPropertyMappings();

            // vegigmegyunk az osszes bekonfigolt tartomanyon, ahol vannak virtualuserek
            foreach (AuthSettingElement authSetting in formsADAuthSettings)
            {
                if (authSetting.VirtualADUser)
                {
                    bool success = Common.SyncVirtualUserFromAD(authSetting.ADServer, guid, virtualUser, propertyMappings, config.CustomADAdminAccountName, config.CustomADAdminAccountPwd, config.NovellSupport, config.GuidProp, config.SyncUserName);
                    if (success)
                        return true;
                }
            }
            return false;
        }
    }
}
