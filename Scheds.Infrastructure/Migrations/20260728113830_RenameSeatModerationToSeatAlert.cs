using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheds.Infrastructure.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Hand-written. EF scaffolded this rename as drop-and-recreate, which would have deleted every
    /// existing subscription. sp_rename keeps the rows, indexes and constraints in place — only the
    /// names change.
    /// </remarks>
    public partial class RenameSeatModerationToSeatAlert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "SeatModerations", newName: "SeatAlerts");
            migrationBuilder.RenameTable(name: "UserSeatModerations", newName: "UserSeatAlerts");
            migrationBuilder.RenameColumn(
                name: "SeatModerationId", table: "UserSeatAlerts", newName: "SeatAlertId");

            // Constraint and index names do not follow a table rename, and later migrations refer to
            // them by the names recorded in the model snapshot.
            migrationBuilder.Sql("EXEC sp_rename N'PK_SeatModerations', N'PK_SeatAlerts', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'PK_UserSeatModerations', N'PK_UserSeatAlerts', N'OBJECT';");
            migrationBuilder.Sql(
                "EXEC sp_rename N'FK_UserSeatModerations_SeatModerations_SeatModerationId', "
                + "N'FK_UserSeatAlerts_SeatAlerts_SeatAlertId', N'OBJECT';");
            migrationBuilder.Sql(
                "EXEC sp_rename N'FK_UserSeatModerations_Users_UserId', "
                + "N'FK_UserSeatAlerts_Users_UserId', N'OBJECT';");
            migrationBuilder.Sql(
                "EXEC sp_rename N'UserSeatAlerts.IX_UserSeatModerations_UserId', "
                + "N'IX_UserSeatAlerts_UserId', N'INDEX';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "EXEC sp_rename N'UserSeatAlerts.IX_UserSeatAlerts_UserId', "
                + "N'IX_UserSeatModerations_UserId', N'INDEX';");
            migrationBuilder.Sql(
                "EXEC sp_rename N'FK_UserSeatAlerts_Users_UserId', "
                + "N'FK_UserSeatModerations_Users_UserId', N'OBJECT';");
            migrationBuilder.Sql(
                "EXEC sp_rename N'FK_UserSeatAlerts_SeatAlerts_SeatAlertId', "
                + "N'FK_UserSeatModerations_SeatModerations_SeatModerationId', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'PK_UserSeatAlerts', N'PK_UserSeatModerations', N'OBJECT';");
            migrationBuilder.Sql("EXEC sp_rename N'PK_SeatAlerts', N'PK_SeatModerations', N'OBJECT';");

            migrationBuilder.RenameColumn(
                name: "SeatAlertId", table: "UserSeatAlerts", newName: "SeatModerationId");
            migrationBuilder.RenameTable(name: "UserSeatAlerts", newName: "UserSeatModerations");
            migrationBuilder.RenameTable(name: "SeatAlerts", newName: "SeatModerations");
        }
    }
}
