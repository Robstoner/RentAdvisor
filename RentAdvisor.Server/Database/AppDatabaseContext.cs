using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentAdvisor.Server.Models.Entities;

namespace RentAdvisor.Server.Database
{
    public class AppDatabaseContext : IdentityDbContext<User>
    {
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Property> Properties { get; set; } = default!;
        public DbSet<Review> Reviews { get; set; } = default!;
        public DbSet<PropertyPhotos> PropertiesPhotos { get; set; } = default!;

        public AppDatabaseContext() : base()
        {
        }
        public AppDatabaseContext(DbContextOptions<AppDatabaseContext> options) : base(options)
        {
        }
    }
}
