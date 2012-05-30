using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging
{
	public class CustomInstallStep
	{
		private static List<CustomInstallStep> _customSteps = new List<CustomInstallStep>();

		internal static void AddCustomStepTypes(Type[] types)
		{
			foreach(var type in types)
				_customSteps.Add((CustomInstallStep)Activator.CreateInstance(type));
		}
        internal static bool Invoke(MethodInfo method, bool probing, out int warnings)
        {
            warnings = 0;
            try
            {
                bool ok = true;
                foreach (var customStep in _customSteps)
                {
                    var customResult = (StepResult)method.Invoke(customStep, new object[] { probing });

                    if (customResult.Kind == StepResultKind.Error)
                        ok = false;
                    if (customResult.Kind == StepResultKind.Warning)
                        warnings++;
                }
                return ok;
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                return false;
            }
        }

        public virtual StepResult OnPackageValidating(bool probing) { return StepResult.Default; }
        public virtual StepResult OnPackageValidated(bool probing) { return StepResult.Default; }
		public virtual StepResult OnBeforeCheckRequirements(bool probing) { return StepResult.Default; }
        public virtual StepResult OnAfterCheckRequirements(bool probing) { return StepResult.Default; }
        public virtual StepResult OnBeforeInstallExecutables(bool probing) { return StepResult.Default; }
        public virtual StepResult OnAfterInstallExecutables(bool probing) { return StepResult.Default; }
        public virtual StepResult OnBeforeInstallContentTypes(bool probing) { return StepResult.Default; }
        public virtual StepResult OnAfterInstallContentTypes(bool probing) { return StepResult.Default; }
        public virtual StepResult OnBeforeInstallContents(bool probing) { return StepResult.Default; }
        public virtual StepResult OnAfterInstallContents(bool probing) { return StepResult.Default; }
	}
}
