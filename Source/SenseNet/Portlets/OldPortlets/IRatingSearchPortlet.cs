using System.Collections;

namespace SenseNet.Portal.Portlets
{
    /// <summary>
    /// Interface for RatingSearchPortlet, the RatingSearch view must implement it.
    /// </summary>
    interface IRatingSearchPortlet
    {
        /// <summary>
        /// Get the results of query
        /// </summary>
        IEnumerable Results { get; set; }

        /// <summary>
        /// Get the "from" value of query
        /// </summary>
        string LastSearchedRatingFrom { get; set; }

        /// <summary>
        /// Get the "to" value of query 
        /// </summary>
        string LastSearchedRatingTo { get; set; }

        /// <summary>
        /// The event of searching
        /// </summary>
        event RatingSearchPortlet.RatingSearchHandler RatingSearching;
    }
}
