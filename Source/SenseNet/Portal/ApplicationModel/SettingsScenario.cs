using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ApplicationModel
{
    public class SettingsScenario : GenericScenario
    {
        public override string Name
        {
            get
            {
                return "Settings";
            }
        }
    }
}
