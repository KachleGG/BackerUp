using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BackerUp.Core
{
    public class Config
    {
        public static string AppDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BackerUp");
        public static string ConfigFilePath = Path.Combine(AppDataFolderPath, "BackerUp.conf");
        public static List<BackupJob> GetJobs()
        {
            EnsureAppData();
            try
            {
                string configJson = File.ReadAllText(ConfigFilePath);
                return JsonSerializer.Deserialize<List<BackupJob>>(configJson, new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } }) ?? new List<BackupJob>();
            }
            catch (Exception ex)
            {
                LoggerService.Log(ex.Message);
                return new List<BackupJob>();
            }
        }

        public static void EnsureAppData()
        {
            try
            {
                if (!Directory.Exists(AppDataFolderPath))
                {
                    Directory.CreateDirectory(AppDataFolderPath);
                }

                if (!File.Exists(ConfigFilePath))
                {
                    File.Create(ConfigFilePath);
                }
            }
            catch (Exception ex) {
                LoggerService.Log($"There was an error with the local appdata folder: {ex.Message}");
            }
        }

        public static void AddJob(BackupJob job)
        {
            EnsureAppData();

            try
            {
                List<BackupJob> jobs = GetJobs();
                jobs.Add(job);
                string json = JsonSerializer.Serialize(jobs);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex) { 
                LoggerService.Log($"There was a problem adding a job(Id: {job.Id}): {ex.Message}");
            }
        }

        public static void RemoveJob(BackupJob job)
        {
            EnsureAppData();

            try
            {
                List<BackupJob> jobs = GetJobs();
                jobs.Remove(job);
                string json = JsonSerializer.Serialize(jobs);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex) {
                LoggerService.Log($"There was a problem removing a job(Id: {job.Id}): {ex.Message}");
            }
        }

        public static void EditJob(BackupJob job, BackupJob newJob)
        {
            EnsureAppData();

            try
            {
                
            }
            catch (Exception ex)
            {
                LoggerService.Log($"There was a problem editing a job(Id: {job.Id}): {ex.Message}");
            }
        }
    }
}
