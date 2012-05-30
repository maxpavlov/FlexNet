using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;

namespace SenseNet.ContentRepository
{
    public interface IDownloadCounter
    {
        void Increment(int fileId);
        void Increment(string filePath);
    }

    internal class DefaultDownloadCounter : IDownloadCounter
    {
        public void Increment(int fileId) { }
        public void Increment(string filePath) { }
    }

    public class DownloadCounter
    {
        static IDownloadCounter _instance;
        private static object _lockObject = new object();

        static IDownloadCounter Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            var instance = TypeHandler.GetTypesByInterface(typeof (IDownloadCounter))
                                .Where(t => t != typeof (DefaultDownloadCounter)).FirstOrDefault();

                            _instance = instance == null
                                            ? new DefaultDownloadCounter()
                                            : (IDownloadCounter) Activator.CreateInstance(instance);
                        }
                    }
                }

                return _instance;
            }
        }

        public static void Increment(int fileId)
        {
            if (Repository.DownloadCounterEnabled)
                Instance.Increment(fileId);
        }

        public static void Increment(string filePath)
        {
            if (Repository.DownloadCounterEnabled)
                Instance.Increment(filePath);
        }
    }
}
