using BackerUp.Admin.Server.Data;
using BackerUp.Admin.Server.Models.Entities;
using BackerUp.Admin.Server.Models.Enums;

namespace BackerUp.Admin.Server.Services;

public class ProblemLogService
{
    private readonly BackerUpDbContext _db;

    public ProblemLogService(BackerUpDbContext db)
    {
        _db = db;
    }

    public void LogWarning(string description, int? jobsClientsId = null)
    {
        WriteLog(Level.Warning, description, jobsClientsId);
    }

    public void LogError(string description, int? jobsClientsId = null)
    {
        WriteLog(Level.Error, description, jobsClientsId);
    }

    public void LogException(string scope, Exception exception, int? jobsClientsId = null)
    {
        WriteLog(Level.Error, $"{scope}: {exception.GetType().Name} - {exception.Message}", jobsClientsId);
    }

    private void WriteLog(Level level, string description, int? jobsClientsId)
    {
        try
        {
            _db.Logs.Add(new Log
            {
                JobsClientsId = jobsClientsId,
                Level = level,
                Description = description,
                CreatedAt = DateTime.UtcNow
            });

            _db.SaveChanges();
        }
        catch
        {
            // Logging must never break the endpoint that triggered it.
        }
    }
}