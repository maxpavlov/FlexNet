using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Packaging
{
	/// <summary>
	/// If the selected value is Custom then ContentName must be specified
	/// </summary>
	public enum ContentViewMode
	{
		Custom, Edit, New, Browse, InlineEdit, Grid, InlineNew, Query
	}
    public enum InstallStepCategory
    {
        None, Assembly, ContentType, Content, DbScript
    }
    public enum PositionInSequence
    {
        Default,
        BeforePackageValidating, AfterPackageValidating,
        BeforeCheckRequirements, AfterCheckRequirements,
        BeforeExecutables, AfterExecutables,
        BeforeContentTypes, AfterContentTypes,
        BeforeContents, AfterContents
    }

	internal enum PreviousItemState
	{
		NotInstalled, UserCreated, Installed, UserModified
	}

}
