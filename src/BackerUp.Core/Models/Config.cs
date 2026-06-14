using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
            catch (Exception ex) {
                LoggerService.Log($"There was an error with the local appdata folder: {ex.Message}");
            }
        }        
    }
}
