using BackerUp.Admin.Server.Data;
using BackerUp.Admin.Server.Models.DTOs;
using BackerUp.Admin.Server.Models.Entities;
using BackerUp.Admin.Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace BackerUp.Admin.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly BackerUpDbContext _db;
        private readonly ProblemLogService _problemLogService;

        public UsersController(BackerUpDbContext db, ProblemLogService problemLogService)
        {
            _db = db;
            _problemLogService = problemLogService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _db.Users.Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                CreatedAt = u.CreatedAt
            }).ToList();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var user = _db.Users.Find(id);
            if (user == null)
            {
                _problemLogService.LogWarning($"Users.GetById user {id} was not found.");
                return NotFound();
            }

            return Ok(new UserResponse { Id = user.Id, Username = user.Username, CreatedAt = user.CreatedAt });
        }

        [HttpPost]
        public IActionResult Create(CreateUserRequest request)
        {
            if (_db.Users.Any(u => u.Username == request.Username))
            {
                _problemLogService.LogWarning($"Users.Create username '{request.Username}' already exists.");
                return Conflict("Username already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Password = HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserResponse { Id = user.Id, Username = user.Username, CreatedAt = user.CreatedAt });
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, UpdateUserRequest request)
        {
            var user = _db.Users.Find(id);
            if (user == null)
            {
                _problemLogService.LogWarning($"Users.Update user {id} was not found.");
                return NotFound();
            }

            user.Username = request.Username;

            if (!string.IsNullOrEmpty(request.Password))
                user.Password = HashPassword(request.Password);

            _db.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var user = _db.Users.Find(id);
            if (user == null)
            {
                _problemLogService.LogWarning($"Users.Delete user {id} was not found.");
                return NotFound();
            }

            _db.Users.Remove(user);
            _db.SaveChanges();
            return NoContent();
        }

        private static string HashPassword(string password) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLower();
    }
}
