using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheds.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IndexCardItemCourseCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CourseCode",
                table: "CardItems",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CardItems_CourseCode",
                table: "CardItems",
                column: "CourseCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CardItems_CourseCode",
                table: "CardItems");

            migrationBuilder.AlterColumn<string>(
                name: "CourseCode",
                table: "CardItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
