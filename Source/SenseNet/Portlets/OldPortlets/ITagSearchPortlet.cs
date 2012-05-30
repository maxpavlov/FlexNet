using System.Collections;

namespace SenseNet.Portal.Portlets
{
    /// <summary>
    /// Interface for TagSearchPortlet, the TagSearch view must implement it.
    /// </summary>
    interface ITagSearchPortlet
    {
        /// <summary>
        /// Get the results of query.
        /// </summary>
        IEnumerable Results { get; set; }

        /// <summary>
        /// Get the tag, which looking for.
        /// </summary>
        string LastSearchedTag { get; set; }

        /// <summary>
        /// The event of searching.
        /// </summary>
        event TagSearchPortlet.TagSearchHandler TagSearching;
    }
}
