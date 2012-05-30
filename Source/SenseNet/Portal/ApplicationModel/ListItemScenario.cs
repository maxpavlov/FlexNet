using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository;

namespace SenseNet.ApplicationModel
{
    public class ListItemScenario : GenericScenario
    {
        public override string Name
        {
            get
            {
                return "ListItem";
            }
        }

        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            var actions = base.CollectActions(context, backUrl).ToList();

            return actions;
        }
    }
}
