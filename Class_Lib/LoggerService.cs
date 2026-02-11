using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Class_Lib
{
    public static class LoggerService
    {
        public static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BackerUp", "logs.log");
        public static void Log(string message)
        {
            string msg = $"[{DateTime.Now.Hour.ToString().PadLeft(2, '0')}:{DateTime.Now.Minute.ToString().PadLeft(2, '0')}:{DateTime.Now.Second.ToString().PadLeft(2, '0')}] {message}";
            Console.WriteLine(msg);
            using (StreamWriter sw = new StreamWriter(logFilePath, true)) {
                sw.WriteLineAsync(msg);
            }
        }
    }
}
