using IronPython.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PocIronPython
{
    class Program
    {
        //http://stackoverflow.com/questions/1664567/embedded-ironpython-memory-leak
        static void Main(string[] args)
        {
            string path = Assembly.GetExecutingAssembly().Location;
            string rootDir = Directory.GetParent(path).FullName;

            while (true)
            {
                var script = "import random; random.randint(1,10)";
                Dictionary<String, Object> options = new Dictionary<string, object>();
                options["LightweightScopes"] = true;

                var engine = Python.CreateEngine();
                engine.SetSearchPaths(new[] { rootDir, "C:\\Program Files (x86)\\IronPython 2.7\\Lib" });
                var runtime = engine.Runtime;
                var scope = engine.CreateScope();

                var source = engine.CreateScriptSourceFromString(script);
                source.Execute(scope);
                runtime.Shutdown();
            }
        }
    }
}
