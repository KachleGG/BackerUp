using BackerUp.Core.Models;

namespace BackerUp.Editor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            // Load jobs from BackerUp.conf in appdata folder
            List<BackupJob> jobs = Config.GetJobs();
        }
    }
}
