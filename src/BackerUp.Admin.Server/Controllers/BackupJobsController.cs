using BackerUp.Admin.Server.Data;
using BackerUp.Admin.Server.Models.DTOs;
using BackerUp.Admin.Server.Models.Entities;
using BackerUp.Admin.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackerUp.Admin.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupJobsController : ControllerBase
    {
        private readonly BackerUpDbContext _db;
        private readonly ProblemLogService _problemLogService;

        public BackupJobsController(BackerUpDbContext db, ProblemLogService problemLogService)
        {
            _db = db;
            _problemLogService = problemLogService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var jobs = _db.BackupJobs
                .Include(j => j.Sources)
                .Include(j => j.Targets)
                .Include(j => j.Retention)
                .ToList()
                .Select(j => new BackupJobResponse
                {
                    Id = j.Id,
                    Method = j.Method,
                    Timing = j.Timing,
                    CreatedAt = j.CreatedAt,
                    Sources = j.Sources.Select(s => s.Path).ToList(),
                    Targets = j.Targets.Select(t => t.Path).ToList(),
                    Retention = j.Retention == null ? null : new RetentionDto { Count = j.Retention.Count, Size = j.Retention.Size }
                })
                .ToList();

            return Ok(jobs);
        }

        [HttpGet("forClient/{clientId}")]
        public IActionResult GetForClient(Guid clientId)
        {
            if (!_db.Clients.Any(c => c.Id == clientId))
            {
                _problemLogService.LogWarning($"BackupJobs.GetForClient client {clientId} was not found.");
                return NotFound();
            }

            var jobs = _db.BackupJobs
                .Include(j => j.Sources)
                .Include(j => j.Targets)
                .Include(j => j.Retention)
                .Where(j => j.JobClients.Any(jc => jc.ClientId == clientId && jc.IsActive))
                .ToList()
                .Select(j => new BackupJobResponse
                {
                    Id = j.Id,
                    Method = j.Method,
                    Timing = j.Timing,
                    CreatedAt = j.CreatedAt,
                    Sources = j.Sources.Select(s => s.Path).ToList(),
                    Targets = j.Targets.Select(t => t.Path).ToList(),
                    Retention = j.Retention == null ? null : new RetentionDto { Count = j.Retention.Count, Size = j.Retention.Size }
                })
                .ToList();

            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var job = _db.BackupJobs
                .Include(j => j.Sources)
                .Include(j => j.Targets)
                .Include(j => j.Retention)
                .FirstOrDefault(j => j.Id == id);

            if (job == null)
            {
                _problemLogService.LogWarning($"BackupJobs.GetById job {id} was not found.");
                return NotFound();
            }

            return Ok(new BackupJobResponse
            {
                Id = job.Id,
                Method = job.Method,
                Timing = job.Timing,
                CreatedAt = job.CreatedAt,
                Sources = job.Sources.Select(s => s.Path).ToList(),
                Targets = job.Targets.Select(t => t.Path).ToList(),
                Retention = job.Retention == null ? null : new RetentionDto { Count = job.Retention.Count, Size = job.Retention.Size }
            });
        }

        [HttpPost]
        public IActionResult Create(CreateBackupJobRequest request)
        {
            var job = new BackupJob
            {
                Method = request.Method,
                Timing = request.Timing,
                CreatedAt = DateTime.UtcNow,
                Sources = request.Sources.Select(p => new Source { Path = p }).ToList(),
                Targets = request.Targets.Select(p => new Target { Path = p }).ToList()
            };

            if (request.Retention != null)
                job.Retention = new Retention { Count = request.Retention.Count, Size = request.Retention.Size };

            _db.BackupJobs.Add(job);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = job.Id }, new BackupJobResponse
            {
                Id = job.Id,
                Method = job.Method,
                Timing = job.Timing,
                CreatedAt = job.CreatedAt,
                Sources = job.Sources.Select(s => s.Path).ToList(),
                Targets = job.Targets.Select(t => t.Path).ToList(),
                Retention = job.Retention == null ? null : new RetentionDto { Count = job.Retention.Count, Size = job.Retention.Size }
            });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateBackupJobRequest request)
        {
            var job = _db.BackupJobs
                .Include(j => j.Sources)
                .Include(j => j.Targets)
                .Include(j => j.Retention)
                .FirstOrDefault(j => j.Id == id);

            if (job == null)
            {
                _problemLogService.LogWarning($"BackupJobs.Update job {id} was not found.");
                return NotFound();
            }

            job.Method = request.Method;
            job.Timing = request.Timing;

            _db.Sources.RemoveRange(job.Sources);
            _db.Targets.RemoveRange(job.Targets);

            job.Sources = request.Sources.Select(p => new Source { Path = p, JobId = id }).ToList();
            job.Targets = request.Targets.Select(p => new Target { Path = p, JobId = id }).ToList();

            if (job.Retention != null) _db.Retentions.Remove(job.Retention);
            if (request.Retention != null)
                job.Retention = new Retention { Count = request.Retention.Count, Size = request.Retention.Size, JobId = id };

            _db.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var job = _db.BackupJobs.Find(id);
            if (job == null)
            {
                _problemLogService.LogWarning($"BackupJobs.Delete job {id} was not found.");
                return NotFound();
            }

            _db.BackupJobs.Remove(job);
            _db.SaveChanges();
            return NoContent();
        }

    }
}
