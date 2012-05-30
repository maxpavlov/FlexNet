using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Workspaces;
using SenseNet.Search;

namespace SenseNet.Portal.UI
{
    public enum WikiArticleAction { Create, Rename, Delete }

    public static class WikiTools
    {
        //================================================================================================ Constants

        private const string WIKILINKPATTERN = "\\[\\[(?<WikiTitle>[^\\]|]+){0,1}[|]{0,1}(?<LinkTitle>[^\\]|]+){0,1}\\]\\]";
        private const string EXISTINGHTMLLINKPATTERN = "\\<a\\ [^\\>]*href=\\\"(?<ArticlePath>[^\\\"]+)\"\\ class=\"sn-wiki-link\"[^\\>]*\\>(?<LinkTitle>[^\\<]+)\\</a\\>";
        private const string EXISTINGHTMLLINKFORMATPATTERN = "\\<a\\ [^\\>]*href=\\\"{0}\"\\ class=\"sn-wiki-link\"[^\\>]*\\>(?<LinkTitle>[^\\<]+)\\</a\\>";
        private const string NONEXISTINGHTMLLINKPATTERN = "\\<a\\ [^\\>]*href=\\\"(?<ParentPath>[^\\\"]+)?action=Add&ContentTypeName=WikiArticle&DisplayName=(?<WikiTitle>[^\\\"&]+)(&backtarget=newcontent)*\"\\ class=\"sn-wiki-add\"[^\\>]*\\>(?<LinkTitle>[^\\<]+)\\</a\\>";
        private const string NONEXISTINGHTMLLINKFORMATPATTERN = "\\<a\\ [^\\>]*href=\\\"(?<ParentPath>[^\\\"]+)?action=Add&ContentTypeName=WikiArticle&DisplayName={0}(&backtarget=newcontent)*\"\\ class=\"sn-wiki-add\"[^\\>]*\\>(?<LinkTitle>[^\\<]+)\\</a\\>";

        public const string REFERENCEDTITLESFIELDSEPARATOR = "##**w**##";

        //================================================================================================ Async helper delegate

        private delegate void RefreshArticlesDelegate(WikiArticle targetArticle, WikiArticleAction articleAction, string oldName, string oldDisplayName);

        //================================================================================================ Public API

        public static string ConvertWikilinksToHtml(string articleText, Node parent)
        {
            if (string.IsNullOrEmpty(articleText))
                return articleText;

            var index = 0;
            var regex = new Regex(WIKILINKPATTERN, RegexOptions.Multiline);

            while (true)
            {
                var match = regex.Match(articleText, index);
                if (!match.Success)
                    break;

                var wTitle = match.Groups["WikiTitle"].Value;
                var lTitle = match.Groups["LinkTitle"].Value;
                if (string.IsNullOrEmpty(lTitle))
                    lTitle = wTitle;

                Node targetArticle;
                
                using (new SystemAccount())
                {
                    var wikiWs = parent == null ? null : Workspace.GetWorkspaceForNode(parent);
                    var articleQuery = ContentQuery.CreateQuery(string.Format("+TypeIs:WikiArticle +DisplayName:\"{0}\"", wTitle), new QuerySettings {EnableAutofilters = false, Top = 1});
                    if (wikiWs != null)
                        articleQuery.AddClause(string.Format("+InTree:\"{0}\"", wikiWs.Path));

                    targetArticle = articleQuery.Execute().Nodes.FirstOrDefault();   
                }

                var templateValue = targetArticle != null ? 
                    GetExistingLinkHtml(targetArticle.Path, lTitle) : 
                    GetAddLinkHtml(parent, wTitle, lTitle);

                articleText = articleText.Remove(match.Index, match.Length)
                    .Insert(match.Index, templateValue);

                index = match.Index + templateValue.Length;

                if (index >= articleText.Length)
                    break;
            }

            return articleText;
        }

        public static string ConvertHtmlToWikilinks(string articleData)
        {
            if (string.IsNullOrEmpty(articleData))
                return articleData;

            //replace links that reference EXISTING content
            var index = 0;
            var regex = new Regex(EXISTINGHTMLLINKPATTERN, RegexOptions.Multiline);

            while (true)
            {
                var match = regex.Match(articleData, index);
                if (!match.Success)
                    break;

                var articlePath = match.Groups["ArticlePath"].Value;
                var lTitle = match.Groups["LinkTitle"].Value;

                Node targetArticle = null;

                using (new SystemAccount())
                {
                    if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
                        targetArticle = ContentQuery.Query(string.Format("+TypeIs:WikiArticle +Path:\"{0}\"", articlePath), 
                            new QuerySettings { EnableAutofilters = false, Top = 1 }).Nodes.FirstOrDefault();
                }

                string templateValue;

                if (targetArticle != null)
                {
                    templateValue = GetWikiLink(targetArticle.DisplayName, lTitle);
                }
                else
                {
                    try
                    {
                        templateValue = GetWikiLink(RepositoryPath.GetFileName(articlePath), lTitle);
                    }
                    catch
                    {
                        templateValue = string.Empty;
                    }
                }

                articleData = articleData.Remove(match.Index, match.Length)
                    .Insert(match.Index, templateValue);

                index = match.Index + templateValue.Length;

                if (index >= articleData.Length)
                    break;
            }

            //replace links that reference NONEXISTING content
            index = 0;
            regex = new Regex(NONEXISTINGHTMLLINKPATTERN, RegexOptions.Multiline);

            while (true)
            {
                var match = regex.Match(articleData, index);
                if (!match.Success)
                    break;

                var wTitle = HttpUtility.UrlDecode(match.Groups["WikiTitle"].Value);
                var lTitle = match.Groups["LinkTitle"].Value;

                var templateValue = GetWikiLink(wTitle, lTitle);

                articleData = articleData.Remove(match.Index, match.Length)
                    .Insert(match.Index, templateValue);

                index = match.Index + templateValue.Length;

                if (index >= articleData.Length)
                    break;
            }

            return articleData;
        }

