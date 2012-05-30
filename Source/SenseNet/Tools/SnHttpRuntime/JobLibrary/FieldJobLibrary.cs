using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Fields;

namespace ConcurrencyTester.JobLibrary
{
    public static class FieldJobLibrary
    {
        
        private static readonly string ListDef1 = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#TestField' type='ShortText'>
			<Configuration>
				<MaxLength>100</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";

        private static readonly string ListDef2 = @"<?xml version='1.0' encoding='utf-8'?>
<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
	<Fields>
		<ContentListField name='#TestField' type='ShortText'>
			<Configuration>
				<MaxLength>200</MaxLength>
			</Configuration>
		</ContentListField>
	</Fields>
</ContentListDefinition>
";

        
        internal static AddFieldJob AddFieldJob_List1_Administrator()
        {
            return new AddFieldJob("AddFieldJob_List1_Administrator", StressTestLibrary.Paths["list1"], ListDef1, Console.Out)
            {
                SleepTime = 0,
                MaxIterationCount = 1,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }

        internal static AddFieldJob AddFieldJob_List1_ilguy()
        {
            return new AddFieldJob("AddFieldJob_List1_ilguy", StressTestLibrary.Paths["list1"], ListDef2, Console.Out)
            {
                SleepTime = 0,
                MaxIterationCount = 1,
                UserName = "ilguy",
                Domain = "Demo",
                WarmupTime = 300
            };
        }
        
        internal static AddFieldJob AddFieldJob_List2_hxn()
        {
            return new AddFieldJob("AddFieldJob_List2_hxn", StressTestLibrary.Paths["list2"], ListDef1, Console.Out)
            {
                SleepTime = 0,
                MaxIterationCount = 1,
                UserName = "hxn",
                Domain = "Demo"
            };
        }

        internal static AddFieldJob AddFieldJob_List2_robspace()
        {
            return new AddFieldJob("AddFieldJob_List2_robspace", StressTestLibrary.Paths["list2"], ListDef2, Console.Out)
            {
                SleepTime = 0,
                MaxIterationCount = 1,
                UserName = "robspace",
                Domain = "Demo",
                WarmupTime = 900
                
            };
        }
    }
}
