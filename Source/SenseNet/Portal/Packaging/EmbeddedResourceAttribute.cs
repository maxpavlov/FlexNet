using System;

namespace SenseNet.Packaging
{
	public abstract class EmbeddedResourceAttribute : ManifestAttribute
    {
		/// <summary>
		/// Absolute or relative embedded resource name.
		/// </summary>
        public string ResourcePath { get; set; }
    }
	public class InstallContentTypeAttribute : EmbeddedResourceAttribute
	{
	}
	public class InstallPageTemplateAttribute : EmbeddedResourceAttribute
	{
		/// <summary>
		/// Content name in the PageTemplates folder of the Sense/Net Content Repository.
		/// </summary>
		public string ContentName { get; set; }
	}
	public class InstallContentViewAttribute : EmbeddedResourceAttribute
	{
		/// <summary>If not specified then ContentName must be a full RepositoryPath</summary>
		public string ContentTypeName { get; set; }
		/// <summary>If the selected value is Custom then ContentName must be specified</summary>
		public ContentViewMode ViewMode { get; set; }
		/// <summary>Must be specified if the ViewMode is ContentViewMode.Custom</summary>
		public string ContentName { get; set; }
	}
	public class InstallContentAttribute : EmbeddedResourceAttribute
	{
		/// <summary>
		/// Container path int the Sense/Net Content Repository.
		/// </summary>
		public String RepositoryContainerPath { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public String Attachments { get; set; }
	}
	public class InstallFileAttribute : EmbeddedResourceAttribute
	{
		/// <summary>
		/// Full path int the Sense/Net Content Repository.
		/// </summary>
		public String RepositoryPath { get; set; }
	}
	public class InstallResourceAttribute : EmbeddedResourceAttribute
	{
		/// <summary>
		/// Content name in the resources folder of the Sense/Net Content Repository.
		/// </summary>
		public string ContentName { get; set; }
	}
    public class InstallDatabaseScriptAttribute : EmbeddedResourceAttribute
    {
        /// <summary>
        /// Position in the install sequence when the script will be executed.
        /// </summary>
        public PositionInSequence Running { get; set; }
    }
}
