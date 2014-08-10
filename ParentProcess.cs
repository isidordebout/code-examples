using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Process myProcess = new Process();

            try
            {
                myProcess.StartInfo.UseShellExecute = false;
                // You can start any process, HelloWorld is a do-nothing example.
                myProcess.StartInfo.FileName = "Child.exe";
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.RedirectStandardOutput = true;
                myProcess.Start();
                // This code assumes the process you are starting will terminate itself.  
                // Given that is is started without a window so you cannot terminate it  
                // on the desktop, it must terminate itself or you can do it programmatically 
                // from this application using the Kill method.
                
                Console.WriteLine(myProcess.StandardOutput.ReadToEnd());
                myProcess.WaitForExit();
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
                Console.ReadKey();
                myProcess.Close();
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
        }
    }
}
