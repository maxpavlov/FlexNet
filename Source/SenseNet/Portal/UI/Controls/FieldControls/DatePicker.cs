using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Fields;
using SenseNet.Portal.UI;

using SenseNet.ContentRepository;
using System.Globalization;

namespace SenseNet.Portal.UI.Controls
{


    [ToolboxData("<{0}:DatePicker ID=\"DatePicker1\" runat=server></{0}:DatePicker>")]
    public class DatePicker : FieldControl, INamingContainer, ITemplateFieldControl
    {

        #region private DateControls for providing dropdown date chooser scenarios.

        private class DateControls : Control
        {
            // Members ////////////////////////////////////////////////////////
            internal static readonly string YearControlId = "YearControl";
            internal static readonly string MonthControlId = "MonthControl";
            internal static readonly string DayControlId = "DayControl";

            private DropDownList yearList;
            private DropDownList monthList;
            private DropDownList dayList;

            // Properties /////////////////////////////////////////////////////
            public string StartYear { get; set; }
            public string EndYear { get; set; }
            public string SelectedYear
            {
                get { return yearList.SelectedValue; }
            }
            public string SelectedMonth
            {
                get { return monthList.SelectedValue; }
            }
            public string SelectedDay
            {
                get { return dayList.SelectedValue; }
            }

            // Methods ////////////////////////////////////////////////////////
            public ListItem[] GetYears()
            {
                var result = new List<ListItem>();

                AmendProperties();

                var startYearNum = Convert.ToInt16(StartYear);
                var endYearNum = Convert.ToInt16(EndYear);

                // correction for user inadvertence :)
                if (Math.Min(startYearNum, endYearNum) != startYearNum)
                {
                    endYearNum = Convert.ToInt16(StartYear);
                    startYearNum = Convert.ToInt16(EndYear);
                }
                for (var i = startYearNum; i <= endYearNum; i++)
                    result.Add(new ListItem(i.ToString()));

                return result.ToArray();
            }
            public ListItem[] GetMonths()
            {
                var result = new List<ListItem>();

                foreach (var month in DateTimeFormatInfo.CurrentInfo.MonthNames)
                    result.Add(new ListItem(month));

                return result.ToArray();
            }
            public ListItem[] GetDays()
            {
                var result = new List<ListItem>();
                for (var i = 1; i < 32; i++)
                    result.Add(new ListItem(i.ToString()));
                return result.ToArray();
            }

            // Events /////////////////////////////////////////////////////////
            protected override void CreateChildControls()
            {
                Controls.Clear();

                yearList = new DropDownList { ID = YearControlId };
                monthList = new DropDownList { ID = MonthControlId };
                dayList = new DropDownList { ID = DayControlId };

                
                yearList.Items.Clear();
                monthList.Items.Clear();
                dayList.Items.Clear();
                
                yearList.Items.AddRange(GetYears());
                monthList.Items.AddRange(GetMonths());
                dayList.Items.AddRange(GetDays());
                
                Controls.Add(dayList);
                Controls.Add(monthList);
                Controls.Add(yearList);

                monthList.Attributes.Add("onchange", String.Format("getMonthLength('{0}','{1}','{2}')", yearList.ClientID, monthList.ClientID, dayList.ClientID));
                yearList.Attributes.Add("onchange", String.Format("getMonthLength('{0}','{1}','{2}')", yearList.ClientID, monthList.ClientID, dayList.ClientID));

                ChildControlsCreated = true;
            }

            // Internals //////////////////////////////////////////////////////
            private void AmendProperties()
            {
                StartYear = String.IsNullOrEmpty(StartYear) ? "1901" : StartYear;
                EndYear = String.IsNullOrEmpty(EndYear) ? "2040" : EndYear;
            }

        }

        #endregion

        // Members //////////////////////////////////////////////////////////////////////
        protected string TimeControlID = "InnerTimeTextBox";
        public event EventHandler OnDateChanged;
        private readonly TextBox _dateTextBox;
        private readonly TextBox _timeTextBox;

        private string _dateTimeText;
        private string _timeText;
        private string _configuration = "{format:'Y.m.d',allowBlank:true}"; // Default configuration
        //private string _serverDateFormat = "yyyy.MM.dd"; // Default server date format
        private string _serverDateFormat = string.Empty;
        

        #region properties

