using ComicService.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace ComicService.Infrastructure.Persistence
{
    public class ComicDbContext : DbContext
    {
        public ComicDbContext(DbContextOptions<ComicDbContext> options) : base(options)
        {
        }

        public DbSet<Comic> Comics { get; set; } = null!;
        public DbSet<Chapter> Chapters { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================
            // Comic
            // =============================
            modelBuilder.Entity<Comic>(entity =>
            {
                entity.ToTable("comics");
                entity.HasKey(e => e.Id);

                // ✅ ID tự tăng
                entity.Property(e => e.Id)
                      .HasColumnName("id")
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                      .HasColumnName("title")
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(e => e.Author)
                      .HasColumnName("author")
                      .HasMaxLength(200);

                entity.Property(e => e.Description)
                      .HasColumnName("description");

                entity.Property(e => e.CoverImageUrl)
                      .HasColumnName("cover_image_url")
                      .HasMaxLength(1000);

                entity.Property(e => e.TotalChapters)
                      .HasColumnName("total_chapters");

                entity.Property(e => e.Status)
                      .HasColumnName("status")
                      .HasConversion<string>();

                // ✅ Dùng timestamp with time zone để tránh lỗi DateTimeKind
                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp with time zone")
                      .IsRequired();

                entity.Property(e => e.UpdatedAt)
                      .HasColumnName("updated_at")
                      .HasColumnType("timestamp with time zone")
                      .IsRequired();

                entity.HasIndex(e => e.Title);
                entity.HasIndex(e => e.Author);
            });

            // =============================
            // Chapter
            // =============================
            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.ToTable("chapters");
                entity.HasKey(e => e.Id);

                // ✅ ID tự tăng
                entity.Property(e => e.Id)
                      .HasColumnName("id")
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.ComicId)
                      .HasColumnName("comic_id")
                      .IsRequired();

                entity.Property(e => e.ChapterNumber)
                      .HasColumnName("chapter_number")
                      .IsRequired();

                entity.Property(e => e.Title)
                      .HasColumnName("title")
                      .HasMaxLength(500);

                entity.Property(e => e.Content)
                      .HasColumnName("content")
                      .IsRequired();

                entity.Property(e => e.ViewCount)
                      .HasColumnName("view_count");

                entity.Property(e => e.CreatedAt)
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp with time zone")
                      .IsRequired();

                entity.HasIndex(e => e.ComicId);
                entity.HasIndex(e => new { e.ComicId, e.ChapterNumber }).IsUnique();

                entity.HasOne(e => e.Comic)
                      .WithMany(c => c.Chapters)
                      .HasForeignKey(e => e.ComicId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // =============================
            // ✅ Seed Data — Dùng giá trị UTC cố định
            // =============================
            modelBuilder.Entity<Comic>().HasData(
                new Comic
                {
                    Id = 1,
                    Title = "One Piece",
                    Author = "Eiichiro Oda",
                    Description = "Monkey D. Luffy's adventure to become Pirate King",
                    CoverImageUrl = "https://example.com/onepiece.jpg",
                    TotalChapters = 1095,
                    Status = ComicStatus.Ongoing,
                    CreatedAt = new DateTime(2022, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Comic
                {
                    Id = 2,
                    Title = "Naruto",
                    Author = "Masashi Kishimoto",
                    Description = "A young ninja's quest to become Hokage",
                    CoverImageUrl = "https://example.com/naruto.jpg",
                    TotalChapters = 700,
                    Status = ComicStatus.Completed,
                    CreatedAt = new DateTime(2021, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Comic
                {
                    Id = 3,
                    Title = "Attack on Titan",
                    Author = "Hajime Isayama",
                    Description = "Humanity's fight against titans",
                    CoverImageUrl = "https://example.com/aot.jpg",
                    TotalChapters = 139,
                    Status = ComicStatus.Completed,
                    CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<Chapter>().HasData(
                new Chapter
                {
                    Id = 1,
                    ComicId = 1,
                    ChapterNumber = 1,
                    Title = "Romance Dawn",
                    Content = "The beginning of Luffy's journey...",
                    ViewCount = 5000,
                    CreatedAt = new DateTime(2022, 10, 1, 12, 0, 0, DateTimeKind.Utc)
                },
                new Chapter
                {
                    Id = 2,
                    ComicId = 1,
                    ChapterNumber = 2,
                    Title = "The Man in the Straw Hat",
                    Content = "Luffy meets Zoro...",
                    ViewCount = 4500,
                    CreatedAt = new DateTime(2022, 10, 8, 12, 0, 0, DateTimeKind.Utc)
                },
                new Chapter
                {
                    Id = 3,
                    ComicId = 2,
                    ChapterNumber = 1,
                    Title = "Uzumaki Naruto",
                    Content = "A troublesome ninja is born...",
                    ViewCount = 6000,
                    CreatedAt = new DateTime(2021, 6, 2, 12, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