        public static void RefreshArticlesAsync(WikiArticle targetArticle, WikiArticleAction articleAction)
        {
            var refreshArticlesDelegate = new RefreshArticlesDelegate(RefreshArticlesAsyncInternal);

            refreshArticlesDelegate.BeginInvoke(targetArticle, articleAction, null, null, null, null);
        }

        public static void RefreshArticlesAsync(WikiArticle targetArticle,  string oldName, string oldDisplayName)
        {
            if (string.IsNullOrEmpty(oldName))
                throw new ArgumentNullException("oldName");

            var refreshArticlesDelegate = new RefreshArticlesDelegate(RefreshArticlesAsyncInternal);

            refreshArticlesDelegate.BeginInvoke(targetArticle, WikiArticleAction.Rename, oldName, oldDisplayName, null, null);
        }

        //================================================================================================ Internal methods

        private static void RefreshArticlesAsyncInternal(WikiArticle targetArticle, WikiArticleAction articleAction, string oldName, string oldDisplayName)
        {
            if (targetArticle == null || !StorageContext.Search.IsOuterEngineEnabled)
                return;

            QueryResult results;

            //background task, needs to run in admin mode
            using (new SystemAccount())
            {
                var ws = Workspace.GetWorkspaceForNode(targetArticle);
                var displayNameFilter = string.IsNullOrEmpty(oldDisplayName)
                    ? string.Format("\"{0}\"", targetArticle.DisplayName)
                    : string.Format("(\"{0}\" \"{1}\")", targetArticle.DisplayName, oldDisplayName);

                results = ContentQuery.Query(string.Format("+TypeIs:WikiArticle +ReferencedWikiTitles:{0} +InTree:\"{1}\"", displayNameFilter, ws.Path),
                        new QuerySettings {EnableAutofilters = false});

                if (results.Count == 0)
                    return;

                foreach (var article in results.Nodes.Cast<WikiArticle>())
                {
                    var articleData = article.WikiArticleText ?? string.Empty;
                    var found = false;

                    switch (articleAction)
                    {
                        case WikiArticleAction.Create:
                            //find references to NONEXISTING articles
                            articleData = ReplaceAddLinksWithExistingLinks(articleData, targetArticle, ref found);
                            break;
                        case WikiArticleAction.Delete:
                            //find references to EXISTING articles
                            articleData = ReplaceExistingLinksWithAddLinks(articleData, targetArticle.Parent, targetArticle.Path, targetArticle.DisplayName, ref found);
                            break;
                        case WikiArticleAction.Rename:
                            //find references to EXISTING articles
                            articleData = ReplaceOldLinksWithRenamedLinks(articleData, targetArticle, RepositoryPath.Combine(targetArticle.ParentPath, oldName), oldDisplayName,  ref found);
                            break;
                    }

                    if (!found)
                        continue;

                    article.WikiArticleText = articleData;

                    //TODO: this is a technical save, so set ModificationDate and 
                    //ModifiedBy fields to the original values!!!!!!!

                    article.Save(SavingMode.KeepVersion);
                }
            }
        }

