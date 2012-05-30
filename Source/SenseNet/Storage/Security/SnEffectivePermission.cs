using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SenseNet.ContentRepository.Storage.Security
{
    [Serializable]
    public partial class SnPermission : INotifyPropertyChanged
    {

        public string Name { get; set; }

        private bool _allow;

        public bool Allow 
        {
            get
            {
                return _allow;
            }
            set
            {
                if (_allow != value)
                {
                    _allow = value;
                    NotifyPropertyChanged("Allow");
                }
            }
        }


        public bool Deny { get; set; }
        public string AllowFrom { get; set; }
        public string DenyFrom { get; set; }

        // bit    path       => checkbox
        // -----------------------------
        // false  irrelevant => editable
        // true   null       => editable
        // true   notnull    => readonly

        

        public event PropertyChangedEventHandler PropertyChanged;



        void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
