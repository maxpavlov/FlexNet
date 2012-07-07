using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Bundling
{
    /// <summary>
    /// Class for bunding multiple files into one.
    /// </summary>
    public class Bundle
    {
        private List<Tuple<string, int>> _pathsWithOrders = new List<Tuple<string, int>>();
        private string _hash = null;

        /// <summary>
        /// Gets the paths of the files to bundle.
        /// </summary>
        public IEnumerable<string> Paths
        {
            get { return _pathsWithOrders.Select(x => x.Item1); }
        }

        /// <summary>
        /// The last date when this bundle was invalidated in the cache.
        /// </summary>
        public DateTime LastCacheInvalidationDate { get; internal set; }

        /// <summary>
        /// Gets or sets the MIME type of this bundle.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets whether this bundle has been closed.
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// The checksum of this bundle, to be used in URLs. You can only get this checksum for closed bundles, to avoid mistaking an incomplete bundle for a complete one.
        /// </summary>
        public string Hash
        {
            get
            {
                if (!IsClosed)
                    throw new Exception("You can't get the hash of this bundle until it's not closed. That would lead to undesired behaviour.");

                if (!Paths.Any())
                    return string.Empty;

                if (_hash == null)
                {
                    _hash = Paths.Aggregate((sum, next) => sum + " " + next);
                    _hash = Tools.CalculateMD5(_hash);
                }

                return _hash;
            }
        }

        /// <summary>
        /// A unique string which also contains the last invalidation date and can be used as a URL for this bundle.
        /// </summary>
        public virtual string FakeFilename
        {
            get
            {
                return Hash + "_" + LastCacheInvalidationDate.ToString("yyyy.MM.dd_HH.mm.ss");
            }
        }

        /// <summary>
        /// Creates a new instance of the Bundle class.
        /// </summary>
        public Bundle()
        {
        }

        /// <summary>
        /// Closes the current bundle. It's not possible to reopen.
        /// </summary>
        public void Close()
        {
            IsClosed = true;
        }

        /// <summary>
        /// Adds a path to the bundle with the order of 0.
        /// </summary>
        /// <param name="path">The path to add to the bundle.</param>
        public void AddPath(string path)
        {
            AddPath(path, 0);
        }

        /// <summary>
        /// Adds a path to the bundle with the specified order.
        /// </summary>
        /// <param name="path">The path to add to the bundle.</param>
        /// <param name="order">The specified order.</param>
        public void AddPath(string path, int order)
        {
            if (_pathsWithOrders.Any(x => x.Item1 == path))
                return;

            if (IsClosed)
                throw new Exception("You can't add more files to the bundle when it's closed.");

            var item = Tuple.Create(path, order);
            bool foundHigherOrder = false;
            int index = -1;

            foreach (var pathWithOrder in _pathsWithOrders)
            {
                index++;
                if (pathWithOrder.Item2 > order)
                {
                    foundHigherOrder = true;
                    break;
                }
            }

            if (foundHigherOrder)
                _pathsWithOrders.Insert(index, item);
            else
                _pathsWithOrders.Add(item);
        }

        /// <summary>
        /// Combines the files in this bundle into one.
        /// </summary>
        /// <returns>The combined content of the bundle.</returns>
        public virtual string Combine()
        {
            var stringBuilder = new StringBuilder();

            foreach (var path in Paths)
            {
                var text = GetTextFromPath(path);
                if (text != null)
                {
                    stringBuilder.Append(text);
                    stringBuilder.Append("\r\n");
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets the text content from a given path and processes it. Override this method if you want to implement custom logic when each file is loaded.
        /// </summary>
        /// <param name="path">The path from which to the content should be retrieved.</param>
        /// <returns>The processed text content of the given path, or null if it was not found.</returns>
        protected virtual string GetTextFromPath(string path)
        {
            if (path[0] == '/')
            {
                // This is supposed to be a repository URL

                var fileNode = Node.Load<File>(path);

                if (fileNode != null)
                {
                    var stream = fileNode.Binary.GetStream();
                    var textContent = Tools.GetStreamString(stream);
                    return textContent;
                }
            }
            else if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                // This is a web URL

                var webClient = new WebClient();

                try
                {
                    return webClient.DownloadString(path);
                }
                catch (Exception exc)
                {
                    Logger.WriteException(exc);
                }
            }

            return null;
        }
    }
}
