
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RentAdvisor.Server.Database;
using RentAdvisor.Server.Models.Entities;
using Swashbuckle.AspNetCore.Filters;

namespace RentAdvisor.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls("http://*:8080");

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<AppDatabaseContext>(options =>
                options.UseLazyLoadingProxies(true)
                    .UseSqlServer(builder.Configuration.GetConnectionString("RentAdvisor"))
            );

            builder.Services.AddIdentityApiEndpoints<User>()
               .AddRoles<IdentityRole>()
               .AddEntityFrameworkStores<AppDatabaseContext>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .WithOrigins("https://localhost:5173") // Update this to your React app's URL
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.UseCors("AllowSpecificOrigin");
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.MapIdentityApi<User>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new[] { "Admin", "Moderator", "PropertyOwner" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                // Create one admin user, one moderator user and one normal user
                var users = new[]
                {
                    new { UserName = "admin@example.com", Email = "admin@example.com", Password = "Admin@123", Role = "Admin" },
                    new { UserName = "moderator@example.com", Email = "moderator@example.com", Password = "Moderator@123", Role = "Moderator" },
                    new { UserName = "propertyowner@example.com", Email = "propertyowner@example.com", Password = "PropertyOwner@123", Role = "PropertyOwner" },
                    new { UserName = "user@example.com", Email = "user@example.com", Password = "User@123", Role = "" }
                };

                foreach (var userInfo in users)
                {
                    var user = await userManager.FindByEmailAsync(userInfo.Email);
                    if (user == null)
                    {
                        user = new User
                        {
                            UserName = userInfo.UserName,
                            Email = userInfo.Email,
                            EmailConfirmed = true
                        };

                        var result = await userManager.CreateAsync(user, userInfo.Password);
                        if (result.Succeeded)
                        {
                            if (userInfo.Role != "")
                                await userManager.AddToRoleAsync(user, userInfo.Role);
                        }
                    }
                }

            }

            app.Run();
        }
    }
}
