using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.UI
{
    public class SNScriptLoader
    {
        private HashSet<string> _requestedScripts;

        private SortedDictionary<string, List<string>> _depTree;
        
        internal SNScriptLoader()
        {
            _requestedScripts = new HashSet<string>(new CaseInsensitiveEqualityComparer());
            _depTree = new SortedDictionary<string, List<string>>();
        }

        private IEnumerable<string> _scriptsToLoad;
        internal IEnumerable<string> GetScriptsToLoad()
        {
            if (_scriptsToLoad == null)
            {
                var scriptsToLoad = new List<string>();

                var notInList = _requestedScripts.ToList();

                while (notInList.Count() > 0)
                {
                    var noDeps = notInList.Where(n => (_depTree[n].Count() == 0));

                    string s = noDeps.FirstOrDefault();
                    if (s == null)
                        throw new ApplicationException("Cycle found in JavaScript/CSS dependency graph");

                    notInList.Remove(s);
                    _depTree.Remove(s);
                    scriptsToLoad.Add(s);

                    foreach (var kv in _depTree)
                        kv.Value.Remove(s);
                }

                _scriptsToLoad = scriptsToLoad.Select(s => SkinManager.Resolve(s));
            }

            return _scriptsToLoad;
        }

        public void AddScript(string relPath)
        {
            if (_scriptsToLoad != null)
                throw new InvalidOperationException("Cannot add new script after dependency resolution.");

            var isNew = _requestedScripts.Add(relPath);
            if (isNew)
                AddDependencies(relPath);
        }

        private void AddDependencies(string relPath)
        {
            IEnumerable<string> deps = GetDependencies(relPath);
            CacheDeps(relPath, deps);
            if (deps == null)
                return;

            foreach (var d in deps)
            {
                AddScript(d);
            }
        }

        private void CacheDeps(string requestedBy, IEnumerable<string> deps)
        {
            if (deps == null)
                deps = new List<string>();

            _depTree.Add(requestedBy, deps.ToList());
        }

        public static SNScriptLoader Current(System.Web.UI.Page page)
        {
            var manager = SNScriptManager.GetCurrent(page) as SNScriptManager;

            if (manager != null)
                return manager.SmartLoader;

            return null;
        }
        private static bool IsAbsoluteScriptPath(string path)
        {
            return path.StartsWith("/");
        }

        private static IEnumerable<string> GetDependencies(string relPath)
        {
            var fullpath = SkinManager.Resolve(relPath);
            return SNScriptDependencyCache.Instance.GetDependencies(fullpath);
        }
    }
}
