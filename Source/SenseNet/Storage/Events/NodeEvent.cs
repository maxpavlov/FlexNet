using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Storage.Events
{
	public enum NodeEvent
	{
		Created = 0, 
		Modified = 1, 
		Deleted = 2, 
		DeletedPhysically = 3, 
		Moved = 4, 
		Copied = 5,
        Loaded = 6
	}
}