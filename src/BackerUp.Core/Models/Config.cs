using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackerUp.Core.Models
{
    public class Config
    {
        public static List<BackupJob> GetJobs()
        {
            EnsureAppData();
            try
            {
                string configJson = File.ReadAllText(AppConstants.ConfigFilePath);
                return JsonSerializer.Deserialize<List<BackupJob>>(configJson, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } }) ?? new List<BackupJob>();
            }
            catch (Exception ex)
            {
                return new List<BackupJob>();
            }
        }

        public static Guid GetClientId()
        {
            EnsureAppData();
            var path = Path.Combine(AppConstants.AppDataFolderPath, "client.id");
            try
            {
                if (File.Exists(path))
                {
                    var txt = File.ReadAllText(path).Trim();
                    if (Guid.TryParse(txt, out var g)) return g;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log($"GetClientId error: {ex.Message}");
            }
            return Guid.Empty;
        }

        public static void SaveClientId(Guid id)
        {
            EnsureAppData();
            var path = Path.Combine(AppConstants.AppDataFolderPath, "client.id");
            try
            {
                File.WriteAllText(path, id.ToString());
            }
            catch (Exception ex)
            {
                LoggerService.Log($"SaveClientId error: {ex.Message}");
            }
        }

        public static void SaveJobs(List<BackupJob> jobs)
        {
            EnsureAppData();
            var options = new JsonSerializerOptions { WriteIndented = true, Converters = { new JsonStringEnumConverter() } };
            File.WriteAllText(AppConstants.ConfigFilePath, JsonSerializer.Serialize(jobs, options));
        }

        public static void EnsureAppData()
        {
            try
            {
                if (!Directory.Exists(AppConstants.AppDataFolderPath))
                {
                    Directory.CreateDirectory(AppConstants.AppDataFolderPath);
                }

                if (!File.Exists(AppConstants.ConfigFilePath))
                {
                    // create an empty JSON array so deserialization won't fail
                    File.WriteAllText(AppConstants.ConfigFilePath, "[]");
                }
            }
            catch (Exception ex)
            {
                LoggerService.Log($"There was an error with the local appdata folder: {ex.Message}");
            }
        }
    }
}