        [PersistenceMode(PersistenceMode.Attribute)]
        public string Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }
        [PersistenceMode(PersistenceMode.Attribute)]
        public string ServerDateFormat
        {
            get { return _serverDateFormat; }
            set { _serverDateFormat = value;}
        }
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool AutoPostBack
        {
            get { return _dateTextBox.AutoPostBack; }
            set { _dateTextBox.AutoPostBack = value; }
        }
        
        
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool ScriptDisabled { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool jQueryPickerEnabled { get; set; }
        [Obsolete("Ext is not supported, use jQueryPickerEnabled instead")]
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool ExtJSPickerEnabled { get; set; }

        // TODO: for future development, when dropdown date choosers will come alive
        private bool ChooseModeEnabled { get; set; }
        #endregion

        private DateTimeMode Mode
        {
            get
            {
                var fieldSetting = Field.FieldSetting as DateTimeFieldSetting;
                if (fieldSetting == null)
                    return DateTimeMode.None;

                return !fieldSetting.DateTimeMode.HasValue ? DateTimeMode.None : fieldSetting.DateTimeMode.Value;
            }
        }

        // Constructor //////////////////////////////////////////////////////////////////
        public DatePicker()
        {
            _dateTextBox = new TextBox {ID = InnerControlID};
            _timeTextBox = new TextBox {ID = TimeControlID};
            AutoPostBack = false;
            ScriptDisabled = false;
            jQueryPickerEnabled = true;
            //
            //  TODO: Half-made stuff: ChooseMode indicates whethe the control renders dropdown choosers for selecting year, month, day value. It must be completed in the next release.
            //
            ChooseModeEnabled = false;
        }
        
        // Methods //////////////////////////////////////////////////////////////////////
        public override void SetData(object data)
        {
            switch (Mode)
            {
                case DateTimeMode.None:
                    // only one textbox appears on page that handles datetime
                    // in this case, no scripts are rendered
                    ProcessNoneMode(data);
                    ScriptDisabled = true;
                    break;
                case DateTimeMode.Date:
                    // one textbox appears on page that handles Date
                    ProcessDateMode(data);
                    break;
                case DateTimeMode.DateAndTime:
                    // two textboxes appear on page, one for handling date and one for handling time values
                    ProcessDateTimeMode(data);
                    break;
                default:
                    break;
            }

            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var dateControl = GetInnerControl() as TextBox;
            var timeControl = GetTimeControl() as TextBox;

            if (title != null) title.Text = Field.DisplayName;
            if (desc != null)
            {
                desc.Text = Field.Description;
                var dateTimeFormat = System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat;
                var shortDatePattern = dateTimeFormat.ShortDatePattern;
                var timePattern = dateTimeFormat.ShortTimePattern;
                var pattern = string.Empty;
                switch (Mode)
                {
                    case DateTimeMode.None:
                    case DateTimeMode.DateAndTime:
                        var patternWithTime = HttpContext.GetGlobalResourceObject("Portal", "DateFieldDateTimeFormatDescription") as string ?? "{0} - {1}";
                        pattern = String.Format(patternWithTime, shortDatePattern, timePattern);
                        break;
                    case DateTimeMode.Date:
                        var patternWithoutTime = HttpContext.GetGlobalResourceObject("Portal", "DateFieldDateFormatDescription") as string ?? "{0}";
                        pattern = String.Format(patternWithoutTime, shortDatePattern);
                        break;
                    default:
                        break;
                }
                
                desc.Text = string.Concat(desc.Text, pattern);
            }
                
            if (dateControl != null) 
                dateControl.Text = Convert.ToString(_dateTimeText);
            if (timeControl != null && Mode == DateTimeMode.DateAndTime) 
                timeControl.Text = GetTime(data);

            #endregion

        }
        public override object GetData()
        {
            var shortDatePattern = string.IsNullOrEmpty(_serverDateFormat)
                                   ? System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern
                                   : _serverDateFormat;
            var format = new DateTimeFormatInfo { ShortDatePattern = shortDatePattern};
            
            #region without using template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
            {
                switch (Mode)
                {
                    case DateTimeMode.None:
                        // only one textbox appears on page that handles datetime
                        if (String.IsNullOrEmpty(_dateTextBox.Text))
                            return DateTime.MinValue;
                        return DateTime.Parse(_dateTextBox.Text);
                    case DateTimeMode.Date:
                        // one textbox appears on page that handles Date
                        if (String.IsNullOrEmpty(_dateTextBox.Text))
                            return DateTime.MinValue;
                        return DateTime.Parse(_dateTextBox.Text, format);
                    case DateTimeMode.DateAndTime:
                        // two textboxes appear on page, one for handling date and one for handling time values
                        if (String.IsNullOrEmpty(_dateTextBox.Text))
                            return DateTime.MinValue;
                        var time = _timeTextBox.Text;
                        if (String.IsNullOrEmpty(time))
                            return DateTime.Parse(_dateTextBox.Text, format);
                        DateTime result = Convert.ToDateTime(_dateTextBox.Text);
                        result += Convert.ToDateTime(_timeTextBox.Text).TimeOfDay;
                        return result;
                    default:
                        break;
                }
            }

            #endregion
            
            #region using template controls

            var innerDateTextBox = GetInnerControl() as TextBox;
            var innerTimeTextBox = GetTimeControl() as TextBox;
            string innerDateValue = null;
            string innerTimeValue = null;

            // two textboxes appear on page, one for handling date and one for handling time values
            if (innerDateTextBox != null)
                innerDateValue = innerDateTextBox.Text;
            else
                innerDateValue = _dateTextBox.Text;

            if (innerTimeTextBox != null)
                innerTimeValue = innerTimeTextBox.Text;
            else
                innerTimeValue = _timeTextBox.Text;

            switch (Mode)
            {
                case DateTimeMode.None:
                    if (string.IsNullOrEmpty(innerDateValue))
                        return null;
                    return DateTime.Parse(innerDateValue);
                case DateTimeMode.Date:
                    if (string.IsNullOrEmpty(innerDateValue))
                        return null;
                    return DateTime.Parse(innerDateValue, format);
                case DateTimeMode.DateAndTime:
                    if (string.IsNullOrEmpty(innerDateValue) && string.IsNullOrEmpty(innerTimeValue))
                        return null;

                    DateTime date;
                    TimeSpan time;
                    if (string.IsNullOrEmpty(innerDateValue))
                        date = DateTime.Today;
                    else
                        date = Convert.ToDateTime(innerDateValue);
                    if (string.IsNullOrEmpty(innerTimeValue))
                        time = DateTime.Today.TimeOfDay;
                    else
                        time = Convert.ToDateTime(string.Format("{0} {1}", DateTime.Today.ToShortDateString(), innerTimeValue)).TimeOfDay;
                    var result = date + time;
                    return result;
                default:
                    break;
            }

            #endregion

            return DateTime.Parse(_dateTextBox.Text, format);
        }

        // Events ///////////////////////////////////////////////////////////////////////
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                if (!AutoPostBack)
                    return;

                var ic = GetInnerControl() as TextBox;
                if (ic != null)
                    ic.TextChanged += _inputTextBox_TextChanged;
                return;
            }
            _dateTextBox.CssClass = "sn-ctrl sn-ctrl-text sn-ctrl-date";
            _timeTextBox.CssClass = "sn-ctrl sn-ctrl-text sn-ctrl-time";
            if (AutoPostBack)
                _dateTextBox.TextChanged += new EventHandler(_inputTextBox_TextChanged);
            
