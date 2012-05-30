using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.ApplicationModel
{
    public class CopyAppLocalAction : ServiceAction
    {
        public override string ServiceName
        {
            get
            {
                return "ContentStore.mvc";
            }
            set
            {
                base.ServiceName = value;
            }
        }

        public override string MethodName
        {
            get
            {
                return "CopyAppLocal";
            }
            set
            {
                base.MethodName = value;
            }
        }

        public override void Initialize(Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            AddParameter("apppath", context.Path);

            //var targetAppPath = RepositoryPath.Combine(nodepath, "(apps)");
            //var targetThisPath = RepositoryPath.Combine(targetAppPath, "This");

            if (context.Path.Contains("/(apps)/This/"))
                this.Forbidden = true;
        }
    }
}
