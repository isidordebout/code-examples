using IronPython.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace 
{
    
    class Program
    {
        private static readonly string script = 
            "import random; " + "\n" +
            "import sys; " + "\n" + 
            "print sys.path" + "\n" +
            "def getRndInt() : " + "\n" + 
            "    random.randint(1,10)";

        private static void BugOnImportWithSharedEnginedBetweenThreads()
        {
            var engine = Python.CreateEngine();
            engine.SetSearchPaths(SearchPaths());
            
            // crash aussi
            //var sysModule = Python.GetSysModule(engine);
            //sysModule.GetVariable("path").append("C:\\Program Files (x86)\\IronPython 2.7\\Lib");
            try
            {
                var tasks = Enumerable.Range(0, 50).Select(_ => Task.Factory.StartNew(() =>
                {
                    var scope = engine.CreateScope();
                    var source = engine.CreateScriptSourceFromString(script);
                    source.Execute(scope);

                    //crash aussi :
                    //var scope = engine.ImportModule("XAlgo");
                    //var m = scope.GetVariable("getrandint");
                    //var r = engine.Operations.Invoke(m, 1, 10);
                    //Console.WriteLine(r);
                })).ToArray();
                Task.WaitAll(tasks);
            }
            catch (AggregateException e)
            {
                Console.WriteLine(e.InnerExceptions.First().Message);
            }
        }

        private static void BugMemoryLeakOneEnginePerCall()
        {
            while(true)
            {
                var engine = Python.CreateEngine();
                // Leak aussi:
                //var sysModule = Python.GetSysModule(engine);
                //sysModule.GetVariable("path").append("C:\\Program Files (x86)\\IronPython 2.7\\Lib");

                engine.SetSearchPaths(SearchPaths());
                var runtime = engine.Runtime;
                var scope = engine.CreateScope();

                var source = engine.CreateScriptSourceFromString(script);
                source.Execute(scope);
                runtime.Shutdown();
            }
        }

        private static void SandBoxedEngine()
        {
            var sandbox = AppDomain.CreateDomain("sandbox");
            var engine = Python.CreateEngine(sandbox);

            engine.SetSearchPaths(SearchPaths());
            var runtime = engine.Runtime;
            var scope = engine.CreateScope();

            var source = engine.CreateScriptSourceFromString(script);
            source.Execute(scope);
            runtime.Shutdown();
            AppDomain.Unload(sandbox);
        }

        //http://stackoverflow.com/questions/1664567/embedded-ironpython-memory-leak
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            for(int i=0; i<10; i++)
                SandBoxedEngine();
            sw.Stop();
            Console.WriteLine("SandBoxed engine " + sw.ElapsedMilliseconds/10);
            
            BugOnImportWithSharedEnginedBetweenThreads();

            BugMemoryLeakOneEnginePerCall();
        }

        private static string[] SearchPaths()
        {
            string path = Assembly.GetExecutingAssembly().Location;
            return new [] {
                Directory.GetParent(path).FullName, 
                "C:\\Program Files (x86)\\IronPython 2.7\\Lib", 
                "C:\\Program Files (x86)\\IronPython 2.7\\DLLs", 
                "C:\\Program Files (x86)\\IronPython 2.7", 
                "C:\\Program Files (x86)\\IronPython 2.7\\lib\\site-packages",
                "C:\\Python27\\Lib",
                "C:\\Python27\\Lib\\site-packages"
            };
        }
    }
}
