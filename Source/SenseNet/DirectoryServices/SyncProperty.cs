using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.DirectoryServices
{
    public class SyncProperty
    {
        // the maximum allowed length of the property
        private int _maxLength;
        public int MaxLength
        {
            get { return _maxLength; }
            set { _maxLength = value; }
        }

        // indicates that the property must be unique in the domain
        // eg.: email must be unique on the portal
        // when deleted, the value of this property has to be renamed to a unique name!
        private bool _unique;
        public bool Unique
        {
            get { return _unique; }
            set { _unique = value; }
        }

        // name of the property (eg.: Name, sAMAccountName, givenName, FullName)
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}
