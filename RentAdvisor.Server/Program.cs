
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
                        .WithOrigins("https://localhost:5173", "http://localhost:3000", "localhost:3000", "165.22.91.149", "http://165.22.91.149") // Update this to your React app's URL
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
                var context = scope.ServiceProvider.GetRequiredService<AppDatabaseContext>();
                context.Database.Migrate();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new[] { "Admin", "Moderator", "PropertyOwner" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                if (!context.Badges.Any())
                {
                    var badges = new[]
                    {
                        new Badge { Name = "First Review", Description = "You submitted your first review." },
                        new Badge { Name = "Fifth Review", Description = "You submitted your fifth review." },
                        new Badge { Name = "Tenth Review", Description = "You uploaded your tenth review." },
                        new Badge { Name = "50th Review", Description = "You uploaded your 50th review." },
                        new Badge { Name = "100th Review", Description = "You uploaded your 50th review." },

                        new Badge { Name = "First Property", Description = "You submitted your first property." },
                        new Badge { Name = "Fifth Property", Description = "You uploaded your fifth property." },
                        new Badge { Name = "Tenth Property", Description = "You uploaded your tenth property." },
                        new Badge { Name = "50th Property", Description = "You uploaded your 50th property." },
                        new Badge { Name = "100th Property", Description = "You uploaded your 100th Property." },
                    };
                    await context.Badges.AddRangeAsync(badges);
                    await context.SaveChangesAsync();
                }

                if (!context.Titles.Any())
                {
                    var titles = new[]
                    {
                        new Title { Id = new Guid("caabb5a5-9ddf-4b67-aa52-dafe7eef748a"), Name = "Newcommer", RequiredPoints = 0 },
                        new Title { Id = new Guid("caabb5a5-9ddf-4b67-aa52-dafe7eef748b"), Name = "Beginner", RequiredPoints = 9 },
                        new Title { Id = new Guid("caabb5a5-9ddf-4b67-aa52-dafe7eef748c"), Name = "Intermediate", RequiredPoints = 35 },
                        new Title { Id = new Guid("caabb5a5-9ddf-4b67-aa52-dafe7eef748d"), Name = "Advanced", RequiredPoints = 100 }
                    };
                    await context.Titles.AddRangeAsync(titles);
                    await context.SaveChangesAsync();
                }

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                // Create one admin user, one moderator user and one normal user
                var users = new[]
                {
                    new { UserName = "admin@example.com", Email = "admin@example.com", Password = "Admin@123", Role = "Admin"},
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
                            EmailConfirmed = true,
                            TitleId = new Guid("caabb5a5-9ddf-4b67-aa52-dafe7eef748a")

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
