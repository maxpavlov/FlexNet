
using Microsoft.Ajax.Utilities;
using System;
using SenseNet.Diagnostics;
namespace SenseNet.Portal.UI.Bundling
{
    /// <summary>
    /// Custom bundle dedicated for bundling and minifying Javascript files.
    /// </summary>
    public class JsBundle : Bundle
    {
        public JsBundle()
        {
            MimeType = "application/x-javascript";
        }

        /// <summary>
        /// In addition to combining, also minifies the given Javascript.
        /// </summary>
        /// <returns>The combined and minified Javascript code for this bundle.</returns>
        public override string Combine()
        {
            var source = base.Combine();
            var result = string.Empty;
            var errorLines = string.Empty;
            var hasError = false;

            try
            {
                var jsParser = new JSParser(source);

                var settings = new CodeSettings()
                {
                    CombineDuplicateLiterals = true,
                    OutputMode = OutputMode.SingleLine,
                    RemoveUnneededCode = true,
                    TermSemicolons = false,
                    PreserveImportantComments = false,
                };

                jsParser.CompilerError += delegate(object sender, JScriptExceptionEventArgs args)
                {
                    // The 0 severity means errors.
                    // We can safely ignore the rest.
                    if (args.Error.Severity == 0)
                    {
                        hasError = true;
                        errorLines += string.Format("\r\n/* Javascript parse error when processing the bundle.\r\nStart: line {0} column {1}, end: line {2} column {3}.\r\nError message: {4} */",
                            args.Error.StartLine,
                            args.Error.StartColumn,
                            args.Error.EndLine,
                            args.Error.EndColumn,
                            args.Error.Message);
                    }
                };
                jsParser.UndefinedReference += delegate(object sender, UndefinedReferenceEventArgs args)
                {
                    // Let's just ignore undefined references.
                };

                var block = jsParser.Parse(settings);
                result = block.ToCode();
            }
            catch (Exception exc)
            {
                hasError = true;
                Logger.WriteException(exc);
            }

            // If there were errors, use the non-minified version and append the errors to the bottom,
            // so that the portal builder can debug it.
            if (hasError)
                result = source + "\r\n\r\n" + errorLines;

            return result;
        }

        protected override string GetTextFromPath(string path)
        {
            // The semicolon is needed for the js to function properly!
            // The comment "SN JS bundle" is only there for debugging purposes.
            return "\r\n//SN JS bundle: " + path + "\r\n" + base.GetTextFromPath(path) + ";\r\n";
        }
    }
}
