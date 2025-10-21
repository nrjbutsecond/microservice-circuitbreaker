using System;
using Microsoft.EntityFrameworkCore;
using UserService.Core.Domain.Entities;

namespace UserService.Infrastructure.Persistence
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.Id);

                // ✅ Id tự tăng
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd(); // <--- CHỈNH DÒNG NÀY

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.FullName)
                    .HasColumnName("full_name")
                    .HasMaxLength(200);

                entity.Property(e => e.AvatarUrl)
                    .HasColumnName("avatar_url")
                    .HasMaxLength(1000);

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.LastLoginAt)
                    .HasColumnName("last_login_at");

                // Indexes
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // ✅ Seed data (chỉ định Id cố định không xung đột với auto-increment)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "jane_smith",
                    Email = "jane@example.com",
                    PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123")),
                    FullName = "Jane Smith",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    Username = "john_doe",
                    Email = "john@example.com",
                    PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("password123")),
                    FullName = "John Doe",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
