using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheds.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleGenerationAnalytics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instructor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Credits = table.Column<double>(type: "float", nullable: false),
                    Section = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SeatsLeft = table.Column<int>(type: "int", nullable: false),
                    SubType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseBases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseBases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instructors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleGenerations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfSchedulesGenerated = table.Column<int>(type: "int", nullable: false),
                    UsedLiveData = table.Column<bool>(type: "bit", nullable: false),
                    ConsideredZeroSeats = table.Column<bool>(type: "bit", nullable: false),
                    IsEngineering = table.Column<bool>(type: "bit", nullable: false),
                    MinimumNumberOfItemsPerDay = table.Column<int>(type: "int", nullable: false),
                    LargestAllowedGap = table.Column<int>(type: "int", nullable: false),
                    NumberOfDays = table.Column<int>(type: "int", nullable: false),
                    IsNumberOfDaysSelected = table.Column<bool>(type: "bit", nullable: false),
                    DaysStart = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaysEnd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelectedDaysJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleGenerations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseSchedules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CardItemId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseSchedules_CardItems_CardItemId",
                        column: x => x.CardItemId,
                        principalTable: "CardItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelectedCourses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleGenerationId = table.Column<int>(type: "int", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelectedCourses_ScheduleGenerations_ScheduleGenerationId",
                        column: x => x.ScheduleGenerationId,
                        principalTable: "ScheduleGenerations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelectedCustomCourses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleGenerationId = table.Column<int>(type: "int", nullable: false),
                    CourseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomMainSection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomSubSection = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomProfessor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomTA = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedCustomCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelectedCustomCourses_ScheduleGenerations_ScheduleGenerationId",
                        column: x => x.ScheduleGenerationId,
                        principalTable: "ScheduleGenerations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardItems_Id",
                table: "CardItems",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseSchedules_CardItemId",
                table: "CourseSchedules",
                column: "CardItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSchedules_Id",
                table: "CourseSchedules",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SelectedCourses_ScheduleGenerationId",
                table: "SelectedCourses",
                column: "ScheduleGenerationId");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedCustomCourses_ScheduleGenerationId",
                table: "SelectedCustomCourses",
                column: "ScheduleGenerationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseBases");

            migrationBuilder.DropTable(
                name: "CourseSchedules");

            migrationBuilder.DropTable(
                name: "Instructors");

            migrationBuilder.DropTable(
                name: "SelectedCourses");

            migrationBuilder.DropTable(
                name: "SelectedCustomCourses");

            migrationBuilder.DropTable(
                name: "CardItems");

            migrationBuilder.DropTable(
                name: "ScheduleGenerations");
        }
    }
}
