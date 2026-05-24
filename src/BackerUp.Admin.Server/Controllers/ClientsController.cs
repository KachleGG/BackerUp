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

        [HttpGet]
        public IActionResult GetAll()
        {
            var clients = _db.Clients.Select(c => new ClientResponse
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(clients);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var client = _db.Clients.Find(id);
            if (client == null) return NotFound();

            return Ok(new ClientResponse { Id = client.Id, Name = client.Name, IsActive = client.IsActive, CreatedAt = client.CreatedAt });
        }

        [HttpPost]
        public IActionResult Create(CreateClientRequest request)
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _db.Clients.Add(client);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = client.Id }, new ClientResponse { Id = client.Id, Name = client.Name, IsActive = client.IsActive, CreatedAt = client.CreatedAt });
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, UpdateClientRequest request)
        {
            var client = _db.Clients.Find(id);
            if (client == null) return NotFound();

            client.Name = request.Name;
            client.IsActive = request.IsActive;

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
