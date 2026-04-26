using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingService.Infrastructure.Migrations._20260426143000_RemoveProgressSnapshotsTable
{
    /// <inheritdoc />
    public partial class RemoveProgressSnapshotsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressSnapshots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgressSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalWorkouts = table.Column<int>(type: "int", nullable: false),
                    WeekOfYear = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressSnapshots_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressSnapshots_UserId",
                table: "ProgressSnapshots",
                column: "UserId");
        }
    }
}
