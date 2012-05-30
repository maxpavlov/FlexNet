using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.Search;

namespace SenseNet.ApplicationModel
{
    public class ContentLinkBatchAction : OpenPickerAction
    {
        protected override string TargetActionName
        {
            get { return "ContentLinker"; }
        }

        protected override string TargetParameterName
        {
            get { return "ids"; }
        }

        protected override string GetIdList()
        {
            return GetIdListMethod();
        }
    }
}
