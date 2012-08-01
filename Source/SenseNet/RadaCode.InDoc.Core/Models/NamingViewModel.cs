using System.Collections.Generic;

namespace RadaCode.InDoc.Core.Models
{
    public class NamingViewModel
    {
        public NamingViewModel(string typeName, List<string> nameBlocks, List<string> paramBlocks)
        {
            TypeName = typeName;

            CodeBlocks = new List<CodeBlockViewModel>();

            for (var i = 0; i < nameBlocks.Count; i++)
            {
                CodeBlocks.Add(new CodeBlockViewModel()
                                   {
                                       Code = nameBlocks[i], 
                                       Param = paramBlocks[i]
                                   });
            }
        }

        public string TypeName { get; set; }

        public List<CodeBlockViewModel> CodeBlocks { get; set; }
    }

    public class CodeBlockViewModel
    {
        public string Code { get; set; }
        public string Param { get; set; }
    }
}
