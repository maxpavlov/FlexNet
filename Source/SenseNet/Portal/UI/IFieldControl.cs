using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using SenseNet.Portal.UI.Controls;
using SenseNet.ContentRepository;


namespace SenseNet.Portal.UI
{
	public interface IFieldControl
	{
		[PersistenceMode(PersistenceMode.Attribute)]
		string FieldName { get; set; }
		[PersistenceMode(PersistenceMode.Attribute)]
		bool ReadOnly { get; set; }
		[PersistenceMode(PersistenceMode.Attribute)]
		bool Inline { get; set; }
		[PersistenceMode(PersistenceMode.Attribute)]
		FieldControlRenderMode RenderMode { get; set; }

		Field Field { get; }

		object GetData();
		void SetData(object data);
		void ClearError();
		void SetErrorMessage(string message);

	}
}