using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Linq;
using RadaCode.InDoc.Data.Extensions;

namespace RadaCode.InDoc.Data.DocumentNaming
{   
    [Table("NamingApproaches")]
    public class NamingApproach
    {
        private static object _editSync = new Object();

        [Key]
        public string TypeName { get; set; }

        public string Format { get; set; }

        public string CurrentParamsCounters { get; set; }

        public DateTime UpdateTime { get; set; }

        #region Behavior

        public string GetNextName()
        {
            var res = Format;

            var moreToParse = false;
            var codeIndex = 0;

            do
            {
                var braceStart = res.IndexOf("{");
                var braceEnd = res.IndexOf("}", braceStart);

                var specialAreaContents = res.Substring(braceStart, braceEnd - braceStart + 1);

                switch (specialAreaContents)
                {
                    case "{intInc_G}":
                        var currentG = GetCurrentValueByIndex(codeIndex);
                        var curIntG = int.Parse(currentG);
                        curIntG++;
                        res = res.SmartReplace(specialAreaContents, curIntG.ToString(), braceStart, 1);
                        SaveNewValueByIndex(codeIndex, curIntG.ToString());
                        codeIndex++;
                        break;
                    case "{intInc_D}":
                        var currentD = GetCurrentValueByIndex(codeIndex);
                        var curIntD = int.Parse(currentD);
                        if (DateTime.UtcNow.Date == UpdateTime.Date)
                        {
                            curIntD++;
                        }
                        else curIntD = 1;
                        res = res.SmartReplace(specialAreaContents, curIntD.ToString(), braceStart, 1);
                        SaveNewValueByIndex(codeIndex, curIntD.ToString());
                        codeIndex++;
                        break;
                    case "{yy}":
                        res = res.SmartReplace(specialAreaContents, DateTime.UtcNow.ToString("yy"), braceStart, 1);
                        codeIndex++;
                        break;
                    default:
                        throw new Exception(String.Format("Unknown naming format encountered: {0}", specialAreaContents));
                }

                moreToParse = res.IndexOf("{") != -1;

            } while (moreToParse);

            UpdateTime = DateTime.UtcNow;
            return res;
        }

        private void SaveNewValueByIndex(int paramIndex, string value)
        {
            lock (_editSync)
            {
                var paramsStruct = XElement.Parse(CurrentParamsCounters);

                var record =
                    paramsStruct.Descendants("ParamPair").FirstOrDefault(
                        pair => pair.Element("Index").Value == paramIndex.ToString());

                record.Element("Value").Value = value;

                CurrentParamsCounters = paramsStruct.ToString();
            }
        }

        private string GetCurrentValueByIndex(int codeIndex)
        {
            var paramsStruct = XElement.Parse(CurrentParamsCounters);

            var record =
                paramsStruct.Descendants("ParamPair").FirstOrDefault(pair => pair.Element("Index").Value == codeIndex.ToString());

            return record.Element("Value").Value;
        }

        public void SaveCurrentParams(List<KeyValuePair<int, string>> initialParams)
        {
            var root = new XElement("Params");

            foreach (var paramPair in initialParams)
            {
                root.Add(new XElement("ParamPair",
                    new XElement("Index", paramPair.Key),
                    new XElement("Value", paramPair.Value)
                    ));
            }

            CurrentParamsCounters = root.ToString();
            UpdateTime = DateTime.UtcNow;
        }

        #endregion
    }
}
