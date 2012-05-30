using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using System.IO;

namespace SenseNet.Portal.UI
{
    public class PathTools
    {
        public string GetFileName(string path)
        {
            if(path.Contains("\\"))
                return Path.GetFileName(path);
            else
                return RepositoryPath.GetFileName(path);
        }
    }
}
