﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
        public DbSet<Badge> Badges { get; set; } = default!;
        public DbSet<Title> Titles { get; set; } = default!;

        public AppDatabaseContext() : base()
        {
        }
        public AppDatabaseContext(DbContextOptions<AppDatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.User)
                .WithMany(u => u.Properties)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Property>()
                .HasMany(p => p.Reviews)
                .WithOne(r => r.Property)
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasMany(p => p.Photos)
                .WithOne(pp => pp.Property)
                .HasForeignKey(pp => pp.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PropertyPhotos>()
                .HasOne(pp => pp.User)
                .WithMany(u => u.PropertyPhotos)
                .HasForeignKey(pp => pp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PropertyPhotos>()
                .HasOne(pp => pp.Property)
                .WithMany(p => p.Photos)
                .HasForeignKey(pp => pp.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Badges)
                .WithMany(b => b.Users)
                .UsingEntity(j => j.ToTable("UserBadges"));

            modelBuilder.Entity<User>()
                .HasOne(u => u.Title)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TitleId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
