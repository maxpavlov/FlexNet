using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;
using System;
using SenseNet.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.Portal.UI.Bundling
{
    /// <summary>
    /// Custom bundle dedicated for bundling and minifying CSS files.
    /// </summary>
    public class CssBundle : Bundle
    {
        private List<string> _postponedPaths;

        /// <summary>
        /// The media type of this CSS bundle.
        /// </summary>
        public string Media { get; set; }

        public IEnumerable<string> PostponedPaths
        {
            get { return _postponedPaths ?? (_postponedPaths = new List<string>()); }
        }

        /// <summary>
        /// Creates a new instance of the CssBundle class.
        /// </summary>
        public CssBundle()
        {
            MimeType = "text/css";
            _postponedPaths = new List<string>();
        }

        /// <summary>
        /// In addition to combining, also minifies the given CSS.
        /// </summary>
        /// <returns>The combined and minified CSS code for this bundle.</returns>
        public override string Combine()
        {
            var combined = base.Combine();

            // Step 1: Bubble the @import directives to the top

            var pattern = @"@import url\(.*?\);";
            var importList = new StringBuilder();

            combined = Regex.Replace(combined, pattern, m =>
            {
                var url = m.Groups[0].Value;
                url = url.Substring(12, url.Length - 13).Replace("\"", "").Replace("'", "");

                if (url.Contains(" "))
                    importList.Append("@import url('" + url + "');\r\n");
                else
                    importList.Append("@import url(" + url + ");\r\n");

                return string.Empty;
            }, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

            combined = importList.ToString() + combined;

            // Step 2: The actual CSS minification

            var result = string.Empty;
            var errorLines = string.Empty;
            var hasError = false;

            try
            {
                var cssParser = new CssParser();

                cssParser.Settings.CommentMode = CssComment.None;
                cssParser.Settings.MinifyExpressions = true;
                cssParser.Settings.OutputMode = OutputMode.SingleLine;
                cssParser.Settings.TermSemicolons = false;

                cssParser.CssError += delegate(object sender, CssErrorEventArgs args)
                {
                    // The 0 severity means errors.
                    // We can safely ignore the rest.
                    if (args.Error.Severity == 0)
                    {
                        hasError = true;
                        errorLines += string.Format("\r\n/* CSS Parse error when processing the bundle.\r\nLine {0} column {1}.\r\nError message: {2}, severity: {3} */",
                            args.Error.StartLine,
                            args.Error.StartColumn,
                            args.Error.Message,
                            args.Error.Severity);
                    }
                };

                result = cssParser.Parse(combined);
            }
            catch (Exception exc)
            {
                hasError = true;
                Logger.WriteException(exc);
            }

            // If there were errors, use the non-minified version and append the errors to the bottom,
            // so that the portal builder can debug it.
            if (hasError)
                result = combined + "\r\n\r\n" + errorLines;

            return result;
        }

        protected override string GetTextFromPath(string path)
        {
            var text = base.GetTextFromPath(path);

            if (text != null)
            {
                var parentPath = path.Substring(0, path.LastIndexOf('/'));

                // Search for url(...) occurences in the CSS, and replace them with absolute URLs

                var pattern = @"url\(.*?\)";

                text = Regex.Replace(text, pattern, m =>
                {
                    var url = m.Groups[0].Value;
                    url = url.Substring(4, url.Length - 5).Replace("\"", "").Replace("'", "");

                    // Replace relative URLs with absolute ones
                    if (url[0] != '/' && !url.StartsWith("http://") && !url.StartsWith("https://"))
                        url = parentPath + "/" + url;

                    if (url.Contains(" "))
                        return "url(\"" + url + "\")";
                    
                    return "url(" + url + ")";
                }, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

                // Search for import directives and expand them if they are URLs in the portal
                // (Other import directives will be dealt with later in the pipeline.)

                pattern = @"@import url\(.*?\);";

                text = Regex.Replace(text, pattern, m =>
                {
                    var url = m.Groups[0].Value;
                    url = url.Substring(12, url.Length - 13).Replace("\"", "").Replace("'", "");

                    // If this is a web URL, just ignore it for now
                    if (url.StartsWith("http://") || url.StartsWith("https://"))
                    {
                        return "@import url('" + url + "');";
                    }

                    // Otherwise just recursively grab the url referenced by the import

                    var importedContent = GetTextFromPath(url);

                    if (importedContent == null)
                        return string.Empty;

                    return "\r\n" + importedContent + "\r\n";
                }, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
            }

            return text;
        }

        public void AddPostponedPath(string path)
        {
            if (string.IsNullOrEmpty(path) || _postponedPaths.Contains(path))
                return;

            _postponedPaths.Add(path);
        }
    }
}
