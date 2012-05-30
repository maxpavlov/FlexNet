using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Data
{
	public enum DataOperationResult
	{
		Successful = 0, 
		//-- Copy related
		Copy_TargetContainsSameName,
		Copy_PartialOpenMinorPermission,
		Copy_ExpectedAddNewPermission,
		Copy_ContentList,
		Copy_NodeWithContentListContent,
		//-- Move related
		Move_TargetContainsSameName,
		Move_PartiallyLockedSourceTree,
		Move_PartialOpenMinorPermission,
		Move_PartialDeletePermission,
		Move_ExpectedAddNewPermission,
		Move_ContentListUnderContentList,
		Move_NodeWithContentListContentUnderContentList,
        //-- Save
        Save_NodeAlreadyExists
	}
}