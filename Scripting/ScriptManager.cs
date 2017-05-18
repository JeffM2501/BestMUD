using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using IronPython.Compiler;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Hosting.Providers;


namespace Scripting
{
    public class ScriptManager
    {
        internal List<ScriptContents> Scripts = new List<ScriptContents>();

        internal static ScriptEngine GetPythonEngine()
        {
            ScriptEngine engine = Python.CreateEngine();

            // Make sure they have access to this API as an assembly
            engine.Runtime.LoadAssembly(Assembly.GetExecutingAssembly());

            // make sure they have access to CLR
            engine.Runtime.ImportModule("clr");

            return engine;
        }

        private ScriptContents LoadScript(string scriptFile, ScriptEngine engine)
        {
            Register.Clear();

            ScriptSource Source = engine.CreateScriptSourceFromString(GetScriptCode(scriptFile), SourceCodeKind.File);
            var code = Source.Compile();
            var scope = engine.CreateScope();

            code.Execute(scope);

            if (Register.RegisteredFunctionNames.Count > 0)
            {
                foreach (var regFunc in Register.RegisteredFunctionNames)
                {
                    if (scope.ContainsVariable(regFunc.Value))
                    {
                        scope.GetVariable(regFunc.Value);
                    }
                }
               
            }

            return null;
        }

        internal static string GetScriptCode(string scriptFile)
        {
            FileInfo file = new FileInfo(scriptFile);

            if (!file.Exists)
                return null;

            FileStream fs = file.OpenRead();
            StreamReader sr = new StreamReader(fs);
            StringBuilder builder = new StringBuilder();

            // add standard imports
            builder.AppendLine("from Scripting import API");
            builder.AppendLine("from Scripting import Register");
            builder.AppendLine("import clr");

            builder.Append(sr.ReadToEnd());
            fs.Close();

            return builder.ToString();
        }
    }
}
