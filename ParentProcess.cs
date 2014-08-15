using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Parent
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadKey();
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    Process myProcess = new Process();

                    var dico = new Dictionary<string, string>()
                {
                    {"key1", "value1"},
                    {"key2", "value2"},
                    {"key3", "value3"}
                };

                    var srz = new BinaryFormatter();

                    myProcess.StartInfo.UseShellExecute = false;
                    // You can start any process, HelloWorld is a do-nothing example.
                    myProcess.StartInfo.FileName = "C:\\Users\\Toto\\Desktop\\Processes\\Child\\Child\\bin\\Release\\Child.exe";
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.RedirectStandardOutput = true;
                    myProcess.StartInfo.RedirectStandardInput = true;
                    myProcess.Start();

                    // This code assumes the process you are starting will terminate itself.  
                    // Given that is is started without a window so you cannot terminate it  
                    // on the desktop, it must terminate itself or you can do it programmatically 
                    // from this application using the Kill method.
                    srz.Serialize(myProcess.StandardInput.BaseStream, dico);
                    myProcess.StandardInput.Flush();
                    myProcess.StandardOutput.ReadToEnd();
                    myProcess.WaitForExit();
                    myProcess.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadKey();
                }
            }
            Console.WriteLine(sw.ElapsedMilliseconds/100);
            Console.ReadKey();
        }
    }
}
