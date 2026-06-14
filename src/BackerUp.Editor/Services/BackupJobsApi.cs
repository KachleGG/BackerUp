using BackerUp.Core;
using BackerUp.Core.Models;
using BackerUp.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BackerUp.Editor.Services
{


    public class BackupJobsApi
    {
        private readonly HttpClient _http;

        private readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };


        public BackupJobsApi()
        {
            var baseAddr = AppConstants.ApiBasePath.TrimEnd('/') + "/";
            _http = new HttpClient { BaseAddress = new Uri(baseAddr) };
        }

        private static BackupJob MapFromDto(BackupJobDto dto)
        {
            return new BackupJob
            {
                Id = dto.Id.ToString(),
                Method = Enum.TryParse<BackupMethod>(dto.Method, true, out var m) ? m : BackupMethod.Full,
                Timing = dto.Timing,
                Sources = new List<string>(dto.Sources),
                Targets = new List<string>(dto.Targets),
                BackupRetention = dto.Retention == null ? new BackupRetention() : new BackupRetention { Count = dto.Retention.Count, Size = dto.Retention.Size }
            };
        }

        private static CreateUpdateBackupJobRequest MapToRequest(BackupJob job)
        {
            return new CreateUpdateBackupJobRequest
            {
                Method = job.Method.ToString(),
                Timing = job.Timing,
                Sources = new List<string>(job.Sources),
                Targets = new List<string>(job.Targets),
                Retention = job.BackupRetention == null ? null : new RetentionDto { Count = job.BackupRetention.Count, Size = job.BackupRetention.Size }
            };
        }

        public async Task<List<BackupJob>> GetAllAsync()
        {
            int attempts = 3;
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    var resp = await _http.GetAsync("BackupJobs");
                    var text = await resp.Content.ReadAsStringAsync();
                    if (!resp.IsSuccessStatusCode)
                    {
                        BackerUp.Core.LoggerService.Log($"API returned non-success status code: {resp.StatusCode}. Body: {text}");
                        return new List<BackupJob>();
                    }

                    try
                    {
                        var dtos = System.Text.Json.JsonSerializer.Deserialize<List<BackupJobDto>>(text, _jsonOptions);
                        if (dtos == null) return new List<BackupJob>();
                        return dtos.Select(MapFromDto).ToList();
                    }
                    catch (System.Text.Json.JsonException jex)
                    {
                        BackerUp.Core.LoggerService.Log($"JSON deserialization failed: {jex.Message}. Raw body: {text}");
                        return new List<BackupJob>();
                    }
                }
                catch (Exception ex)
                {
                    BackerUp.Core.LoggerService.Log($"Failed to get jobs from API (attempt {i + 1}/{attempts}): {ex.Message}");
                    await Task.Delay(500);
                }
            }

            return new List<BackupJob>();
        }

        public async Task<BackupJob?> CreateAsync(BackupJob job)
        {
            var req = MapToRequest(job);
            try
            {
                var resp = await _http.PostAsJsonAsync("BackupJobs", req, _jsonOptions);
                if (!resp.IsSuccessStatusCode)
                {
                    BackerUp.Core.LoggerService.Log($"Failed to create job via API: {resp.StatusCode}");
                    return null;
                }
                var dto = await resp.Content.ReadFromJsonAsync<BackupJobDto>(_jsonOptions);
                if (dto == null) return null;
                return MapFromDto(dto);
            }
            catch (Exception ex)
            {
                BackerUp.Core.LoggerService.Log($"Exception creating job via API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateAsync(string jobId, BackupJob job)
        {
            if (!int.TryParse(jobId, out var id)) return false;
            var req = MapToRequest(job);
            try
            {
                var resp = await _http.PutAsJsonAsync($"BackupJobs/{id}", req, _jsonOptions);
                if (!resp.IsSuccessStatusCode)
                    BackerUp.Core.LoggerService.Log($"Failed to update job via API: {resp.StatusCode}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                BackerUp.Core.LoggerService.Log($"Exception updating job via API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string jobId)
        {
            if (!int.TryParse(jobId, out var id)) return false;
            try
            {
                var resp = await _http.DeleteAsync($"BackupJobs/{id}");
                if (!resp.IsSuccessStatusCode)
                    BackerUp.Core.LoggerService.Log($"Failed to delete job via API: {resp.StatusCode}");
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                BackerUp.Core.LoggerService.Log($"Exception deleting job via API: {ex.Message}");
                return false;
            }
        }
    }
}
