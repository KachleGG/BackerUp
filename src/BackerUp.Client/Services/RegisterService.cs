using BackerUp.Client.Models;
using BackerUp.Core;
using BackerUp.Core.Models;
using System.Net.Http.Json;

namespace BackerUp.Client.Services;

public class RegisterService
{
    private readonly HttpClient _http = new();

    // single public method to run registration/healthcheck and fetch jobs if registered
    public async Task<bool> RunOnceAsync()
    {
        var clientId = Config.GetClientId();
        if (clientId == Guid.Empty)
        {
            clientId = await AttemptRegisterAsync();
            if (clientId != Guid.Empty)
            {
                Config.SaveClientId(clientId);
            }
            else
            {
                return false;
            }
        }

        // check approval status before fetching jobs
        try
        {
            var detailsUrl = AppConstants.ApiBasePath.TrimEnd('/') + $"/Clients/{clientId}";
            var detailsRes = await _http.GetAsync(detailsUrl);
            if (!detailsRes.IsSuccessStatusCode)
            {
                LoggerService.Log($"Client details request returned {detailsRes.StatusCode}");
                return true;
            }

            var clientDto = await detailsRes.Content.ReadFromJsonAsync<ClientResponseDto>();
            if (clientDto == null)
            {
                LoggerService.Log("Client details deserialize returned null");
                return true;
            }

            if (!clientDto.IsActive)
            {
                LoggerService.Log("Client is not active; skipping job run");
                return false;
            }

            if (!clientDto.IsApproved)
            {
                LoggerService.Log("Client not yet approved; skipping job fetch");
                return false;
            }

            // post healthcheck once the client is approved
            await PostHealthcheckAsync(clientId);

            // fetch jobs assigned to this client
            var jobs = await FetchJobsForClientAsync(clientId);
            if (jobs != null && jobs.Any())
            {
                // convert to core model list and replace local jobs
                var coreJobs = jobs.Select(j => new BackupJob
                {
                    Id = j.Id.ToString(),
                    Sources = j.Sources,
                    Targets = j.Targets,
                    Method = j.Method,
                    Timing = j.Timing,
                    BackupRetention = j.Retention == null ? new BackupRetention() { Count = 3, Size = 1 } : new BackupRetention { Count = j.Retention.Count, Size = j.Retention.Size }
                }).ToList();

                // save to local config
                Config.SaveJobs(coreJobs);
            }

            return true;
        }
        catch (Exception ex)
        {
            LoggerService.Log($"RunOnceAsync error checking approval/fetching jobs: {ex.Message}");
            return true;
        }
    }

    private async Task<Guid> AttemptRegisterAsync()
    {
        var url = AppConstants.ApiBasePath.TrimEnd('/') + "/Clients/register";
        var payload = new { Name = Environment.MachineName, IsActive = true };
        HttpResponseMessage res;
        try
        {
            res = await _http.PostAsJsonAsync(url, payload);
        }
        catch (Exception ex)
        {
            LoggerService.Log($"AttemptRegisterAsync network error: {ex.Message}");
            return Guid.Empty;
        }

        if (!res.IsSuccessStatusCode)
        {
            LoggerService.Log($"AttemptRegisterAsync returned {res.StatusCode}");
            return Guid.Empty;
        }

        ClientRegistrationDto? json;
        try
        {
            json = await res.Content.ReadFromJsonAsync<ClientRegistrationDto>();
        }
        catch (Exception ex)
        {
            LoggerService.Log($"AttemptRegisterAsync deserialize error: {ex.Message}");
            return Guid.Empty;
        }

        return json?.Id ?? Guid.Empty;
    }

    private async Task PostHealthcheckAsync(Guid clientId)
    {
        var url = AppConstants.ApiBasePath.TrimEnd('/') + "/Clients/healthcheck";
        try
        {
            await _http.PostAsJsonAsync(url, new { Id = clientId });
        }
        catch (Exception ex)
        {
            LoggerService.Log($"PostHealthcheckAsync error: {ex.Message}");
        }
    }

    private async Task<List<BackupJobResponse>?> FetchJobsForClientAsync(Guid clientId)
    {
        var url = AppConstants.ApiBasePath.TrimEnd('/') + $"/BackupJobs/forClient/{clientId}";
        HttpResponseMessage res;
        try
        {
            res = await _http.GetAsync(url);
        }
        catch (Exception ex)
        {
            LoggerService.Log($"FetchJobsForClientAsync network error: {ex.Message}");
            return null;
        }

        if (!res.IsSuccessStatusCode)
        {
            LoggerService.Log($"FetchJobsForClientAsync returned {res.StatusCode}");
            return null;
        }

        try
        {
            return await res.Content.ReadFromJsonAsync<List<BackupJobResponse>>();
        }
        catch (Exception ex)
        {
            LoggerService.Log($"FetchJobsForClientAsync deserialize error: {ex.Message}");
            return null;
        }
    }
}
