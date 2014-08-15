using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Child
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var srz = new BinaryFormatter();
                var dico = (Dictionary<string, string>)srz.Deserialize(Console.OpenStandardInput());
                srz.Serialize(Console.OpenStandardOutput(), 42.0);
            }
            catch(Exception e)
            {
                string msg = e.Message;
            }
        }
    }
}
