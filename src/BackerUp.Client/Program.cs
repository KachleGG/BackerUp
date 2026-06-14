using BackerUp.Client.Services;
using BackerUp.Core;
using BackerUp.Core.Models;

namespace BackerUp.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.CursorVisible = false;
            var registerService = new RegisterService();

            LoggerService.Log("Backup Client Started");

            while (true)
            {
                try
                {
                    bool canRunBackups = await registerService.RunOnceAsync();

                    if (canRunBackups)
                    {
                        BackupService backupService = new(Config.GetJobs());
                        LoggerService.Log("Running backup service");
                        await backupService.RunAsync();
                    }
                    else
                    {
                        LoggerService.Log("Client is not approved; skipping backup run");
                    }
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
