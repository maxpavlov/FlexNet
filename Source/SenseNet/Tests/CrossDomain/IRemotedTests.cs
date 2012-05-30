using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ContentRepository.Tests.CrossDomain
{
	public interface IRemotedTests
	{
        void Initialize(string startupPath);

		string[] GetContentTypeNames();
		string[] GetCacheKeys();
        int LoadNodeAndGetId(string path);
        string LoadNodeAndGetFileContent(string path);
	}
}
