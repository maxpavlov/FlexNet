using SenseNet.Portal.ContentExplorer;

namespace DigestEventNotifier
{
    class Notifier
    {
        static void Main(string[] args)
        {
            var path = args[0];

            if (!string.IsNullOrEmpty(path))
            {
                EventNotifier.SendDigest(path);
            }
        }
    }
}
