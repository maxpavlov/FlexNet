using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Versioning;

namespace SenseNet.ContentRepository.Fields
{
    [ShortName("Currency")]
    [DataSlot(0, RepositoryDataType.Currency, typeof(decimal), typeof(byte), typeof(Int16), typeof(Int32), typeof(Int64),
            typeof(Single), typeof(Double), typeof(SByte), typeof(UInt16), typeof(UInt32), typeof(UInt64))]
    [DefaultFieldSetting(typeof(CurrencyFieldSetting))]
    [DefaultFieldControl("SenseNet.Portal.UI.Controls.Currency")]
    public class CurrencyField : NumberField
    {
    }
}
