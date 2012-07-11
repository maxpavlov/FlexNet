using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RadaCode.InDoc.Data.DocumentNaming
{   
    [Table("NamingApproaches")]
    public class NamingApproach
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual Guid Id { get; set; }

        public NamingApproach(string typeName, string format, List<KeyValuePair<int,string>> initialParamsValues)
        {
            TypeName = typeName;
            Format = format;
            CurrentParamsValues = initialParamsValues;
        }

        public string TypeName { get; set; }

        public string Format { get; set; }

        public List<KeyValuePair<int, string>> CurrentParamsValues { get; set; }

        public string GetNextName()
        {
            var res = string.Empty;

            return res;
        }

        public void RegisterNameUsage(string nameUsed)
        {
            
        }

    }
}
