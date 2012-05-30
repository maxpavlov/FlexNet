using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Packaging;

namespace SenseNet.Tools.Installer
{
    internal class InstallLogger : IInstallLogger
    {
        const int LINELENGTH = 80;

        Dictionary<char, string> _lines;

        public InstallLogger()
        {
            _lines = new Dictionary<char, string>();
            _lines['='] = new StringBuilder().Append('=', LINELENGTH - 1).ToString();
            _lines['-'] = new StringBuilder().Append('-', LINELENGTH - 1).ToString();
        }

        public void WriteTitle(string title)
        {
            Console.WriteLine(_lines['=']);
            Console.WriteLine(Center(title));
            Console.WriteLine(_lines['=']);
            Console.WriteLine();
        }
        public void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }
        public void WriteInstallStep(InstallStepCategory category, string stepName, string resourceName, string targetName, bool probe, bool overwrite, bool userModified, object previousState)
        {
            var msg = String.Concat(Logger.GetVerb(category, stepName, probe, overwrite, userModified), ": ", probe ? resourceName : targetName);
            Console.WriteLine(msg);
        }

        private string Center(string text)
        {
            if (text.Length >= LINELENGTH - 1)
                return text;
            var sb = new StringBuilder();
            sb.Append(' ', (LINELENGTH - text.Length) / 2).Append(text);
            return sb.ToString();
        }
    }
}