            Controls.Add(_dateTextBox);
            
            if (Mode == DateTimeMode.DateAndTime)
                Controls.Add(_timeTextBox);
        }
        protected void _inputTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.OnDateChanged != null)
                this.OnDateChanged(this, e);
        }
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (UseBrowseTemplate)
            {
                base.RenderContents(writer);
                return;
            }
            if (UseEditTemplate)
            {
                ManipulateTemplateControls();
                base.RenderContents(writer);
                return;
            }
            if (UseInlineEditTemplate)
            {
                ManipulateTemplateControls();
                base.RenderContents(writer);
                return;
            }
            if (this.RenderMode == FieldControlRenderMode.Browse)
                RenderSimple(writer);
            else
                RenderEditor(writer);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!ScriptDisabled)
                AddDatePickerScriptToPage();

            base.OnPreRender(e);
        }

        #region ITemplateFieldControl Members
        public Control GetInnerControl() { return this.FindControlRecursive(InnerControlID); }
        public Control GetLabelForDescription() { return this.FindControlRecursive(DescriptionControlID); }
        public Control GetLabelForTitleControl() { return this.FindControlRecursive(TitleControlID); }

        public Control GetTimeControl() {
            return this.FindControlRecursive(TimeControlID); }
        
        #endregion

        // Internals ////////////////////////////////////////////////////////////////////
        private void ManipulateTemplateControls()
        {
            var ic = GetInnerControl() as TextBox;
            var timeControl = GetTimeControl() as TextBox;
            if (ic == null) return;
            if (Field.ReadOnly)
            {
                ic.Enabled = false;
                ic.EnableViewState = true;
                if (timeControl != null) timeControl.Enabled = false;
            }
            else if (ReadOnly)
            {
                ic.Enabled = false;
                ic.EnableViewState = true;
                if (timeControl != null) timeControl.Enabled = false;
            }
            if (timeControl != null && Mode == DateTimeMode.Date)
            {
                var timeControlPlaceHolder = this.FindControlRecursive("InnerTimeHolder");
                if (timeControlPlaceHolder != null)
                    timeControlPlaceHolder.Visible = false;
            }
                
            if (RenderMode != FieldControlRenderMode.InlineEdit) 
                return;
            var altText = String.Concat(Field.DisplayName, " ", Field.Description);
            ic.Attributes.Add("Title", altText);
        }
        private void AddDatePickerScriptToPage()
        {
            if (!jQueryPickerEnabled)
                return;

            UITools.AddScript(UITools.ClientScriptConfigurations.JQueryUIPath);
            UITools.AddStyleSheetToHeader(Page.Header,UITools.ClientScriptConfigurations.jQueryCustomUICssPath);

            var datePickerScript = GetDatePickerScript();
            var ic = GetInnerControl() as TextBox;
            UITools.RegisterStartupScript("datepickerex_" + (ic != null ? ic.ClientID :_dateTextBox.ClientID), datePickerScript, Page);
        }

        private string GetDatePickerScript()
        {
            var ic = GetInnerControl() as TextBox;
            var clientId = (ic != null ? ic.ClientID : _dateTextBox.ClientID);
            var datePickerScript = string.Empty;
            
            if (jQueryPickerEnabled)
            {
                datePickerScript = @"$(""#" + clientId + @""").datepicker(" + Configuration + ");";
                return datePickerScript;
            }

            return datePickerScript;
        }

        #region old school

        private void RenderSimple(TextWriter writer) { writer.Write(_dateTimeText); }
        private void RenderEditor(HtmlTextWriter writer)
        {
            if (RenderMode == FieldControlRenderMode.InlineEdit)
            {
                var altText = String.Concat(this.Field.DisplayName, " ", this.Field.Description);
                _dateTextBox.Attributes.Add("Title", altText);
            }
            if (Field.ReadOnly)
            {
                // label
                writer.Write(_dateTextBox.Text);
            }
            else if (ReadOnly)
            {
                // render readonly control
                _dateTextBox.Enabled = !this.ReadOnly;
                _dateTextBox.EnableViewState = false;
                _dateTextBox.RenderControl(writer);
            }
            else
            {
                #region commented out defaultvalue conditions, if u want it back, remove the // marks and run tests.

//                if (DefaultValue != null && this.Field.Content.Id == 0) // If the content is not yet created then DateTime default value can be set
//                {
//                    if (DefaultValue.ToLower() == "now")
//                    {
//                        _dateTextBox.Text = DateTime.Now.ToString();
//                    }
//                    else
//                    {
//                        DateTime dateTime;
//                        if (DateTime.TryParse(DefaultValue, out dateTime))
//                            _dateTextBox.Text = dateTime.ToString();
//                    }
//                }

                #endregion
                // render read/write control
                _dateTextBox.RenderControl(writer);
                if (Mode  == DateTimeMode.DateAndTime)
                    _timeTextBox.RenderControl(writer);
            }
        }

        #endregion

        private static string GetTime(object data)
        {
            if (data == null) 
                throw new ArgumentNullException("data");

            if (data.GetType() == typeof(DateTime))
            {
                var dateTimeData = (DateTime) data;
                if (dateTimeData == DateTime.MinValue || dateTimeData == System.Data.SqlTypes.SqlDateTime.MinValue)
                    return string.Empty;
                return ((DateTime) data).TimeOfDay.ToString();
            }

            var result = Convert.ToDateTime(data).TimeOfDay;
            return result.ToString();
        }
        private void ProcessDateTimeMode(object data)
        {
            ProcessDateMode(data);
            TimeSpan timeValue;
            if (data == null)
            {
                _timeText = null;
            }
            else
            {
                var dateTimeValue = Convert.ToDateTime(data);
                if (dateTimeValue == DateTime.MinValue || dateTimeValue == System.Data.SqlTypes.SqlDateTime.MinValue) {
                    _timeText = string.Empty;
                }
                else {
                    timeValue = dateTimeValue.TimeOfDay;
                    _timeText = timeValue.ToString();
                }

                
            }
            _timeTextBox.Text = Convert.ToString(_timeText);
        }
        private void ProcessDateMode(object data)
        {
            if (data == null)
                _dateTimeText = null;
            else
            {
                var isDateTime = data.GetType() == typeof(DateTime);
                if (isDateTime)
                {
                    var dateTimeValue = Convert.ToDateTime(data);
                    if (dateTimeValue == DateTime.MinValue || dateTimeValue == System.Data.SqlTypes.SqlDateTime.MinValue) {
                        _dateTimeText = string.Empty;
                    }
                    else
                        _dateTimeText = dateTimeValue.ToShortDateString();
                        
                } else 
                    _dateTimeText = data.ToString();
            }

            _dateTextBox.Text = Convert.ToString(_dateTimeText);
        }
        private void ProcessNoneMode(object data)
        {
            if (data == null)
                _dateTimeText = null;
            else
            {
                var isDateTime = data.GetType() == typeof(DateTime);
                _dateTimeText = isDateTime ? Convert.ToDateTime(data).ToString() : data.ToString();
            }

            _dateTextBox.Text = Convert.ToString(_dateTimeText);
        }
    }
}
