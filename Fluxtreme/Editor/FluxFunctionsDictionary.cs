using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CodeImp.Fluxtreme.Editor
{
    public class FluxFunctionsDictionary : Dictionary<string, IReadOnlyList<string>>
    {
        private const string RESOURCE_NAME = "CodeImp.Fluxtreme.Editor.FluxFunctions.txt";

        public static FluxFunctionsDictionary FromResource()
        {
            FluxFunctionsDictionary dict = new FluxFunctionsDictionary();

            string[] functions = ReadResourceStrings(RESOURCE_NAME);
            foreach (string f in functions)
            {
                int argspos = f.IndexOf('(');
                string fname = f.Substring(0, argspos);
                string fargs = f.Substring(argspos + 1).TrimEnd(')');
                List<string> args = fargs.Split(',').Select(a => a.Trim()).ToList();
                dict[fname] = args;
            }

            return dict;
        }

        private static string[] ReadResourceStrings(string resourcename)
        {
            List<string> lines = new List<string>();
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream stream = asm.GetManifestResourceStream(resourcename))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        lines.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            return lines.ToArray();
        }
    }
}
