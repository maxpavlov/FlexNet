using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.Search;

namespace SenseNet.ApplicationModel
{
    public class CopyBatchAction : CopyToAction
    {
        protected override string GetIdList()
        {
            return GetIdListMethod();
        }
        
    }
}
