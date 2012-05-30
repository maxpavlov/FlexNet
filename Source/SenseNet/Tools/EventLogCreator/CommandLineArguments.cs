using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SenseNet.Tools.EventLogCreator
{
    public class CommandLineArguments
    {
        private Dictionary<string, string> _parameters;

		/// <summary>
		/// Constructor.
		/// Parameter format: {/,-,--}argname{ ,=,:}((",')value(",')). Examples: /arg3:"any-:-text" -arg1 value1 --arg2 /arg4=text -arg5 'any text'
		/// </summary>
		/// <param name="args">Command line parameters</param>
		public CommandLineArguments(string[] args)
        {
			_parameters = new Dictionary<string, string>();

            Regex spliterRegex = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex removerRegex = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string parameter = null;
            string[] parts;

            foreach (string arg in args)
            {
                // Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
                parts = spliterRegex.Split(arg, 3);
                switch (parts.Length)
                {
                    case 1: // Found a value (for the last parameter found (space separator))
                        if (parameter != null)
                        {
                            if (!_parameters.ContainsKey(parameter))
                            {
                                parts[0] = removerRegex.Replace(parts[0], "$1");
                                _parameters.Add(parameter, parts[0]);
                            }
                            parameter = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;
                    case 2: // Found just a parameter
                        // The last parameter is still waiting. With no value, set it to true.
                        if (parameter != null)
                        {
                            if (!_parameters.ContainsKey(parameter))
								_parameters.Add(parameter, "true");
                        }
                        parameter = parts[1];
                        break;
                    case 3: // Parameter with enclosed value
                        // The last parameter is still waiting. With no value, set it to true.
                        if (parameter != null)
							if (!_parameters.ContainsKey(parameter))
								_parameters.Add(parameter, "true");
                        parameter = parts[1];

                        // Remove possible enclosing characters (",')
						if (!_parameters.ContainsKey(parameter))
                        {
                            parts[2] = removerRegex.Replace(parts[2], "$1");
							_parameters.Add(parameter, parts[2]);
                        }
                        parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (parameter != null)
				if (!_parameters.ContainsKey(parameter))
					_parameters.Add(parameter, "true");
        }

		public string this[string Param]
		{
			get
            {
                string value;
                _parameters.TryGetValue(Param, out value);
                return value;
            }
		}
    }
}