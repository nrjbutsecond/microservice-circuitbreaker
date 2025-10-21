using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Reading.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comic_stats",
                columns: table => new
                {
                    comic_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    total_reads = table.Column<long>(type: "bigint", nullable: false),
                    unique_readers = table.Column<int>(type: "integer", nullable: false),
                    active_readers_24h = table.Column<int>(type: "integer", nullable: false),
                    avg_reading_time_seconds = table.Column<int>(type: "integer", nullable: false),
                    last_updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comic_stats", x => x.comic_id);
                });

            migrationBuilder.CreateTable(
                name: "reading_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    comic_id = table.Column<int>(type: "integer", nullable: false),
                    chapter_id = table.Column<int>(type: "integer", nullable: false),
                    chapter_number = table.Column<int>(type: "integer", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reading_duration_seconds = table.Column<int>(type: "integer", nullable: false),
                    completed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_history", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "comic_stats",
                columns: new[] { "comic_id", "active_readers_24h", "avg_reading_time_seconds", "last_updated", "total_reads", "unique_readers" },
                values: new object[,]
                {
                    { 1, 45, 780, new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1250L, 320 },
                    { 2, 28, 650, new DateTime(2024, 10, 2, 0, 0, 0, 0, DateTimeKind.Utc), 890L, 210 }
                });

            migrationBuilder.InsertData(
                table: "reading_history",
                columns: new[] { "id", "chapter_id", "chapter_number", "comic_id", "completed", "read_at", "reading_duration_seconds", "user_id" },
                values: new object[,]
                {
                    { 1, 1, 1, 1, true, new DateTime(2024, 10, 3, 12, 0, 0, 0, DateTimeKind.Utc), 600, 1 },
                    { 2, 2, 2, 2, true, new DateTime(2024, 10, 4, 14, 30, 0, 0, DateTimeKind.Utc), 720, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_reading_history_comic_id",
                table: "reading_history",
                column: "comic_id");

            migrationBuilder.CreateIndex(
                name: "IX_reading_history_read_at",
                table: "reading_history",
                column: "read_at");

            migrationBuilder.CreateIndex(
                name: "IX_reading_history_user_id",
                table: "reading_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reading_history_user_id_comic_id",
                table: "reading_history",
                columns: new[] { "user_id", "comic_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comic_stats");

            migrationBuilder.DropTable(
                name: "reading_history");
        }
    }
}
