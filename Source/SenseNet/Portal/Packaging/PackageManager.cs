using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;
using SenseNet.Packaging.Internal;
using System.Linq.Expressions;

namespace SenseNet.Packaging
{
    public class PackageManager
    {
        private abstract class StepVisitor
        {
            public abstract bool IsProbing { get; }
            public abstract StepResult DoIt(InstallStep step);
        }
        private class ProbeVisitor : StepVisitor
        {
            public override bool IsProbing { get { return true; } }
            public override StepResult DoIt(InstallStep step)
            {
                return step.Probe();
            }
        }
        private class InstallVisitor : StepVisitor
        {
            public override bool IsProbing { get { return false; } }
            public override StepResult DoIt(InstallStep step)
            {
                return step.Install();
            }
        }

        internal const string ExtractDirextorySuffix = ".PackageExtract";

        internal static string PluginsPath { get; private set; }
        internal static string SourcePath { get; private set; }

        public static InstallResult InstallProbe(string sourcePath, string pluginsPath)
        {
            InitializeInstall(sourcePath, pluginsPath);
            var visitor = new ProbeVisitor();
            return DoIt(sourcePath, visitor, true);
        }
        public static InstallResult Install(string sourcePath, string pluginsPath)
        {
            InitializeInstall(sourcePath, pluginsPath);
            var visitor = new InstallVisitor();
            return DoIt(sourcePath, visitor, true);
        }
        public static InstallResult InstallPhase2(string sourcePath, string pluginsPath)
        {
            InitializeInstall(sourcePath, pluginsPath);
            var visitor = new InstallVisitor();
            return DoIt(sourcePath, visitor, false);
        }
        private static void InitializeInstall(string sourcePath, string pluginsPath)
        {
            SourcePath = sourcePath;
            PluginsPath = pluginsPath;
        }

        private static InstallResult DoIt(string fsPath, StepVisitor visitor, bool withBinaries)
        {
            InstallResult result = new InstallResult { Successful = true, NeedRestart = false };
            try
            {
                int warn;
                int err;
                int warnings = 0;
                int errors = 0;

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(AssemblyHandler.CurrentDomain_ReflectionOnlyAssemblyResolve);
                var unpacker = CreateUnpacker(fsPath);

                var manifests = unpacker.Unpack(fsPath);

                var dbScripts = GetDbScripts(manifests);

                InstallResult dbScriptResult;
                dbScriptResult = ExecuteDbScripts(PositionInSequence.BeforePackageValidating, dbScripts, visitor);
                if (!dbScriptResult.Successful)
                    return dbScriptResult;
                result.Combine(dbScriptResult);

                var validateResult = ValidatePackage(manifests, visitor, withBinaries, out warn);
                warnings += warn;
                if (!validateResult.Successful)
                    return validateResult;
                result.Combine(validateResult);

                dbScriptResult = ExecuteDbScripts(PositionInSequence.AfterPackageValidating, dbScripts, visitor);
                if (!dbScriptResult.Successful)
                    return dbScriptResult;
                result.Combine(dbScriptResult);

                //------------------------------------------------------------ 

                if (withBinaries)
                {
                    dbScriptResult = ExecuteDbScripts(PositionInSequence.BeforeCheckRequirements, dbScripts, visitor);
                    if (!dbScriptResult.Successful)
                        return dbScriptResult;
                    result.Combine(dbScriptResult);

                    Logger.LogMessage("");
                    Logger.LogMessage("------------------------------ Prerequisits ---------------------------------");
                    Logger.LogMessage("");
                    result.Combine(ExecutePrerequisits(manifests, visitor, out warn, out err));
                    warnings += warn;
                    if (!result.Successful)
                        return result;

                    dbScriptResult = ExecuteDbScripts(PositionInSequence.AfterCheckRequirements, dbScripts, visitor);
                    if (!dbScriptResult.Successful)
                        return dbScriptResult;
                    result.Combine(dbScriptResult);

                    dbScriptResult = ExecuteDbScripts(PositionInSequence.BeforeExecutables, dbScripts, visitor);
                    if (!dbScriptResult.Successful)
                        return dbScriptResult;
                    result.Combine(dbScriptResult);

                    Logger.LogMessage("");
                    Logger.LogMessage("------------------------------ Executables ----------------------------------");
                    Logger.LogMessage("");
                    result.Combine(ExecuteExecutables(manifests, visitor, out warn, out err));
                    warnings += warn;

                    dbScriptResult = ExecuteDbScripts(PositionInSequence.AfterExecutables, dbScripts, visitor);
                    if (!dbScriptResult.Successful)
                        return dbScriptResult;
                    result.Combine(dbScriptResult);

                    if (!result.Successful || result.NeedRestart)
                    {
                        if (warnings > 0)
                            Logger.LogMessage(String.Concat(warnings, " warnings"));
                        if (errors > 0)
                            Logger.LogMessage(String.Concat(errors, " errors"));
                        return result;
                    }
                }

                dbScriptResult = ExecuteDbScripts(PositionInSequence.BeforeContentTypes, dbScripts, visitor);
                if (!dbScriptResult.Successful)
                    return dbScriptResult;
                result.Combine(dbScriptResult);

                Logger.LogMessage("");
                Logger.LogMessage("------------------------------ ContentTypes ---------------------------------");
                Logger.LogMessage("");
                result.Combine(ExecuteContentTypes(manifests, visitor, out warn, out err));
                warnings += warn;
                if (!result.Successful)
                    return result;

                dbScriptResult = ExecuteDbScripts(PositionInSequence.AfterContentTypes, dbScripts, visitor);
                if (!dbScriptResult.Successful)
                    return dbScriptResult;
                result.Combine(dbScriptResult);

                dbScriptResult = ExecuteDbScripts(PositionInSequence.BeforeContents, dbScripts, visitor);
                if (!dbScriptResult.Successful)
                    return dbScriptResult;
                result.Combine(dbScriptResult);

                Logger.LogMessage("");
                Logger.LogMessage("-------------------------------- Contents -----------------------------------");
                Logger.LogMessage("");
                result.Combine(ExecuteContents(manifests, visitor, out warn, out err));
                warnings += warn;
                if (!result.Successful)
                    return result;

                dbScriptResult = ExecuteDbScripts(PositionInSequence.AfterContents, dbScripts, visitor);
                if (!dbScriptResult.Successful)
                    return dbScriptResult;
                result.Combine(dbScriptResult);

                if (warnings > 0)
                    Logger.LogMessage(String.Concat(warnings, " warnings"));
                if (errors > 0)
                    Logger.LogMessage(String.Concat(errors, " errors"));

                return result;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                return new InstallResult { Successful = false };
            }
        }

