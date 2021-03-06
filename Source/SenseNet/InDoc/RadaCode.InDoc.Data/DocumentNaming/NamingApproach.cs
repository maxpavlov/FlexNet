﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Linq;
using RadaCode.InDoc.Data.DocumentNaming.SpecialNamings;
using RadaCode.InDoc.Data.Extensions;

namespace RadaCode.InDoc.Data.DocumentNaming
{   
    [Table("NamingApproaches")]
    public class NamingApproach
    {
        [Key]
        public string TypeName { get; set; }

        public string Format { get; set; }

        public string CurrentParamsCounters { get; set; }

        public DateTime UpdateTime { get; set; }

        public List<string> NameBlocks
        {
            get
            {
                var res = new List<string>();
                var format = Format;

                var moreToParse = true;

                while (moreToParse)
                {
                    var braceStart = format.IndexOf("{");

                    if (braceStart == -1)
                    {
                        var lastToAdd = format.Substring(0, format.Length);
                        if(!String.IsNullOrEmpty(lastToAdd)) res.Add(lastToAdd);
                        break;
                    }
                        
                    var braceEnd = format.IndexOf("}", braceStart);

                    var beforeToAdd = format.Substring(0, braceStart);
                    if(!String.IsNullOrEmpty(beforeToAdd)) res.Add(beforeToAdd);

                    var bracesWithContents = format.Substring(braceStart, braceEnd - braceStart + 1);
                    if(!String.IsNullOrEmpty(bracesWithContents)) res.Add(bracesWithContents);


                    if (braceEnd + 1 == format.Length) break;
                    format = format.Substring(braceEnd + 1);
                }

                return res;
            }
        } 
        public List<string> ParamBlocks
        {
            get
            {
                var res = new List<string>();
                var sI = 0;
                for (var i = 0; i < NameBlocks.Count; i++)
                {
                    while(NameBlocks[i][0] != '{')
                    {
                        res.Add(string.Empty);
                        i++;
                    }
                    res.Add(GetCurrentValueByIndex(sI));
                    sI++;
                }

                return res;
            }
        } 

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

                var processor = SpecialNamingsFactory.GetNamingProcessor(this, specialAreaContents);
                processor.ProcessGetNextForNaming(ref res, braceStart, braceEnd, codeIndex);
                codeIndex++;

                moreToParse = res.IndexOf("{") != -1;

            } while (moreToParse);

            UpdateTime = DateTime.UtcNow;
            return res;
        }

        private string GetCurrentValueByIndex(int codeIndex)
        {
            var paramsStruct = XElement.Parse(CurrentParamsCounters);

            var record =
                paramsStruct.Descendants("ParamPair").FirstOrDefault(pair => pair.Element("Index").Value == codeIndex.ToString());

            return record != null ? record.Element("Value").Value : string.Empty;
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