        internal static string GetReferencedTitles(WikiArticle wikiArticle)
        {
            var articleData = wikiArticle.WikiArticleText;

            if (string.IsNullOrEmpty(articleData))
                return string.Empty;

            var referencedTitles = new StringBuilder();

            //find links that reference EXISTING content
            var index = 0;
            var regex = new Regex(EXISTINGHTMLLINKPATTERN, RegexOptions.Multiline);

            while (true)
            {
                var match = regex.Match(articleData, index);
                if (!match.Success)
                    break;

                var articlePath = match.Groups["ArticlePath"].Value;

                if (!string.IsNullOrEmpty(articlePath))
                {
                    Node targetArticle = null;

                    using (new SystemAccount())
                    {
                        if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
                            targetArticle = ContentQuery.Query(string.Format("+TypeIs:WikiArticle +Path:\"{0}\"", articlePath),
                                new QuerySettings {EnableAutofilters = false, Top = 1}).Nodes.FirstOrDefault();
                    }

                    if (targetArticle != null)
                        referencedTitles.AppendFormat("{0}{1}", targetArticle.DisplayName, REFERENCEDTITLESFIELDSEPARATOR);
                }

                index = match.Index + match.Length;

                if (index >= articleData.Length)
                    break;
            }

            //find links that reference NONEXISTING content
            index = 0;
            regex = new Regex(NONEXISTINGHTMLLINKPATTERN, RegexOptions.Multiline);

            while (true)
            {
                var match = regex.Match(articleData, index);
                if (!match.Success)
                    break;

                var wTitle = HttpUtility.UrlDecode(match.Groups["WikiTitle"].Value);

                if (!string.IsNullOrEmpty(wTitle))
                    referencedTitles.AppendFormat("{0}{1}", wTitle, REFERENCEDTITLESFIELDSEPARATOR);

                index = match.Index + match.Length;

                if (index >= articleData.Length)
                    break;
            }

            return referencedTitles.ToString();
        }

        //================================================================================================ Helper methods

        private static string GetExistingLinkHtml(string path, string linkText)
        {
            return string.Format("<a href=\"{0}\" class=\"sn-wiki-link\">{1}</a>", path, linkText);
        }

        private static string GetAddLinkHtml(Node parent, string articleTitle, string linkText)
        {
            return  string.Format("<a href=\"{0}\" class=\"sn-wiki-add\">{1}</a>",
                Helpers.Actions.ActionUrl(Content.Create(parent), "Add", false, new { ContentTypeName = "WikiArticle", DisplayName = HttpUtility.UrlEncode(articleTitle), backtarget = "newcontent" }), linkText);
        }

        private static string GetWikiLink(string wikiTitle, string linkText)
        {
            if (wikiTitle == null)
                wikiTitle = string.Empty;

            if (linkText == null)
                linkText = string.Empty;

            return wikiTitle != linkText && !string.IsNullOrEmpty(linkText) ? 
                string.Format("[[{0}|{1}]]", wikiTitle, linkText) : 
                string.Format("[[{0}]]", wikiTitle);
        }    

        private static string ReplaceAddLinksWithExistingLinks(string articleData, WikiArticle targetArticle, ref bool found)
        {
            var encodedDisplayName = HttpUtility.UrlEncode(targetArticle.DisplayName) ?? string.Empty;
            var regex = new Regex(string.Format(NONEXISTINGHTMLLINKFORMATPATTERN, Regex.Escape(encodedDisplayName)), RegexOptions.Multiline);

            return ReplaceWithExistingLinks(regex, articleData, targetArticle, null, ref found);
        }

        private static string ReplaceOldLinksWithRenamedLinks(string articleData, WikiArticle targetArticle, string oldPath, string oldDisplayName, ref bool found)
        {
            var regex = new Regex(string.Format(EXISTINGHTMLLINKFORMATPATTERN, Regex.Escape(oldPath)), RegexOptions.Multiline);

            return ReplaceWithExistingLinks(regex, articleData, targetArticle, oldDisplayName, ref found);
        }

        private static string ReplaceWithExistingLinks(Regex regex, string articleData, WikiArticle targetArticle, string oldDisplayName, ref bool found)
        {
            var index = 0;

            while (true)
            {
                var match = regex.Match(articleData, index);
                if (!match.Success)
                    break;

                //if the link title equals with the old display name, change it to the new one
                var lTitle = match.Groups["LinkTitle"].Value;
                if (string.IsNullOrEmpty(lTitle) || lTitle.CompareTo(oldDisplayName ?? string.Empty) == 0)
                    lTitle = targetArticle.DisplayName;

                var templateValue = GetExistingLinkHtml(targetArticle.Path, lTitle);

                articleData = articleData.Remove(match.Index, match.Length)
                    .Insert(match.Index, templateValue);

                found = true;

                index = match.Index + templateValue.Length;

                if (index >= articleData.Length)
                    break;
            }

            return articleData;
        }

        private static string ReplaceExistingLinksWithAddLinks(string articleData, Node parent, string targetPath, string wikiTitle, ref bool found)
        {
            var regex = new Regex(string.Format(EXISTINGHTMLLINKFORMATPATTERN, Regex.Escape(targetPath)), RegexOptions.Multiline);
            var index = 0;

            while (true)
            {
                var match = regex.Match(articleData, index);
                if (!match.Success)
                    break;

                var lTitle = match.Groups["LinkTitle"].Value;
                if (string.IsNullOrEmpty(lTitle))
                    lTitle = wikiTitle;

                var templateValue = GetAddLinkHtml(parent, wikiTitle, lTitle);

                articleData = articleData.Remove(match.Index, match.Length)
                    .Insert(match.Index, templateValue);

                found = true;

                index = match.Index + templateValue.Length;

                if (index >= articleData.Length)
                    break;
            }

            return articleData;
        }
    }
}
