using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ApplicationModel
{
    class VotingSettingsScenario : SettingsScenario
    {
        public override string Name
        {
            get
            {
                return "VotingSettings";
            }
        }

        protected override IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            return base.CollectActions(context, backUrl).ToList();
        }
    }
}
