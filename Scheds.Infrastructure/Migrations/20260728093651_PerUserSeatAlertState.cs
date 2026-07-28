using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheds.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PerUserSeatAlertState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "SeatModerations",
                newName: "LastCheckedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotifiedAt",
                table: "UserSeatModerations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastSeenSeats",
                table: "SeatModerations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenSince",
                table: "SeatModerations",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastNotifiedAt",
                table: "UserSeatModerations");

            migrationBuilder.DropColumn(
                name: "LastSeenSeats",
                table: "SeatModerations");

            migrationBuilder.DropColumn(
                name: "OpenSince",
                table: "SeatModerations");

            migrationBuilder.RenameColumn(
                name: "LastCheckedAt",
                table: "SeatModerations",
                newName: "LastUpdated");
        }
    }
}
