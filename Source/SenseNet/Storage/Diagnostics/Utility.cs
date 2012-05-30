using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SenseNet.ContentRepository.Storage.Security;
using System.Threading;

namespace SenseNet.Diagnostics
{
    internal static class Utility
    {
        private const string LoggedUserNameKey = "UserName";
        private const string LoggedUserNameKey2 = "LoggedUserName";
        private const string SpecialUserNameKey = "SpecialUserName";

        private static string thisNameSpace = typeof(Utility).Namespace;

        public static MethodInfo GetOriginalCaller(object handlerInstance)
        {
            //skip the last few method calls
            var stackTrace = new System.Diagnostics.StackTrace(3);
            MethodBase result = null;

            var i=0;
            while (true)
            {
                var sf = stackTrace.GetFrame(i++);
                if (sf == null)
                    break;

                result = sf.GetMethod();
                if (result == null)
                    break;

                //skip everything in SenseNet.Diagnostics namespace
                if (result.DeclaringType.Namespace != thisNameSpace)
                    break;
            }
            return result as MethodInfo;
        }

        public static IDictionary<string, object> GetDefaultProperties(object target)
        {
            var n = target as SenseNet.ContentRepository.Storage.Node;
            if (n != null)
                return new Dictionary<string, object> { { "NodeId", n.Id }, { "Path", n.Path } };
            var e = target as Exception;
            if(e != null)
                return new Dictionary<string, object> { { "Messages", CollectExceptionMessages(e) } };

            var t = target as Type;
            if (t != null)
                return new Dictionary<string, object> { { "Type", t.FullName } };

            return new Dictionary<string, object> { { "Type", target.GetType().FullName }, { "Value", target.ToString() } };
        }

        public static IDictionary<string, object> CollectAutoProperties(IDictionary<string, object> properties)
        {
            var props = properties;
            if (props == null)
                props = new Dictionary<string, object>();
            if (props.IsReadOnly)
                props = new Dictionary<string, object>(props);

            CollectUserProperties(props);

            var nullNames = new List<string>();
            foreach (var key in props.Keys)
                if (props[key] == null)
                    nullNames.Add(key);
            foreach (var key in nullNames)
                props[key] = String.Empty;

            return props;
        }
        private static void CollectUserProperties(IDictionary<string, object> properties)
        {
            //if (!AccessProvider.IsInitialized)
            //    return;

            //IUser loggedUser = AccessProvider.Current.GetCurrentUser();
            IUser loggedUser = GetCurrentUser();

            if (loggedUser == null)
                return;

            IUser specialUser = null;

            if (loggedUser is StartupUser)
            {
                specialUser = loggedUser;
                loggedUser = null;
            }
            else
            {
                var systemUser = loggedUser as SystemUser;
                if (systemUser != null)
                {
                    specialUser = systemUser;
                    loggedUser = systemUser.OriginalUser;
                }
            }

            if (loggedUser != null)
            {
                if (properties.ContainsKey(LoggedUserNameKey))
                {
                    if (properties.ContainsKey(LoggedUserNameKey2))
                        properties[LoggedUserNameKey2] = loggedUser.Username ?? String.Empty;
                    else
                        properties.Add(LoggedUserNameKey2, loggedUser.Username ?? String.Empty);
                }
                else
                {
                    properties.Add(LoggedUserNameKey, loggedUser.Username ?? String.Empty);
                }
            }
            if (specialUser != null)
            {
                if (properties.ContainsKey(SpecialUserNameKey))
                    properties[SpecialUserNameKey] = specialUser.Username ?? String.Empty;
                else
                    properties.Add(SpecialUserNameKey, specialUser.Username ?? String.Empty);
            }
        }

        private static IUser GetCurrentUser()
        {
            if ((System.Web.HttpContext.Current != null) && (System.Web.HttpContext.Current.User != null))
                return System.Web.HttpContext.Current.User.Identity as IUser;
            return Thread.CurrentPrincipal.Identity as IUser;
        }

        private static string CollectExceptionMessages(Exception ex)
        {
            var sb = new StringBuilder();
            var e = ex;
            while (e != null)
            {
                sb.AppendLine(e.Message).AppendLine(e.StackTrace).AppendLine("-----------------");
                e = e.InnerException;
            }
            return sb.ToString();
        }

    }
}
