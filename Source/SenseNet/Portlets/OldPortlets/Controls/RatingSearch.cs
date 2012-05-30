using System;
using System.Web.UI;
using System.Collections;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Portlets.Controls
{
    public class RatingSearch : UserControl, IRatingSearchPortlet
    {
        protected TextBox tbRatingSearchFrom;
        protected TextBox tbRatingSearchTo;
        protected ListView RatingSearchListView;
        public IEnumerable Results { get; set; }
        public string LastSearchedRatingFrom { get; set; }
        public string LastSearchedRatingTo { get; set; }

        public event RatingSearchPortlet.RatingSearchHandler RatingSearching;

        /// <summary>
        /// Initalize the view, set the last from and to value, and the result.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
           base.OnInit(e);
           tbRatingSearchFrom.Text = LastSearchedRatingFrom;
           tbRatingSearchTo.Text = LastSearchedRatingTo;
           RatingSearchListView.DataSource = Results;
           RatingSearchListView.DataBind();
        }

        /// <summary>
        /// Handle the button click on the content view, and triger the Ratingsearching event with the value of from and to.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event arguments</param>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
           // Response.Redirect("asd");
            if (RatingSearching != null)
                Response.Redirect(RatingSearching(e, tbRatingSearchFrom.Text, tbRatingSearchTo.Text));
        }
    }
}
