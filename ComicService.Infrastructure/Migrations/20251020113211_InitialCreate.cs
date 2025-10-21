using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ComicService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    total_chapters = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chapters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    comic_id = table.Column<int>(type: "integer", nullable: false),
                    chapter_number = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    view_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chapters", x => x.id);
                    table.ForeignKey(
                        name: "FK_chapters_comics_comic_id",
                        column: x => x.comic_id,
                        principalTable: "comics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "comics",
                columns: new[] { "id", "author", "cover_image_url", "created_at", "description", "status", "title", "total_chapters", "updated_at" },
                values: new object[,]
                {
                    { 1, "Eiichiro Oda", "https://example.com/onepiece.jpg", new DateTime(2022, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Monkey D. Luffy's adventure to become Pirate King", "Ongoing", "One Piece", 1095, new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Masashi Kishimoto", "https://example.com/naruto.jpg", new DateTime(2021, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "A young ninja's quest to become Hokage", "Completed", "Naruto", 700, new DateTime(2023, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Hajime Isayama", "https://example.com/aot.jpg", new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Humanity's fight against titans", "Completed", "Attack on Titan", 139, new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "chapters",
                columns: new[] { "id", "chapter_number", "comic_id", "content", "created_at", "title", "view_count" },
                values: new object[,]
                {
                    { 1, 1, 1, "The beginning of Luffy's journey...", new DateTime(2022, 10, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Romance Dawn", 5000 },
                    { 2, 2, 1, "Luffy meets Zoro...", new DateTime(2022, 10, 8, 12, 0, 0, 0, DateTimeKind.Utc), "The Man in the Straw Hat", 4500 },
                    { 3, 1, 2, "A troublesome ninja is born...", new DateTime(2021, 6, 2, 12, 0, 0, 0, DateTimeKind.Utc), "Uzumaki Naruto", 6000 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_chapters_comic_id",
                table: "chapters",
                column: "comic_id");

            migrationBuilder.CreateIndex(
                name: "IX_chapters_comic_id_chapter_number",
                table: "chapters",
                columns: new[] { "comic_id", "chapter_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comics_author",
                table: "comics",
                column: "author");

            migrationBuilder.CreateIndex(
                name: "IX_comics_title",
                table: "comics",
                column: "title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chapters");

            migrationBuilder.DropTable(
                name: "comics");
        }
    }
}
