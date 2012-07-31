using System;
using System.Linq;
using System.Xml.Linq;

namespace RadaCode.InDoc.Data.DocumentNaming.SpecialNamings
{
    public abstract class SpecialNamingBase
    {
        protected NamingApproach Approach { get; set; }

        protected static object _editSync = new Object();
        
        public abstract string SpecialCode { get; }

        protected SpecialNamingBase(ref NamingApproach approach)
        {
            Approach = approach;
        }

        public abstract void ProcessGetNextForNaming(ref string res, int braceStart, int braceEnd, int codeIndex);

        protected void SaveNewValueByIndex(int paramIndex, string value)
        {
            lock (_editSync)
            {
                var paramsStruct = XElement.Parse(Approach.CurrentParamsCounters);

                var record =
                    paramsStruct.Descendants("ParamPair").FirstOrDefault(
                        pair => pair.Element("Index").Value == paramIndex.ToString());

                record.Element("Value").Value = value;

                Approach.CurrentParamsCounters = paramsStruct.ToString();
            }
        }

        protected string GetCurrentValueByIndex(int codeIndex)
        {
            var paramsStruct = XElement.Parse(Approach.CurrentParamsCounters);

            var record =
                paramsStruct.Descendants("ParamPair").FirstOrDefault(pair => pair.Element("Index").Value == codeIndex.ToString());

            return record != null ? record.Element("Value").Value : string.Empty;
        }
    }
}
