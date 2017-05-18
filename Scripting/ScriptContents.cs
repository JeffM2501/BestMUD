using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Scripting
{
    internal class ScriptContents
    {
        public string ModuleName = string.Empty;
        public string ScriptName = string.Empty;
        public FileInfo ScriptFile = null;

        public List<string> ScriptFunctions = new List<string>();
    }

}
