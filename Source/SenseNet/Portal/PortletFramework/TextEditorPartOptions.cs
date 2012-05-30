using System;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.PortletFramework
{
    public enum TextEditorCommonType
    {
        SingleLine = 0,
        MultiLine,
        MiddleSize,
        Small
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TextEditorPartOptions : EditorOptions
    {
        /* ================================================================================================================= Properties */
        public TextBoxMode TextMode { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int MaxLength { get; set; }


        /* ================================================================================================================= Common constructors */
        public TextEditorPartOptions()
        {
            this.TextMode = TextBoxMode.SingleLine;
            this.Rows = 1;
            this.Columns = 30;
        }
        public TextEditorPartOptions(TextEditorCommonType commonType)
        {
            switch (commonType)
            {
                case TextEditorCommonType.SingleLine:
                    this.TextMode = TextBoxMode.SingleLine;
                    this.Rows = 1;
                    this.Columns = 30;
                    break;
                case TextEditorCommonType.MultiLine:
                    this.TextMode = TextBoxMode.MultiLine;
                    this.Rows = 10;
                    this.Columns = 30;
                    break;
                case TextEditorCommonType.MiddleSize:
                    this.TextMode = TextBoxMode.MultiLine;
                    this.Rows = 4;
                    this.Columns = 30;
                    break;
                case TextEditorCommonType.Small:
                    this.TextMode = TextBoxMode.SingleLine;
                    this.Columns = 10;
                    this.MaxLength = 10;
                    break;
            }
        }
    }
}
