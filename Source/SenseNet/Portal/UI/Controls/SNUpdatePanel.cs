using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Web.UI;

// From: http://weblogs.asp.net/leftslipper/archive/2006/12/06/customizing-the-rendering-of-the-updatepanel.aspx
namespace SenseNet.Portal.UI.Controls
{
	public class SNUpdatePanel : UpdatePanel
	{
		private string _cssClass;
		private HtmlTextWriterTag _tag = HtmlTextWriterTag.Div;

		[DefaultValue("")]
		[Description("Applies a CSS style to the panel.")]
		public string CssClass
		{
			get
			{
				return _cssClass ?? "sn-updatepanel";
			}
			set
			{
				_cssClass = value;
			}
		}

		// Hide the base class's RenderMode property since we don't use it
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new UpdatePanelRenderMode RenderMode
		{
			get
			{
				return base.RenderMode;
			}
			set
			{
				base.RenderMode = value;
			}
		}

		[DefaultValue(HtmlTextWriterTag.Div)]
		[Description("The tag to render for the panel.")]
		public HtmlTextWriterTag Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		}

		protected override void RenderChildren(HtmlTextWriter writer)
		{
			if (IsInPartialRendering)
			{
				// If the UpdatePanel is rendering in "partial" mode that means
				// it's the top-level UpdatePanel in this part of the page, so
				// it doesn't render its outer tag. We just delegate to the base
				// class to do all the work.
				base.RenderChildren(writer);
			}
			else
			{
				// If we're rendering in normal HTML mode we do all the new custom
				// rendering. We then go render our children, which is what the
				// normal control's behavior is.
				writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
				if (CssClass.Length > 0)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
				}
				writer.RenderBeginTag(Tag);
				foreach (Control child in Controls)
				{
					child.RenderControl(writer);
				}
				writer.RenderEndTag();
			}
		}
	}
}