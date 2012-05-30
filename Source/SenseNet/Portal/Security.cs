using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Portal
{
    public class Security
    {
        public static string Sanitize(string userInput)
        {
            //return userInput;
            return HtmlSanitizer.sanitize(userInput);
        }
    }
}
