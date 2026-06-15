
using BackerUp.Admin.Server.Data;
using BackerUp.Admin.Server.Filters;
using BackerUp.Admin.Server.Models.Entities;
using BackerUp.Admin.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace BackerUp.Admin.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddDbContext<BackerUpDbContext>();
            builder.Services.AddScoped<ProblemLogService>();
            builder.Services.AddScoped<ProblemLoggingFilter>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var key = builder.Configuration["Jwt:Key"] ?? "BackerUp-Development-Key-Change-Me-Please-1234567890";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BackerUp.Admin.Server",
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BackerUp.Admin.Frontend",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });
            builder.Services.AddControllers()
                .AddMvcOptions(options => options.Filters.Add<ProblemLoggingFilter>())
                .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<BackerUpDbContext>();
                    db.Database.Migrate();

                    if (!db.Users.Any(u => u.Username == "admin"))
                    {
                        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("admin"))).ToLower();
                        db.Users.Add(new User
                        {
                            Id = Guid.NewGuid(),
                            Username = "admin",
                            Password = hash,
                            CreatedAt = DateTime.UtcNow
                        });
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Could not connect to the database. Skipping migration and seeding.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
