using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheds.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class minorFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('SeatModerations') AND name = 'LastUpdated'
                )
                BEGIN
                    ALTER TABLE [SeatModerations] ADD [LastUpdated] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('SeatModerations') AND name = 'LastUpdated'
                )
                BEGIN
                    ALTER TABLE [SeatModerations] DROP COLUMN [LastUpdated];
                END
            ");
        }
    }
}
