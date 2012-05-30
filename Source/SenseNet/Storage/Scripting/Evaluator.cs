using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using SenseNet.Diagnostics;

namespace SenseNet.ContentRepository.Storage.Scripting
{
	public class Evaluator
	{
		#region Singleton
		private static readonly object _lock = new object();
		private static Evaluator _current;
		private static Evaluator Current
		{
			get
			{
				if (_current == null)
				{
					lock (_lock)
					{
						if (_current == null)
						{
							var current = new Evaluator();
							current.Load();
                            _current = current;
                            Logger.WriteInformation("Evaluator created.", Logger.GetDefaultProperties, _current);
                        }
					}
				}
				return _current;
			}
		}
		private Evaluator()
		{
		}
		internal static void Reset()
		{
			// Need to call:
			//	Node.Delete (need to remove in-memory entries of whole deleted subtree)
			//	Node.DeletePhysical (need to remove in-memory entries of whole deleted subtree)
			//	Modify permissions on a Node (add, remove, copy, break etc.)
			_current = null;
		}
		#endregion

		private Dictionary<string, IEvaluator> engines;

		private void Load()
		{
			engines = new Dictionary<string, IEvaluator>();
			foreach (var type in TypeHandler.GetTypesByInterface(typeof(IEvaluator)))
			{
				var attrs = (ScriptTagNameAttribute[])type.GetCustomAttributes(typeof(ScriptTagNameAttribute), false);
				if (attrs.Length == 0)
					throw new ApplicationException(String.Concat(
						"Evaluator has not ScriptTagNameAttribute: ", type.FullName, " (Assembly: ", type.Assembly.ToString(), ")"));
                var engine = (IEvaluator)Activator.CreateInstance(type);
				engines.Add(attrs[0].TagName, engine);

                Logger.WriteInformation(String.Concat("Add Evaluator: ", attrs[0].TagName, "."), Logger.GetDefaultProperties, engine);
			}
		}

		public static string Evaluate(string sourceCode)
		{
            if (sourceCode == null)
                throw new ArgumentNullException("sourceCode");
            return Current.EvaluateInternal(sourceCode);
		}

        private string EvaluateInternal(string sourceCode)
		{
			Regex regex = new Regex(@"\[Script:[\w]*\](.+?)\[\/Script\]");
            return regex.Replace(sourceCode, new MatchEvaluator(ReplaceTags));
		}
		private string ReplaceTags(Match match)
		{
			// "[Script:tagName]...[/Script]"
			var src = match.Value;

			var start = "[Script:".Length;
            var end = src.IndexOf("]", StringComparison.Ordinal);

			var tagName = src.Substring(start, end - start);
			var startTag = String.Concat("[Script:", tagName, "]");
			src = src.Replace(startTag, "").Replace("[/Script]", "");

			IEvaluator evaluator;
			if (!engines.TryGetValue(tagName, out evaluator))
				return src;

			var result = src;

			try
			{
				result = evaluator.Evaluate(src);
                Logger.WriteVerbose("Script evaluated", new Dictionary<string, object> { { "Evaluator", evaluator.GetType().FullName }, { "Source", src } });
			}
			catch (Exception e) //logged
			{
                Logger.WriteException(e);
				var ee = e;
				var msgBuilder = new StringBuilder();
				while (ee != null)
				{
					msgBuilder.Append(ee.Message).Append("; ");
					ee = ee.InnerException;
				}
                return msgBuilder.ToString();
			}
			return result;
		}
	}
}
