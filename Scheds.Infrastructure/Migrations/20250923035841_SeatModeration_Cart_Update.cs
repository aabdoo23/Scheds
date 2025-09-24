using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheds.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeatModeration_Cart_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeatModerations",
                columns: table => new
                {
                    CourseCode_Section = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatModerations", x => x.CourseCode_Section);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSeatModerations",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SeatModerationId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSeatModerations", x => new { x.UserId, x.SeatModerationId });
                    table.ForeignKey(
                        name: "FK_UserSeatModerations_SeatModerations_SeatModerationId",
                        column: x => x.SeatModerationId,
                        principalTable: "SeatModerations",
                        principalColumn: "CourseCode_Section",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSeatModerations_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSeatModerations_SeatModerationId",
                table: "UserSeatModerations",
                column: "SeatModerationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSeatModerations");

            migrationBuilder.DropTable(
                name: "SeatModerations");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
