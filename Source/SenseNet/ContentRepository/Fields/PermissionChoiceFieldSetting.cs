using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Fields
{
    public class PermissionChoiceFieldSetting : ChoiceFieldSetting
    {
        protected override void SetDefaults()
        {
            base.SetDefaults();
            _options = ActiveSchema.PermissionTypes.Select(t => new ChoiceOption(t.Id.ToString(), t.Name)).ToList();
        }
    }
}
