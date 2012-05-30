using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Configuration;

namespace SenseNet.Portal.UI.Controls
{
	[ToolboxData("<{0}:ErrorView ID=\"ErrorView1\" runat=\"server\"></{0}:ErrorView>")]
	public class ErrorView : ErrorControl
	{
		protected override void RenderContents(HtmlTextWriter writer)
		{
            bool beginTagWritten = false;
            foreach (var field in this.ContentView.Content.Fields.Values)
            {
                if (field.ValidationResult == null)
                    continue;
                if (this.ContentView.NeedToValidate && !field.ReadOnly && !field.IsValid)
                {
                    if (!beginTagWritten)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-error-msg");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        beginTagWritten = true;
                    }
                    writer.Write(field.DisplayName);
                    writer.Write(": ");
                    writer.Write(ResolveValidationResult(field));
                    writer.WriteBreak();
                }
            }
            if (beginTagWritten)
                writer.RenderEndTag();
            if (this.ContentView.ContentException != null)
                RenderContentError(writer, this.ContentView.ContentException, this.Debug);
		}

        internal static void RenderContentError(HtmlTextWriter writer, Exception exception)
        {
            var debugSetting = ConfigurationManager.AppSettings["ShowErrorDetails"] ?? string.Empty;
            var debug = string.Compare(debugSetting.ToLower(), "true") == 0;

            RenderContentError(writer, exception, debug);
        }

		internal static void RenderContentError(HtmlTextWriter writer, Exception exception, bool debug)
		{
			if (exception == null)
				return;

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-error-msg");
		    writer.RenderBeginTag(HtmlTextWriterTag.Div);
            
            var e = exception;
            while (e != null)
            {
                writer.Write(e.Message);
                writer.WriteBreak();

                //only show inner messages if we are in debug mode);
                if (!debug)
                    break;

                e = e.InnerException;
            }

		    if (debug)
		    {
                if (exception.Data.Keys.Count > 0)
                {
                    writer.WriteBreak();

                    foreach (var key in exception.Data.Keys)
                    {
                        writer.Write(string.Format("{0}: {1}", key, exception.Data[key]));
                        writer.WriteBreak();
                    }

                    writer.WriteBreak();
                }

		        writer.Write(exception.StackTrace);
			    writer.WriteBreak();
		    }

			writer.RenderEndTag();
		}
	}
}