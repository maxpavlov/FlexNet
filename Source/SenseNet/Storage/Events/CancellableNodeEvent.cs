using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Events
{
	public enum CancellableNodeEvent
	{
		Creating = 6, 
		Modifying = 7, 
		Deleting = 8, 
		DeletingPhysically = 9, 
		Moving = 10, 
		Copying = 11
	}
}