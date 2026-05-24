using BackerUp.Admin.Server.Data;
using BackerUp.Admin.Server.Models.DTOs;
using BackerUp.Admin.Server.Models.Entities;
using BackerUp.Admin.Server.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BackerUp.Admin.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly BackerUpDbContext _db;

        public LogsController(BackerUpDbContext db)
        {
            _db = db;
        }

        // GET api/logs?jobsClientsId=1&level=Error
        [HttpGet]
        public IActionResult GetAll([FromQuery] int? jobsClientsId, [FromQuery] Level? level)
        {
            var query = _db.Logs.AsQueryable();

            if (jobsClientsId.HasValue)
                query = query.Where(l => l.JobsClientsId == jobsClientsId.Value);

            if (level.HasValue)
                query = query.Where(l => l.Level == level.Value);

            var logs = query.OrderByDescending(l => l.CreatedAt).Select(l => new LogResponse
            {
                Id = l.Id,
                JobsClientsId = l.JobsClientsId,
                Level = l.Level,
                Description = l.Description,
                CreatedAt = l.CreatedAt
            }).ToList();

            return Ok(logs);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var log = _db.Logs.Find(id);
            if (log == null) return NotFound();

            return Ok(new LogResponse { Id = log.Id, JobsClientsId = log.JobsClientsId, Level = log.Level, Description = log.Description, CreatedAt = log.CreatedAt });
        }

        [HttpPost]
        public IActionResult Create(CreateLogRequest request)
        {
            if (!_db.JobsClients.Any(jc => jc.Id == request.JobsClientsId))
                return BadRequest("JobClient not found.");

            var log = new Log
            {
                JobsClientsId = request.JobsClientsId,
                Level = request.Level,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _db.Logs.Add(log);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = log.Id }, new LogResponse { Id = log.Id, JobsClientsId = log.JobsClientsId, Level = log.Level, Description = log.Description, CreatedAt = log.CreatedAt });
        }
    }
}
