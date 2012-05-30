using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.Search;

namespace SenseNet.ApplicationModel
{
    public class MoveBatchAction : MoveToAction
    {
        protected override string GetIdList()
        {
            return GetIdListMethod();
        }
    }
}