        private static InstallResult ExecutePrerequisits(IEnumerable<IManifest> manifests, StepVisitor visitor, out int warnings, out int errors)
        {
            int warn;
            bool needRestart;

            warnings = 0;
            errors = 0;

            if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnBeforeCheckRequirements(default(bool))), visitor.IsProbing, out warn))
                return ReturnWithErrorResult();
            warnings += warn;
            foreach (var manifest in manifests)
            {
                Logger.LogMessage(String.Concat("Package: ", manifest.PackageInfo.Name, " version: ", manifest.PackageInfo.Version));
                if (manifest.Prerequisits.Length == 0)
                    Logger.LogMessage("Has not any prerequisits.");
                if (!ExecuteStepFamily(manifest.Prerequisits, visitor, out needRestart, out warn))
                    return ReturnWithErrorResult();
                warnings += warn;
            }
            if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnAfterCheckRequirements(default(bool))), visitor.IsProbing, out warn))
                return ReturnWithErrorResult();
            warnings += warn;

            return new InstallResult { Successful = true, NeedRestart = false };
        }
        private static InstallResult ExecuteExecutables(IEnumerable<IManifest> manifests, StepVisitor visitor, out int warnings, out int errors)
        {
            int warn;
            bool needRestart;
            bool needRestartAggregated = false;

            warnings = 0;
            errors = 0;

            if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnBeforeInstallExecutables(default(bool))), visitor.IsProbing, out warn))
                return ReturnWithErrorResult();
            warnings += warn;

            foreach (var manifest in manifests)
            {
                Logger.LogMessage(String.Concat("Package: ", manifest.PackageInfo.Name, " version: ", manifest.PackageInfo.Version));
                if (manifest.Executables.Length == 0)
                    Logger.LogMessage("Has not any executables");

                if (!ExecuteStepFamily(manifest.Executables, visitor, out needRestart, out warn))
                    return ReturnWithErrorResult();
                if (needRestart)
                    needRestartAggregated = true;
                warnings += warn;
            }

            if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnAfterInstallExecutables(default(bool))), visitor.IsProbing, out warn))
                return ReturnWithErrorResult();
            warnings += warn;

            return new InstallResult { Successful = true, NeedRestart = needRestartAggregated };
        }
        private static InstallResult ExecuteContentTypes(IEnumerable<IManifest> manifests, StepVisitor visitor, out int warnings, out int errors)
        {
            warnings = 0;
            errors = 0;
            int warn;

            if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnBeforeInstallContentTypes(default(bool))), visitor.IsProbing, out warn))
                return ReturnWithErrorResult();
            warnings += warn;

            var batchContentTypeInstallStep = AggregateContentTypeInstallSteps(manifests);
            if (batchContentTypeInstallStep != null)
            {
                //---- Initialize
                try
                {
                    batchContentTypeInstallStep.Initialize();
                }
                catch (Exception e)
                {
                    Logger.LogException(e, "INITIALIZING ERROR");
                    return ReturnWithErrorResult();
                }

                //---- Install
                try
                {
                    StepResult result = visitor.DoIt(batchContentTypeInstallStep);
                    if (result.Kind == StepResultKind.Warning)
                        warnings++;
                    if (result.Kind == StepResultKind.Error)
                        return ReturnWithErrorResult();
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                    return ReturnWithErrorResult();
                }
            }

            if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnAfterInstallContentTypes(default(bool))), visitor.IsProbing, out warn))
                return ReturnWithErrorResult();
            warnings += warn;

            return new InstallResult { Successful = true, NeedRestart = false };
        }
        private static InstallResult ExecuteContents(IEnumerable<IManifest> manifests, StepVisitor visitor, out int warnings, out int errors)
        {
            warnings = 0;
            int warn;
            errors = 0;

            if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnBeforeInstallContents(default(bool))), visitor.IsProbing, out warn))
                return ReturnWithErrorResult();
            warnings += warn;

            var contentSteps = new List<ContentInstallStep>();
            foreach (var manifest in manifests)
                contentSteps.AddRange(GetContentSteps(manifest));

            //---- Initialize  contents
            try
            {
                foreach (var step in contentSteps)
                    step.Initialize();
            }
            catch (Exception e)
            {
                Logger.LogException(e, "INITIALIZING ERROR");
                return ReturnWithErrorResult();
            }

            //---- Install contents
            contentSteps.Sort();
            var postponedContents = new List<ContentInstallStep>();
            foreach (var step in contentSteps)
            {
                try
                {
                    StepResult result = visitor.DoIt(step);
                    if (result.Kind == StepResultKind.Warning)
                        warnings++;
                    if (result.Kind == StepResultKind.Error)
                        return ReturnWithErrorResult();
                    if (result.NeedSetReferencePhase)
                        postponedContents.Add(step);
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                    return ReturnWithErrorResult();
                }
            }
            foreach (var step in postponedContents)
            {
                try
                {
                    StepResult result = step.SetReferences();
                    if (result.Kind == StepResultKind.Warning)
                        warnings++;
                    if (result.Kind == StepResultKind.Error)
                        errors++;
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                    return ReturnWithErrorResult();
                }
            }

            if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnAfterInstallContents(default(bool))), visitor.IsProbing, out warn))
                return ReturnWithErrorResult();
            warnings += warn;

            return new InstallResult { Successful = true, NeedRestart = false };
        }
        private static InstallResult ExecuteDbScripts(PositionInSequence positionInSequence, IEnumerable<DbScriptInstallStep> dbScripts, StepVisitor visitor)
        {
            var currentScripts = (from dbScript in dbScripts where dbScript.Running == positionInSequence select dbScript).Cast<InstallStep>().ToArray<InstallStep>();
            if(currentScripts.Length == 0)
                return new InstallResult { Successful = true };

            Logger.LogMessage("");
            Logger.LogMessage(String.Concat("------------------- Execute DbScripts " + positionInSequence.ToString() + " -------------------"));
            Logger.LogMessage("");
            bool needRestart;
            int warnings;
            if(!ExecuteStepFamily(currentScripts, visitor, out needRestart, out warnings))
                return ReturnWithErrorResult();

            return new InstallResult { Successful = true, NeedRestart = false };
        }

        private static IEnumerable<DbScriptInstallStep> GetDbScripts(IEnumerable<IManifest> manifests)
        {
            var dbScripts = new List<DbScriptInstallStep>();
            foreach (var manifest in manifests)
                dbScripts.AddRange(manifest.DbScripts);
            return dbScripts;
        }
        private static BatchContentTypeInstallStep AggregateContentTypeInstallSteps(IEnumerable<IManifest> manifests)
        {
            var steps = new List<ContentTypeInstallStep>();
            foreach (var manifest in manifests)
                steps.AddRange(manifest.ContentTypes);
            if (steps.Count > 0)
                return new BatchContentTypeInstallStep(steps.ToArray());
            return null;
        }
        private static InstallResult ReturnWithErrorResult()
        {
            return new InstallResult { Successful = false, NeedRestart = false };
        }
        private static bool ExecuteStepFamily(IEnumerable<InstallStep> steps, StepVisitor visitor, out bool needRestart, out int warnings)
        {
            warnings = 0;
            needRestart = false;

            //-- initialize steps
            try
            {
                foreach (var step in steps)
                    step.Initialize();
            }
            catch (Exception e)
            {
                Logger.LogException(e, "INITIALIZING ERROR");
                return false;
            }

            //-- excute steps
            foreach (var step in steps)
            {
                try
                {
                    StepResult result = visitor.DoIt(step);
                    needRestart = result.NeedRestart;
                    if (result.Kind == StepResultKind.Error)
                        return false;
                    if (result.Kind == StepResultKind.Warning)
                        warnings++;
                }
                catch (Exception e)
                {
                    Logger.LogException(e);
                    return false;
                }
            }
            return true;
        }

        private static List<ContentInstallStep> GetContentSteps(IManifest manifest)
        {
            var steps = new List<ContentInstallStep>();
            steps.AddRange(manifest.PageTemplates.Cast<ContentInstallStep>());
            steps.AddRange(manifest.Resources.Cast<ContentInstallStep>());
            steps.AddRange(manifest.ContentViews.Cast<ContentInstallStep>());
            steps.AddRange(manifest.Files.Cast<ContentInstallStep>());
            steps.AddRange(manifest.Contents.Cast<ContentInstallStep>());
            return steps;
        }

        //==================================================================

        private static MethodInfo GetMethod<T>(Expression<Action<T>> lambda)
        {
            return ((MethodCallExpression)lambda.Body).Method;
        }

        private static IUnpacker CreateUnpacker(string fsPath)
        {
            var ext = Path.GetExtension(fsPath).ToUpper();
            if (ext == ExtractDirextorySuffix.ToUpper())
                return new DirectoryUnpacker();
            if (ext == ".ZIP")
                return new ZipUnpacker();
            if (ext == ".DLL" || ext == ".EXE")
                return new AssemblyUnpacker();
            throw new ApplicationException(String.Concat("Invalid package: ", fsPath, "'"));
        }
        private static InstallResult ValidatePackage(IManifest[] manifests, StepVisitor visitor, bool enableCustomValidation, out int warnings)
        {
            InstallResult result = new InstallResult();
            warnings = 0;
            int warn;

            if (enableCustomValidation)
            {
                if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnPackageValidating(default(bool))), visitor.IsProbing, out warn))
                    return ReturnWithErrorResult();
                warnings += warn;
            }

            foreach (var manifest in manifests)
            {
                if (enableCustomValidation)
                    Logger.LogMessage(String.Concat("Validating ", manifest.PackageInfo.Name, " (version: ", manifest.PackageInfo.Version, ")"));

                var pkgInfo = manifest.PackageInfo;
                if (pkgInfo == null)
                {
                    Logger.LogMessage("Required PackageDescription is missing");
                    return new InstallResult { Successful = false };
                }
                CheckPackageNameAndVersion(pkgInfo);

                if (enableCustomValidation)
                    Logger.LogMessage("Package is valid.");
            }

            if (enableCustomValidation)
            {
                if (!CustomInstallStep.Invoke(GetMethod<CustomInstallStep>(x => x.OnPackageValidated(default(bool))), visitor.IsProbing, out warn))
                    return ReturnWithErrorResult();
                warnings += warn;
            }

            return new InstallResult { Successful = true };
        }
        private static void CheckPackageNameAndVersion(PackageInfo pkgInfo)
        {
            if (String.IsNullOrEmpty(pkgInfo.Name))
                throw new InvalidManifestException("Expected 'Name' parameter is missing in the PackageDescription. " + pkgInfo.ToString());
            if (String.IsNullOrEmpty(pkgInfo.Version))
                throw new InvalidManifestException("Expected 'Version' parameter is missing in the PackageDescription. " + pkgInfo.ToString());

            Version version;
            try
            {
                version = new Version(pkgInfo.Version);
            }
            catch (Exception e)
            {
                throw new InvalidManifestException(String.Concat("the 'Version' parameter is invalid in the PackageDescription. ", pkgInfo.ToString(), ". ", e.Message));
            }

            // get installed manifests for checking uniqueness and version
        }

        internal static PreviousItemState GetPreviousContentState(ContentInstallStep contentInstallStep)
        {
            var version = contentInstallStep.Manifest.PackageInfo.Version;

            var exists = ContentManager.Path.IsPathExists(contentInstallStep.ContentPath);

            return exists ? PreviousItemState.UserCreated : PreviousItemState.NotInstalled;
        }
        internal static PreviousItemState GetPreviousAssemblyState(string path, AssemblyName assemblyName)
        {
            var installedPath = Path.Combine(PluginsPath, Path.GetFileName(path));
            if (File.Exists(installedPath))
                return PreviousItemState.UserCreated;
            return PreviousItemState.NotInstalled;
        }

        //==================================================================

        internal static void EnsureEmptyDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                foreach (var path in Directory.GetDirectories(dirPath))
                    Directory.Delete(path, true);
                foreach (var path in Directory.GetFiles(dirPath))
                    System.IO.File.Delete(path);
                return;
            }
            EnsureDirectory(dirPath);
        }
        internal static void EnsureDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
                return;
            EnsureDirectory(Path.GetDirectoryName(dirPath));
            Directory.CreateDirectory(dirPath);
        }

    }
}
