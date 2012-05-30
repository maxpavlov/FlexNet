using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using System;

namespace SenseNet.ApplicationModel
{
    public class ScenarioManager
    {
        public static GenericScenario GetScenario(string name)
        {
            return GetScenario(name, null);
        }

        public static GenericScenario GetScenario(string name, string parameters)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var sc = TypeHandler.ResolveInstance<GenericScenario>(name);
            if (sc == null)
                return null;

            //default, generic scenario without codebehind
            if (string.IsNullOrEmpty(sc.Name))
                sc.Name = name;

            if (!string.IsNullOrEmpty(parameters))
                sc.Initialize(GetParameters(parameters));

            return sc;
        }

        private static Dictionary<string, object> GetParameters(string parameters)
        {
            return ActionFramework.ParseParameters(parameters);
        }
    }
}
