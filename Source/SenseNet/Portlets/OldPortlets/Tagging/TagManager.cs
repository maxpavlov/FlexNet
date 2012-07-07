using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Index;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search;
using SenseNet.Search.Indexing;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.Portlets
{
    /// <summary>
    /// <c>TagManager</c> class is for managing tags on contents with Lucene.Net accelleration.
    /// Implements back end for tag-related user controls and portlets.
    /// </summary>
    public static class TagManager
    {
        /// <summary>
        /// Returns the occurrencies of every used tag.
        /// </summary>
        /// <returns>Dictionary key: Tag; Value: overall occurrency on content.</returns>
        public static Dictionary<string, int> GetTagOccurrencies()
        {
            return GetTagOccurrencies(null, null);
        }

        /// <summary>
        /// Gets the tag occurrencies.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <param name="types">The types.</param>
        /// <returns>Dictionary key: Tag; Value: overall occurrency on content.</returns>
        public static Dictionary<string, int> GetTagOccurrencies(string[] paths, string[] types)
        {
            using (var readerFrame = LuceneManager.GetIndexReaderFrame())
            {
                var reader = readerFrame.IndexReader;

                var pathList = new List<string>();
                var typeList = new List<string>();

                if (types != null)
                {
                    typeList.AddRange(types.Select(type => type.ToLower()));
                }
                if (paths != null)
                {
                    pathList.AddRange(paths.Select(path => path.ToLower()));
                }



                var occ = new Dictionary<string, int>();
                var terms = reader.Terms(new Term("Tags", "*"));

                do
                {
                    if (terms.Term().Field() == "Tags")
                    {
                        var docs = reader.TermDocs(terms.Term());
                        var count = 0;


                        while (docs.Next())
                        {
                            var pathValid = false;



                            var doc = reader.Document(docs.Doc()); //lucene document to examine for search criterias

                            if (pathList.Count() > 0)
                            {
                                if (pathList.Any(path => doc.Get("Path").StartsWith(path)))
                                {
                                    pathValid = true;
                                }
                            }
                            else
                            {
                                pathValid = true;
                            }

                            var typeValid = typeList.Count() <= 0 || typeList.Contains(doc.Get("Type"));


                            if (typeValid && pathValid)
                            {
                                count++;

                            }
                        }


                        if (!occ.ContainsKey(terms.Term().Text().ToLower()) && count > 0)
                        {
                            occ.Add(terms.Term().Text(), count);



                        }
                    }
                } while (terms.Next());

                terms.Close();
                return occ;
            }
        }

        /// <summary>
        /// Returns the ID-s of Nodes, that contains the tag given as parameter.
        /// </summary>
        /// <param name="tag">Searched tag as String.</param>
        /// <returns>Node Id list.</returns>
        public static List<int> GetNodeIds(string tag)
        {
            return GetNodeIds(tag, new string[] { }, new string[] { });
        }

        /// <summary>
        /// Returns the ID-s of Nodes, that contains the tag given as parameter.
        /// </summary>
        /// <param name="tag">Searched tag as String.</param>
        /// <param name="searchPathArray">Paths to search in.</param>
        /// <param name="contentTypeArray">Types to search.</param>
        /// <returns>Node Id list.</returns>
        public static List<int> GetNodeIds(string tag, string[] searchPathArray, string[] contentTypeArray, string queryFilter = "")
        {
            if (String.IsNullOrEmpty(tag))
                return new List<int>();
            tag = tag.ToLower();
            var ids = new List<int>();
            var pathList = string.Empty;
            var typeList = string.Empty;
            if (searchPathArray.Count() > 0 && contentTypeArray.Count() > 0)
            {

                pathList = "+Path:(";
                foreach (var path in searchPathArray)
                {
                    pathList = String.Concat(pathList, path, "* ");
                }
                pathList = String.Concat(pathList, ")");

                typeList = "+Type:(";
                foreach (var path in contentTypeArray)
                {
                    typeList = String.Concat(typeList, path, " ");
                }
                typeList = String.Concat(typeList, ")");

            }

            if (tag != string.Empty)
            {
                LucQuery query;
                if (!String.IsNullOrEmpty(pathList) && !String.IsNullOrEmpty(typeList))
                {
                    query = LucQuery.Parse(string.Format("+Tags:\"{0}\" {1} {2} {3}", tag.Trim(), pathList, typeList, queryFilter));
                }
                else
                {
                    query = LucQuery.Parse(string.Format("+Tags:\"{0}\" {1}", tag.Trim(), queryFilter));
                }

                var results = query.Execute();
                ids.AddRange(results.Select(o => o.NodeId));
            }
            return ids;
        }

        /// <summary>
        /// Returns all tags currently in use and containing the given string fragment.
        /// </summary>
        /// <param name="filter">Filter string.</param>
        /// <param name="pathList">The path list.</param>
        /// <returns>
        /// List of tags in use, and containing the filter string.
        /// </returns>
        public static List<string> GetAllTags(string filter, List<string> pathList)
        {


            var tags = GetTagOccurrencies(pathList != null ? pathList.ToArray() : null, null);


            //var reader = LuceneManager.IndexReader;
            //var tags = new List<string>();
            //var terms = reader.Terms(new Term("Tags", "*"));

            //do
            //{
            //    if (terms.Term().Field() == "Tags")
            //    {
            //        if (!tags.Contains(terms.Term().Text().ToLower()) && (String.IsNullOrEmpty(filter) || terms.Term().Text().ToLower().Contains(filter.ToLower())))
            //        {
            //            tags.Add(terms.Term().Text().ToLower());
            //        }
            //    }
            //} while (terms.Next());

            //terms.Close();

            return tags.Select(tag => tag.Key).ToList();
        }

        /// <summary>
        /// Replaces the given tag to the given new tag on every content in reppository.
        /// Also used for deleting tags, by calling with newtag = String.Empty parameter.
        /// </summary>
        /// <param name="tag">Tag to replace.</param>
        /// <param name="newtag">Tag to replace with.</param>
        /// <param name="pathList">The path list.</param>
        public static void ReplaceTag(string tag, string newtag, List<string> pathList)
        {
            tag = tag.ToLower();
            newtag = newtag.ToLower();
            if (tag == string.Empty)
                return;
            var query = String.Format("Tags:\"{0}\"", tag);

            if (pathList != null && pathList.Count > 0)
            {
                query = string.Concat(query, String.Format("+InTree:({0})", pathList.Aggregate(String.Empty, (current, path) => String.Concat(current, path, " ")).TrimEnd(' ')));
            }
            var lq = LucQuery.Parse(query);
            var result = lq.Execute();
            foreach (var tmp in result.Select(item => Node.LoadNode(item.NodeId)))
            {
                if (tmp["Tags"] != null)
                {
                    tmp["Tags"] = (tmp["Tags"].ToString().ToLower().Replace(tag, newtag)).Trim();
                }
                tmp.Save();
            }
        }

        /// <summary>
        /// Determines if the given tag is blacklisted or not.
        /// </summary>
        /// <param name="tag">Tag to search on blacklist.</param>
        /// <param name="blacklistPaths">The blacklist paths.</param>
        /// <returns>True if blacklisted, false if not.</returns>
        public static bool IsBlacklisted(string tag, List<string> blacklistPaths)
        {
            LucQuery query;
            using (var readerFrame = LuceneManager.GetIndexReaderFrame())
            {
                var reader = readerFrame.IndexReader;
                var terms = reader.Terms(new Term("BlackListPath", "*"));

                if (blacklistPaths != null && blacklistPaths.Count > 0)
                {
                    var usedPaths = new List<string>();

                    do
                    {
                        if (terms.Term().Field() == "BlackListPath")
                        {
                            if (blacklistPaths.Any(path => path.ToLower().StartsWith(terms.Term().Text())))
                            {
                                usedPaths.Add(terms.Term().Text());
                            }
                        }
                    } while (terms.Next());

                    var pathString = usedPaths.Aggregate(string.Empty, (current, usedPath) => String.Concat(current, usedPath, " "));

                    pathString = pathString.TrimEnd(' ');

                    var queryString = String.Format("+Type:tag +DisplayName:\"{0}\"", tag.ToLower());

                    queryString = String.Concat(queryString, String.IsNullOrEmpty(pathString) ? " +BlackListPath:/root" : String.Format(" +BlackListPath:({0})", pathString));

                    query = LucQuery.Parse(queryString);

                }
                else
                {
                    query = LucQuery.Parse(String.Concat("+Type:tag +DisplayName:\"", tag.ToLower(), "\" +BlackListPath:/root"));
                }

                var result = query.Execute();
                return (result.Count() > 0);
            }
        }

        /// <summary>
        /// Manages the blacklist.
        /// </summary>
        /// <param name="add">if set to <c>true</c> [add to blacklist] else remove from it.</param>
        /// <param name="tagId">The tag id.</param>
        /// <param name="pathList">The blacklist path list.</param>
        public static void ManageBlacklist(bool add, int tagId, List<string> pathList)
        {
            var tagNode = Node.LoadNode(tagId);
            var nodePaths = tagNode["BlackListPath"] as string;
            if (nodePaths == null) nodePaths = "";

            var blacklistPaths = nodePaths.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var path in pathList)
            {
                if (add && !blacklistPaths.Contains(path))
                {
                    blacklistPaths.Add(path);
                }
                else if (blacklistPaths.Contains(path))
                {
                    blacklistPaths.Remove(path);
                }
            }

            tagNode["BlackListPath"] = blacklistPaths.Aggregate(string.Empty, (current, blacklistPath) => String.Concat(current, blacklistPath, ",")).TrimEnd(',');
            tagNode.Save();
        }

        /// <summary>
        /// Determines if the tag given as parameter is presented in content repository or not.
        /// </summary>
        /// <param name="tag">Tag to search.</param>
        /// <param name="tagPath">The tag path.</param>
        /// <returns>True is present, false if not.</returns>
        public static bool IsInRepository(string tag, string tagPath)
        {
            var query = LucQuery.Parse(String.Concat("+Type:tag +DisplayName:\"", tag.ToLower(), "\""));
            var result = query.Execute();
            return (result.Count() > 0);
        }

        /// <summary>
        /// Stores the given tag into the content repository as Tag node type.
        /// </summary>
        /// <param name="tag">Tag to store.</param>
        /// <param name="path">The folder path in repository, where you want to save the stored Tag node.</param>
        public static void AddToRepository(string tag, string path)
        {
            tag = tag.ToLower();
            var parentNode = Node.LoadNode(path);
            var subFolderName = ContentNamingHelper.GetNameFromDisplayName("", tag);
            subFolderName = subFolderName.Replace('.', '_').Replace('"', '_').Replace('\'', '_');
            subFolderName = subFolderName.Length > 1 ? subFolderName.Substring(0, 2) : subFolderName;
            var fullPath = RepositoryPath.Combine(path, subFolderName);

            if (!Node.Exists(fullPath))
            {
                var cnt = ContentRepository.Content.CreateNew("Folder", parentNode, subFolderName);
                cnt.Save();
                parentNode = cnt.ContentHandler;
            }
            else
            {
                parentNode = Node.LoadNode(fullPath);
            }
            var tagfileName = ContentNamingHelper.GetNameFromDisplayName("", tag);
            tagfileName = tagfileName.Replace('.', '_').Replace('"', '_').Replace('\'', '_');
            var newTag = ContentRepository.Content.CreateNew("Tag", parentNode, UrlNameValidator.ValidUrlName(tagfileName));
            newTag["DisplayName"] = tag.ToLower();
            newTag["TrashDisabled"] = true;
            newTag["CreationDate"] = DateTime.Now;

            newTag.Save();
        }

        /// <summary>
        /// Implements syncing between Lucene Index and Content Repository.
        /// - Cleans up not referenced tag Nodes (not used on any content)
        /// - Imports new tags which are not stored as Tag node in repository.
        /// - Optimizes Lucene Index for accurate search results.
        /// </summary>
        /// <param name="tagPath">Repository path where to import new tags.</param>
        /// <param name="searchPaths">The search paths.</param>
        public static void SynchronizeTags(string tagPath, List<string> searchPaths)
        {
            //1. adding new tags to repositry
            var usedTags = GetAllTags(null, searchPaths);

            if (tagPath != string.Empty)
            {
                foreach (var tag in usedTags)
                {
                    if (!IsInRepository(tag, tagPath.ToLower()))
                    {
                        AddToRepository(tag.ToLower(), tagPath);
                    }
                }
            }
            //2. deleting unused tag nodes
            var query = LucQuery.Parse("+Type:tag");
            var result = query.Execute();
            var nodes = result.Select(o => Node.LoadNode(o.NodeId)).ToList();


            foreach (var node in nodes)
            {
                if (!usedTags.Contains(node.DisplayName) && string.IsNullOrWhiteSpace(node.GetProperty<string>("BlackListPath")))
                {
                    node.Delete();
                }
            }
        }

        /// <summary>
        /// Gets the tag classes.
        /// </summary>
        /// <returns>List of classes</returns>
        public static List<KeyValuePair<string, int>> GetTagClasses()
        {
            return GetTagClasses(null, null);
        }

        /// <summary>
        /// Works like GetAllTags, but gives tag class (1-10) to tags, based on overall occurrencies.
        /// Used for TagCloud.
        /// </summary>
        /// <returns>List of tag classes.</returns>
        public static List<KeyValuePair<string, int>> GetTagClasses(string[] searchPaths, string[] contentTypes)
        {
            return GetTagClasses(searchPaths, contentTypes, 0);
        }

        /// <summary>
        /// Gets the tag classes.
        /// </summary>
        /// <param name="searchPaths">The search paths.</param>
        /// <param name="contentTypes">The content types.</param>
        /// <param name="top">Count of requested tags.</param>
        /// <returns>List of classes</returns>
        public static List<KeyValuePair<string, int>> GetTagClasses(string[] searchPaths, string[] contentTypes, int top)
        {
            Dictionary<string, int> occ;
            if (searchPaths != null && contentTypes != null)
            {
                occ = GetTagOccurrencies(searchPaths, contentTypes);
            }
            else
            {
                occ = GetTagOccurrencies();
            }

            var maxCount = 0;

            foreach (var o in occ)
            {
                if (o.Value > maxCount)
                {
                    maxCount = o.Value;
                }
            }

            var rnd = new Random();

            var classes = (from oc in occ
                           let tepmValue = (oc.Value / (float)(maxCount)) * 10
                           let value = (int)(tepmValue)
                           orderby rnd.Next()
                           select new KeyValuePair<string, int>(oc.Key, Math.Max(value, 1))).ToList();

            // If all tags are requested for tagcloud.
            if (top <= 0)
            {
                return classes;
            }

            // If a specified number of tags (top) are requsted for tagcloud.
            classes.Clear();

            var sortedOcc = (from entry in occ orderby entry.Value descending select entry).Take(top);

            classes.AddRange(from keyValuePair in sortedOcc
                             let tepmValue = (keyValuePair.Value / (float)(maxCount)) * 10
                             let value = (int)(tepmValue)
                             orderby rnd.Next()
                             select new KeyValuePair<string, int>(keyValuePair.Key, Math.Max(value, 1)));

            return classes;
        }

        /// <summary>
        /// Gets the tag classes.
        /// </summary>
        /// <param name="top">Count of requested tags.</param>
        /// <returns>List of classes</returns>
        public static List<KeyValuePair<string, int>> GetTagClasses(int top)
        {
            return GetTagClasses(null, null, top);
        }

    }
}

