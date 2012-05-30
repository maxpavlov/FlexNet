using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.Portal.Portlets.ContentHandlers;
using SenseNet.Portal.UI;
using SNC = SenseNet.ContentRepository;
using System.Linq;

namespace SenseNet.Portal.Portlets.Controls
{
	[ToolboxData("<{0}:ExportFormItems runat=server />")]
	public class ExportFormItems : WebControl
	{
		TextBox tbFrom;
		TextBox tbTo;

		Form _currentForm = null;

		Form CurrentForm
		{
			get
			{
				if (_currentForm == null && this.Parent is SingleContentView && (this.Parent as SingleContentView).Content.ContentHandler is Form)
				{
					_currentForm = (this.Parent as SingleContentView).Content.ContentHandler as Form;
				}
				return _currentForm;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			LiteralControl lastExport = CreateLastExport();

			tbFrom = new TextBox();
			tbFrom.ID = "tbFrom";
			tbFrom.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy.MM.dd");

			tbTo = new TextBox();
			tbTo.ID = "tbTo";
			tbTo.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).ToString("yyyy.MM.dd");

			Button btnExport = new Button();
			btnExport.ID = "btnExport";
			btnExport.Text = HttpContext.GetGlobalResourceObject("FormPortlet", "ExportToCsv") as string;//"Export to .csv";
			btnExport.Click += new EventHandler(btnExport_Click);

			Controls.Add(new LiteralControl(HttpContext.GetGlobalResourceObject("FormPortlet", "DateFrom") as string));
			Controls.Add(tbFrom);
			Controls.Add(new LiteralControl(HttpContext.GetGlobalResourceObject("FormPortlet", "DateTo") as string));
			Controls.Add(tbTo);
			Controls.Add(btnExport);
		    if (lastExport != null) Controls.Add(lastExport);
		}

		private LiteralControl CreateLastExport()
		{
			if (CurrentForm != null)
			{
				NodeQuery query = new NodeQuery();
				query.Add(new IntExpression(IntAttribute.ParentId, ValueOperator.Equal, CurrentForm.Id));
				query.Add(new StringExpression(StringAttribute.Name, StringOperator.EndsWith, ".csv"));
				query.Add(new TypeExpression(ActiveSchema.NodeTypes["File"]));
				query.Orders.Add(new SearchOrder(DateTimeAttribute.CreationDate, OrderDirection.Desc));
				query.Top = 1;
				var nodeList = query.Execute();

				if (nodeList.Count > 0)
				{
					Node node = nodeList.Nodes.First<Node>();
					return new LiteralControl(string.Concat("<br />", HttpContext.GetGlobalResourceObject("FormPortlet", "LastExport") as string, "<a target='_blank' href='", node.Path, "'/>", node.CreationDate.ToString("yyyy.MM.dd HH:mm:ss"), "</a>"));
				}
			}
			return null;
		}

		void btnExport_Click(object sender, EventArgs e)
		{
			if (CurrentForm != null)
			{
				string fileName = string.Concat("_", CurrentForm.Name, DateTime.Now.ToString("yyyy_MM_dd___HH_mm_ss"), ".csv");

				var csv = new SNC.File(CurrentForm);
				csv.Name = fileName;

				csv.Binary = new BinaryData();
				csv.Binary.FileName = fileName;
				csv.Binary.ContentType = "application/vnd.ms-excel";

				string text = GetCSV(CurrentForm);

				MemoryStream stream = new MemoryStream();
				StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding("windows-1250"));
				writer.Write(text);
				writer.Flush();

				csv.Binary.SetStream(stream);

				csv.Save();

				//HttpContext.Current.Response.ClearHeaders();
				//HttpContext.Current.Response.Clear();
				//HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + csv.Name);
				//HttpContext.Current.Response.AddHeader("Content-Length", csv.Binary.Size.ToString());
				//HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
				//HttpContext.Current.Response.Write(new BinaryReader(csv.Binary.GetStream()).ReadString());
				//HttpContext.Current.Response.End();

				(this.Parent as SingleContentView).OnUserAction(sender, "cancel", "Click");
			}
		}

		private string GetCSV(Form form)
		{
			StringBuilder sb = new StringBuilder();

			DateTime? fromDate = GetDate(tbFrom.Text);
			DateTime? toDate = GetDate(tbTo.Text);

			if (fromDate == null || toDate == null)
			{
				throw new Exception(HttpContext.GetGlobalResourceObject("FormPortlet", "IncorrectDateFormat") as string);
			}

			SenseNet.ContentRepository.Storage.Security.AccessProvider.ChangeToSystemAccount();

			NodeQuery query = new NodeQuery();
			query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, string.Concat(form.Path, "/")));
			query.Add(new TypeExpression(ActiveSchema.NodeTypes["FormItem"]));
			query.Add(new DateTimeExpression(DateTimeAttribute.CreationDate, ValueOperator.GreaterThanOrEqual, fromDate));
			query.Add(new DateTimeExpression(DateTimeAttribute.CreationDate, ValueOperator.LessThan, toDate));
			var result = query.Execute();

			SenseNet.ContentRepository.Storage.Security.AccessProvider.RestoreOriginalUser();

            if (result != null && result.Count > 0)
			{
				bool first = true;
                foreach (Node node in result.Nodes)
				{
					if (node is FormItem)
					{
						FormItem fi = node as FormItem;
						if (first)
						{
							CreateHeader(fi, sb);
							first = false;
						}
						CreateLine(fi, sb);
					}
				}
			}
			return sb.ToString();
		}

		private DateTime? GetDate(string dateString)
		{
			DateTime date = DateTime.MinValue;
			DateTime.TryParse(dateString, out date);
			return date == DateTime.MinValue ? null : (DateTime?)date;
		}

		private static void CreateHeader(FormItem fi, StringBuilder sb)
		{
			sb.Append("Name;CreatedBy;");

			var c = SNC.Content.Create(fi);
			foreach (KeyValuePair<string, Field> kvp in c.Fields)
			{
				Field f = kvp.Value;

				if (!f.Name.StartsWith("#"))
					continue;

                sb.Append(f.DisplayName);
				sb.Append(";");
			}

			sb.Append("\n\n");
		}

		private static void CreateLine(FormItem fi, StringBuilder sb)
		{
			sb.Append(fi.Name);
			sb.Append(";");
			sb.Append(fi.CreatedBy.Name);
			sb.Append(";");

			var c = SNC.Content.Create(fi);
			foreach (KeyValuePair<string, Field> kvp in c.Fields)
			{
				Field f = kvp.Value;

				if (!f.Name.StartsWith("#"))
					continue;

				foreach (string b in f.FieldSetting.Bindings)
                    sb.Append(GetPropertyValue(fi[b])).Append(";");
			}

			sb.Append("\n");
		}
        private static string GetPropertyValue(object propertyValue)
        {
            if (propertyValue == null)
                return string.Empty;
            return new StringBuilder(propertyValue.ToString())
                .Replace(';', ',')
                .Replace('\r', ' ')
                .Replace('\n', ' ')
                .ToString();
        }
	}
}