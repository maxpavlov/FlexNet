using System.Xml.Serialization;

namespace SenseNet.Portal.UI.ContentListViews.Handlers
{
    public enum WrapMode { Wrap, NoWrap }

    [XmlRoot("Column")]
    public class Column
    {
        [XmlAttribute("title")]
        public string Title { get; set; }
        [XmlAttribute("fullName")]
        public string FullName { get; set; }
        [XmlAttribute("bindingName")]
        public string BindingName { get; set; }
        [XmlAttribute("icon")]
        public string Icon { get; set; }
        [XmlAttribute("index")]
        public int Index { get; set; }
        [XmlAttribute("width")]
        public int Width { get; set; }
        [XmlAttribute("hAlign")]
        public string HorizontalAlign { get; set; }
        [XmlAttribute("wrap")]
        public string Wrap { get; set; }
        [XmlIgnore]
        public bool Selected { get; set; }

        private bool _isLeadColumn;

        [XmlIgnore]
        public bool IsLeadColumn
        {
            get { return _isLeadColumn; }
            set { _isLeadColumn = value; }
        }

        [XmlAttribute("modifiers")]
        public string Modifiers
        {
            get
            {
                return _isLeadColumn ? "main" : "";
            }
            set
            {
                _isLeadColumn = (!string.IsNullOrEmpty(value) && value.Contains("main"));
            }
        }
    }
}
