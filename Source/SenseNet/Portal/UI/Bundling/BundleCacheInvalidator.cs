using System;
using SenseNet.Communication.Messaging;
using SenseNet.ContentRepository.Storage.Events;

namespace SenseNet.Portal.UI.Bundling
{
    /// <summary>
    /// Tells the BundleHandler to invalidate its cached contents when appropriate.
    /// </summary>
    internal sealed class BundleCacheInvalidator : NodeObserver
    {
        [Serializable]
        internal sealed class BundleCacheInvalidatorDistributedAction : DistributedAction
        {
            public string Path { get; private set; }

            public BundleCacheInvalidatorDistributedAction(string path)
            {
                Path = path;
            }

            public override void DoAction(bool onRemote, bool isFromMe)
            {
                // Cleaning the cache for the given path
                BundleHandler.InvalidateCacheForPath(Path);
            }
        }

        protected override void OnNodeModified(object sender, NodeEventArgs e)
        {
            base.OnNodeModified(sender, e);
            InvalidateCache(sender, e);
        }

        protected override void OnNodeDeleted(object sender, NodeEventArgs e)
        {
            base.OnNodeDeleted(sender, e);
            InvalidateCache(sender, e);
        }

        protected override void OnNodeMoved(object sender, NodeOperationEventArgs e)
        {
            base.OnNodeMoved(sender, e);
            InvalidateCache(sender, e);
        }

        protected override void OnNodeDeletedPhysically(object sender, NodeEventArgs e)
        {
            base.OnNodeDeletedPhysically(sender, e);
            InvalidateCache(sender, e);
        }

        private static void InvalidateCache(object sender, NodeEventArgs e)
        {
            if (e.SourceNode is SenseNet.ContentRepository.File)
            {
                InvalidateCacheForPath(e.OriginalSourcePath);
                if (e.OriginalSourcePath != e.SourceNode.Path)
                    InvalidateCacheForPath(e.SourceNode.Path);
            }
        }

        private static void InvalidateCacheForPath(string path)
        {
            var extension = System.IO.Path.GetExtension(path);
            if (extension == ".js" || extension == ".css")
            {
                // Sending a message which'll tell everyone to clean their cache
                var action = new BundleCacheInvalidatorDistributedAction(path);
                action.Execute();
            }
        }
    }
}
