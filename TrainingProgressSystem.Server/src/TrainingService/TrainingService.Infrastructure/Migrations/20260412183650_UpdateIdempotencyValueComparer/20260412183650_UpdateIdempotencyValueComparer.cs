using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingService.Infrastructure.Migrations._20260412183650_UpdateIdempotencyValueComparer
{
    /// <inheritdoc />
    public partial class UpdateIdempotencyValueComparer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'RefreshToken')
    ALTER TABLE [AspNetUsers] ADD [RefreshToken] nvarchar(max) NULL;");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = 'RefreshTokenExpiryTime')
    ALTER TABLE [AspNetUsers] ADD [RefreshTokenExpiryTime] datetime2 NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUsers");
        }
    }
}
