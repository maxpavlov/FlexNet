using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadaCode.InDoc.Data.DocumentNaming;

namespace RadaCode.InDoc.Core.Models
{
    public class ExistingNamingsModel
    {
        public List<NamingApproach> ExistingNamings { get; set; }

        public ExistingNamingsModel(List<NamingApproach> namings)
        {
            ExistingNamings = namings;
        }
    }
}
