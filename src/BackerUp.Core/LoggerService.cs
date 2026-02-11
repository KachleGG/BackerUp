using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackerUp.Core
{
    public static class LoggerService
    {
        private static readonly object _lock = new object();
        public static string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BackerUp", "logs.log");

        public static void Log(string message)
        {
            string msg = $"[{DateTime.Now.Hour.ToString().PadLeft(2, '0')}:{DateTime.Now.Minute.ToString().PadLeft(2, '0')}:{DateTime.Now.Second.ToString().PadLeft(2, '0')}] {message}";
            Console.WriteLine(msg);
            lock (_lock)
            {
                try
                {
                    string? dir = Path.GetDirectoryName(logFilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.AppendAllText(logFilePath, msg + Environment.NewLine);
                }
                catch
                {
                    // Silently fail if logging fails
                }
            }
        }
    }
}
