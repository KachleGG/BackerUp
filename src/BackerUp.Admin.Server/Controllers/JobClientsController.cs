using BackerUp.Admin.Server.Data;
using BackerUp.Admin.Server.Models.DTOs;
using BackerUp.Admin.Server.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BackerUp.Admin.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobClientsController : ControllerBase
    {
        private readonly BackerUpDbContext _db;

        public JobClientsController(BackerUpDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var links = _db.JobsClients.Select(jc => new JobClientResponse
            {
                Id = jc.Id,
                JobId = jc.JobId,
                ClientId = jc.ClientId,
                IsActive = jc.IsActive
            }).ToList();

            return Ok(links);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var jc = _db.JobsClients.Find(id);
            if (jc == null) return NotFound();

            return Ok(new JobClientResponse { Id = jc.Id, JobId = jc.JobId, ClientId = jc.ClientId, IsActive = jc.IsActive });
        }

        [HttpPost]
        public IActionResult Create(CreateJobClientRequest request)
        {
            if (!_db.BackupJobs.Any(j => j.Id == request.JobId))
                return BadRequest("Job not found.");

            if (!_db.Clients.Any(c => c.Id == request.ClientId))
                return BadRequest("Client not found.");

            var jc = new JobClient
            {
                JobId = request.JobId,
                ClientId = request.ClientId,
                IsActive = request.IsActive
            };

            _db.JobsClients.Add(jc);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = jc.Id }, new JobClientResponse { Id = jc.Id, JobId = jc.JobId, ClientId = jc.ClientId, IsActive = jc.IsActive });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateJobClientRequest request)
        {
            var jc = _db.JobsClients.Find(id);
            if (jc == null) return NotFound();

            jc.IsActive = request.IsActive;

            _db.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var jc = _db.JobsClients.Find(id);
            if (jc == null) return NotFound();

            _db.JobsClients.Remove(jc);
            _db.SaveChanges();
            return NoContent();
        }
    }
}
