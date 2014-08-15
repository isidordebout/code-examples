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
        private static TResult Spawn<TParam, TResult>(string path, TParam param, int timeout)
        {
            var srz = new BinaryFormatter();
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = path;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardInput = true;
            try
            {
                p.Start();

                srz.Serialize(p.StandardInput.BaseStream, param);
                p.StandardInput.Flush();

                if (!p.WaitForExit(timeout))
                    throw new TimeoutException();
                p.StandardOutput.DiscardBufferedData();// Se positionne au début du flux
                var result =  (TResult)srz.Deserialize(p.StandardOutput.BaseStream);
                p.Close();
                return result;
            }
            catch
            {
                if (!p.HasExited)
                {
                    try { p.Kill(); }
                    catch { }
                }
                throw;
            }
        }

        static void Main(string[] args)
        {
            Console.ReadKey();
            var sw = Stopwatch.StartNew();
            const string path = "C:\\Users\\Toto\\Desktop\\Processes\\Child\\Child\\bin\\Release\\Child.exe";
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    var dico = new Dictionary<string, string>()
                    {
                        {"key1", "value1"},
                        {"key2", "value2"},
                        {"key3", "value3"}
                    };

                    var result = Spawn<Dictionary<string, string>, double>(path, dico, 200);
                    //Console.WriteLine(result);
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
