using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Packaging
{
    public interface IInstallLogger
    {
        void WriteTitle(string title);
        void WriteMessage(string message);
        void WriteInstallStep(InstallStepCategory category, string stepName, string resourceName, string targetName, bool probe, bool overwrite, bool userModified, object previousState);
    }

    public class Logger
    {
        private static IInstallLogger[] _loggers;
        private static IInstallLogger[] Loggers
        {
            get
            {
                if (_loggers == null)
                    _loggers = (from t in TypeHandler.GetTypesByInterface(typeof(IInstallLogger)) 
                                select (IInstallLogger)Activator.CreateInstance(t)).ToArray<IInstallLogger>();
                return _loggers;
            }
        }

        public static void LogTitle(string title)
        {
            foreach (var logger in Loggers)
                logger.WriteTitle(title);
        }
        public static void LogMessage(string message)
        {
            foreach (var logger in Loggers)
                logger.WriteMessage(message);
        }
        public static void LogMessage(string format, params object[] parameters)
        {
            var msg = String.Format(format, parameters);
            foreach (var logger in Loggers)
                logger.WriteMessage(msg);
        }
        public static void LogWarningMessage(string message)
        {
            var msg = String.Concat("WARNING: ", message);
            foreach (var logger in Loggers)
                logger.WriteMessage(msg);
        }
        public static void LogException(Exception e)
        {
            LogMessage(PrintException(e, null));
        }
        public static void LogException(Exception e, string prefix)
        {
            LogMessage(PrintException(e, prefix));
        }
        internal static void LogInstallStep(InstallStepCategory category, string stepName, string resourceName, string targetName, bool probe, bool overwrite, bool userModified, object previousState)
        {
            foreach (var logger in Loggers)
                logger.WriteInstallStep(category, stepName, resourceName, targetName, probe, overwrite, userModified, previousState);
        }

        public static string GetVerb(InstallStepCategory category, string stepName, bool probe, bool overwrite, bool userModified)
        {
            switch (category)
            {
                case InstallStepCategory.Assembly:
                    return probe ?
                        stepName + (overwrite ? " will be upgraded" : " will be installed") :
                        (overwrite ? "UPGRADE " : "INSTALL ") + stepName;
                case InstallStepCategory.DbScript:
                    return (probe ? "CHECK " : "EXECUTE ") + stepName;
                case InstallStepCategory.None:
                case InstallStepCategory.ContentType:
                case InstallStepCategory.Content:
                default:
                    switch ((probe ? 4 : 0) + (overwrite ? 2 : 0) + (userModified ? 1 : 0))
                    {
                        case 0: return "INSTALL " + stepName;
                        case 1: return "OVERWRITE " + stepName; //"BACKUP AND OVERWRITE " + stepName;
                        case 2: return "UPGRADE " + stepName;
                        case 3: return "UPGRADE " + stepName; //"BACKUP AND UPGRADE " + stepName;
                        case 4: return stepName + " will be installed";
                        case 5: return stepName + " will be overwritten"; //" will be backed up and overwritten";
                        case 6: return stepName + " will be upgraded";
                        case 7: return stepName + " will be upgraded"; //will be backed up and upgraded";
                    }
                    throw new NotImplementedException();
            }

        }

        private static string PrintException(Exception e, string prefix)
        {
            StringBuilder sb = new StringBuilder();
            if (prefix != null)
                sb.Append(prefix).Append(": ");

            sb.Append(e.GetType().Name);
            sb.Append(": ");
            sb.AppendLine(e.Message);
            PrintTypeLoadError(e as System.Reflection.ReflectionTypeLoadException, sb);
            sb.AppendLine(e.StackTrace);
            while ((e = e.InnerException) != null)
            {
                sb.AppendLine("---- Inner Exception:");
                sb.Append(e.GetType().Name);
                sb.Append(": ");
                sb.AppendLine(e.Message);
                PrintTypeLoadError(e as System.Reflection.ReflectionTypeLoadException, sb);
                sb.AppendLine(e.StackTrace);
            }
            return sb.ToString();
        }
        private static void PrintTypeLoadError(System.Reflection.ReflectionTypeLoadException exc, StringBuilder sb)
        {
            if (exc == null)
                return;
            sb.AppendLine("LoaderExceptions:");
            foreach (var e in exc.LoaderExceptions)
            {
                sb.Append("-- ");
                sb.Append(e.GetType().FullName);
                sb.Append(": ");
                sb.AppendLine(e.Message);

                var fileNotFoundException = e as System.IO.FileNotFoundException;
                if (fileNotFoundException != null)
                {
                    sb.AppendLine("FUSION LOG:");
                    sb.AppendLine(fileNotFoundException.FusionLog);
                }
            }
        }
    }
}
