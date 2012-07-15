using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RadaCode.InDoc.Data.DocumentNaming;

namespace RadaCode.InDoc.Core.Models
{
    public class ExistingNamingsModel
    {
        private List<NamingApproach> _existingNamings;

        public ExistingNamingsModel(List<NamingApproach> namings)
        {
            _existingNamings = namings;
        }
    }
}
