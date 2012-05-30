using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using SenseNet.ContentRepository.Storage.Data;
using System.IO;
using System.Data;

namespace SenseNet.Packaging.Internal
{
    internal class DbScriptInstallStep : ItemInstallStep
    {
        public override string StepShortName { get { return "DbScript"; } }
        public PositionInSequence Running { get; set; }

        public DbScriptInstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
        {
            Running = GetParameterValue<PositionInSequence>("Running");
        }
        public override void Initialize()
        {
            if (Running == PositionInSequence.Default)
                Running = PositionInSequence.BeforeExecutables;
            base.Initialize();
        }

        public override StepResult Probe()
        {
            Logger.LogInstallStep(InstallStepCategory.DbScript, StepShortName, ResourceName, ResourceName, true, false, false, null);
            return Check() ?? new StepResult { Kind = StepResultKind.Successful };
        }
        public override StepResult Install()
        {
            var checkError = Check();
            if (checkError != null)
                return checkError;
            Logger.LogInstallStep(InstallStepCategory.DbScript, StepShortName, ResourceName, ResourceName, false, false, false, null);

            string query = null;
            using (var reader = new StreamReader(GetResourceStream(), true))
            {
                query = reader.ReadToEnd();
            }
            var sb = new StringBuilder();
            using (var proc = DataProvider.CreateDataProcedure(query))
            {
                proc.CommandType = CommandType.Text;
                var reader = proc.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            sb.Append(reader[i]).Append("\t");
                        sb.AppendLine();
                    }
                }
            }
            if (sb.Length > 0)
                Logger.LogMessage(sb.ToString());
            else
                Logger.LogMessage("Script is successfully executed.");

            return new StepResult { Kind = StepResultKind.Successful };
        }

        private StepResult Check()
        {
            try
            {
                string query = null;
                using (var reader = new StreamReader(GetResourceStream(), true))
                {
                    query = reader.ReadToEnd();
                }
                DataProvider.CheckScript(query);
            }
            catch (Exception e)
            {
                Logger.LogException(e, "DBSCRIPT ERROR");
                return new StepResult { Kind = StepResultKind.Error };
            }
            return null;
        }
    }
}
