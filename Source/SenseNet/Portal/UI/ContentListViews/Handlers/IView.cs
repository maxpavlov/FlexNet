using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal.UI.ContentListViews.Handlers
{
    public interface IView
    {
        void AddColumn(Column col);
        void RemoveColumn(string fullName);
        IEnumerable<Column> GetColumns();
    }
}
