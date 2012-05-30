using System.Collections.Generic;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository.Fields
{
    public class YesNoFieldSetting : ChoiceFieldSetting
    {
        public const string YesValue = "yes";
        public const string NoValue = "no";

        public YesNoFieldSetting()
        {
            _allowMultiple = false;
            _allowExtraValue = false;

            //TODO: globalization
            _options = new List<ChoiceOption>
                           {
                               new ChoiceOption(YesValue, "Yes"), 
                               new ChoiceOption(NoValue, "No")
                           };
        }

        public override IDictionary<string, FieldMetadata> GetFieldMetadata()
        {
            var fmd = base.GetFieldMetadata();

            fmd[AllowExtraValueName].FieldSetting.Visible = false;
            fmd[AllowMultipleName].FieldSetting.Visible = false;
            fmd[OptionsName].FieldSetting.Visible = false;
            fmd[DisplayChoicesName].FieldSetting.Visible = false;
            fmd[CompulsoryName].FieldSetting.Visible = false;

            var fs = new ChoiceFieldSetting
            {
                Name = DefaultValueName,
                DisplayName = GetTitleString(DefaultValueName),
                Description = GetDescString(DefaultValueName),
                FieldClassName = typeof(ChoiceField).FullName,
                AllowMultiple = false,
                AllowExtraValue = false,
                Options = new List<ChoiceOption>(_options),
                DisplayChoice = Fields.DisplayChoice.DropDown,
                Visible = true
            };

            fmd[DefaultValueName].FieldSetting = fs;

            return fmd;
        }
    }
}
