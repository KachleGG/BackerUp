using BackerUp.Client.Services;
using BackerUp.Core;

namespace BackerUp.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.CursorVisible = false;
            // Load jobs from BackerUp.conf in appdata folder
            BackupService backupService = new(Config.GetJobs());

            LoggerService.Log("Backup Client Started");

            while (true)
            {
                try
                {
                    LoggerService.Log("Running backup service");
                    await backupService.RunAsync();
#if DEBUG
                    await Task.Delay(TimeSpan.FromSeconds(5));
#else
                await Task.Delay(TimeSpan.FromMinutes(1));
#endif
                }
                catch (Exception ex)
                {
                    LoggerService.Log($"Error: {ex.Message}");
                }
            }
        }
    }
}
