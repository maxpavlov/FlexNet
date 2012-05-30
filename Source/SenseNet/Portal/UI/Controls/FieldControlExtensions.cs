using System.Web.UI;

namespace SenseNet.Portal.UI.Controls
{
    public static class FieldControlExtensions
    {
        /// <summary>    
        /// Searches recursively in this control to find a control with the name specified. Note, finds the first match
        /// and exists.
        /// </summary>
        /// <param name="root">The Control in which to begin searching.</param>    
        /// <param name="id">The ID of the control to be found.</param>    
        /// <returns>The control if it is found or null if it is not.</returns>    
        public static Control FindControlRecursive(this Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (Control c in root.Controls)
            {
                var t = FindControlRecursive(c, id);
                if (t != null)
                    return t;
            }
            return null;
        }
    }
}