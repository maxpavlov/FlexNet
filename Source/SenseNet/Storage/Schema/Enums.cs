using System;

namespace SenseNet.ContentRepository.Storage.Schema
{
    public enum DataType
    {
		//-- Each value must be greater than zero
        String = 1,
        Text = 2,
        Int = 3,
        Currency = 4,
        DateTime = 5,
        Binary = 6,
        Reference = 7
    }

}