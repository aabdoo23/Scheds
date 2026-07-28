using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheds.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropSeatModerationCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The cart and the subscription used to be written non-transactionally, so a cart row
            // could exist with no matching subscription (and vice versa). Reconcile in that
            // direction before dropping the table, otherwise those watches are silently lost.
            // Ids are built the same way CourseSection.ToId() builds them: UPPER(trimmed code)
            // + '_' + trimmed section.
            migrationBuilder.Sql(@"
                INSERT INTO SeatModerations (Id, LastUpdated)
                SELECT ids.Id, DATEADD(minute, -20, SYSUTCDATETIME())
                FROM (
                    SELECT DISTINCT
                        UPPER(LTRIM(RTRIM(c.CourseCode))) + '_' + LTRIM(RTRIM(c.Section)) AS Id
                    FROM CartSeatModerations c
                    WHERE LTRIM(RTRIM(ISNULL(c.CourseCode, ''))) <> ''
                      AND LTRIM(RTRIM(ISNULL(c.Section, ''))) <> ''
                ) ids
                WHERE NOT EXISTS (
                    SELECT 1 FROM SeatModerations sm WHERE sm.Id = ids.Id
                );");

            migrationBuilder.Sql(@"
                INSERT INTO UserSeatModerations (SeatModerationId, UserId)
                SELECT DISTINCT
                    UPPER(LTRIM(RTRIM(c.CourseCode))) + '_' + LTRIM(RTRIM(c.Section)),
                    c.UserId
                FROM CartSeatModerations c
                WHERE LTRIM(RTRIM(ISNULL(c.CourseCode, ''))) <> ''
                  AND LTRIM(RTRIM(ISNULL(c.Section, ''))) <> ''
                  AND EXISTS (SELECT 1 FROM Users u WHERE u.Id = c.UserId)
                  AND NOT EXISTS (
                    SELECT 1 FROM UserSeatModerations us
                    WHERE us.SeatModerationId =
                            UPPER(LTRIM(RTRIM(c.CourseCode))) + '_' + LTRIM(RTRIM(c.Section))
                      AND us.UserId = c.UserId
                );");

            migrationBuilder.DropTable(
                name: "CartSeatModerations");
        }

        /// <inheritdoc />
        /// <remarks>
        /// Recreates the table empty. The rows are not restorable, but the watches they
        /// represented survive as SeatModerations/UserSeatModerations rows.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartSeatModerations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Section = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartSeatModerations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartSeatModerations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartSeatModerations_UserId",
                table: "CartSeatModerations",
                column: "UserId");
        }
    }
}
