using BackerUp.Admin.Server.Data;
using BackerUp.Admin.Server.Models.DTOs;
using BackerUp.Admin.Server.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BackerUp.Admin.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly BackerUpDbContext _db;

        public ClientsController(BackerUpDbContext db)
        {
            _db = db;
        }

        [HttpPost("healthcheck")]
        public IActionResult HealthCheck(HealthCheckRequest request)
        {
            var client = _db.Clients.Find(request.Id);
            if (client == null) return NotFound();

            client.LastHealthCheck = DateTime.UtcNow;
            _db.SaveChanges();

            return Ok();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var raw = _db.Clients.Select(c => new
            {
                c.Id,
                c.Name,
                c.IsActive,
                c.IsApproved,
                c.LastHealthCheck,
                c.CreatedAt,
                JobIds = c.JobClients.Select(jc => jc.JobId).ToList()
            }).AsEnumerable();

            var clients = raw.Select(c => new ClientResponse
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
                IsApproved = c.IsApproved,
                LastHealthCheck = c.LastHealthCheck,
                IsOnline = c.LastHealthCheck.HasValue && (DateTime.UtcNow - c.LastHealthCheck.Value).TotalMinutes <= 3,
                CreatedAt = c.CreatedAt,
                JobIds = c.JobIds
            }).ToList();

            return Ok(clients);
        }

        [HttpGet("summary")]
        public IActionResult Summary()
        {
            var now = DateTime.UtcNow;
            var total = _db.Clients.Count();
            var approved = _db.Clients.Count(c => c.IsApproved);
            var pending = total - approved;
            var online = _db.Clients.Count(c => c.LastHealthCheck != null && (now - c.LastHealthCheck.Value).TotalMinutes <= 3);
            var offline = total - online;

            return Ok(new { Total = total, Approved = approved, PendingApproval = pending, Online = online, Offline = offline });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var raw = _db.Clients
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.IsActive,
                    c.IsApproved,
                    c.LastHealthCheck,
                    c.CreatedAt,
                    JobIds = c.JobClients.Select(jc => jc.JobId).ToList()
                })
                .FirstOrDefault();

            if (raw == null) return NotFound();

            var client = new ClientResponse
            {
                Id = raw.Id,
                Name = raw.Name,
                IsActive = raw.IsActive,
                IsApproved = raw.IsApproved,
                LastHealthCheck = raw.LastHealthCheck,
                IsOnline = raw.LastHealthCheck.HasValue && (DateTime.UtcNow - raw.LastHealthCheck.Value).TotalMinutes <= 3,
                CreatedAt = raw.CreatedAt,
                JobIds = raw.JobIds
            };

            return Ok(client);
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register(CreateClientRequest request)
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                IsActive = request.IsActive,
                // clients cannot self-approve; admin must approve
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.Clients.Add(client);
            _db.SaveChanges();

            // handle job links
            if (request.JobIds != null)
            {
                foreach (var jid in request.JobIds)
                {
                    if (_db.BackupJobs.Any(j => j.Id == jid))
                    {
                        _db.JobsClients.Add(new JobClient { JobId = jid, ClientId = client.Id, IsActive = true });
                    }
                }
                _db.SaveChanges();
            }

            return CreatedAtAction(nameof(GetById), new { id = client.Id }, new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                IsActive = client.IsActive,
                IsApproved = client.IsApproved,
                LastHealthCheck = client.LastHealthCheck,
                IsOnline = false,
                CreatedAt = client.CreatedAt,
                JobIds = request.JobIds ?? new List<int>()
            });
        }

        [HttpPost("{id}/approve")]
        public IActionResult Approve(Guid id)
        {
            var client = _db.Clients.Find(id);
            if (client == null) return NotFound();

            client.IsApproved = true;
            _db.SaveChanges();
            return NoContent();
        }

        [HttpGet("pending")]
        public IActionResult GetPending()
        {
            // Fetch raw values first, then compute IsOnline in-memory to avoid EF translation issues
            var raw = _db.Clients
                .Where(c => !c.IsApproved)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.IsActive,
                    c.IsApproved,
                    c.LastHealthCheck,
                    c.CreatedAt,
                    JobIds = c.JobClients.Select(jc => jc.JobId).ToList()
                })
                .AsEnumerable()
                .Select(c => new ClientResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    IsApproved = c.IsApproved,
                    LastHealthCheck = c.LastHealthCheck,
                    IsOnline = c.LastHealthCheck.HasValue && (DateTime.UtcNow - c.LastHealthCheck.Value).TotalMinutes <= 3,
                    CreatedAt = c.CreatedAt,
                    JobIds = c.JobIds
                })
                .ToList();

            return Ok(raw);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, UpdateClientRequest request)
        {
            var client = _db.Clients.Find(id);
            if (client == null) return NotFound();

            client.Name = request.Name;
            client.IsActive = request.IsActive;

            if (request.JobIds != null)
            {
                var existing = _db.JobsClients.Where(jc => jc.ClientId == id).ToList();
                _db.JobsClients.RemoveRange(existing);
                foreach (var jid in request.JobIds)
                {
                    if (_db.BackupJobs.Any(j => j.Id == jid))
                    {
                        _db.JobsClients.Add(new JobClient { JobId = jid, ClientId = id, IsActive = true });
                    }
                }
            }

            _db.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var client = _db.Clients.Find(id);
            if (client == null) return NotFound();

            _db.Clients.Remove(client);
            _db.SaveChanges();
            return NoContent();
        }
    }
}
