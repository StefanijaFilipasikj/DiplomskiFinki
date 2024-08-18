using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DiplomskiFinki.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    SubStep = table.Column<double>(type: "float", nullable: false),
                    SubStepName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.SubStep);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Credits = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiplomaStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepSubStep = table.Column<double>(type: "float", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiplomaStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiplomaStatuses_Steps_StepSubStep",
                        column: x => x.StepSubStep,
                        principalTable: "Steps",
                        principalColumn: "SubStep");
                });

            migrationBuilder.CreateTable(
                name: "Diplomas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MentorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Member1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Member2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiplomaStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PresentationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Classroom = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diplomas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diplomas_DiplomaStatuses_DiplomaStatusId",
                        column: x => x.DiplomaStatusId,
                        principalTable: "DiplomaStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Diplomas_Staff_Member1Id",
                        column: x => x.Member1Id,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Diplomas_Staff_Member2Id",
                        column: x => x.Member2Id,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Diplomas_Staff_MentorId",
                        column: x => x.MentorId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Diplomas_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Steps",
                columns: new[] { "SubStep", "SubStepName" },
                values: new object[,]
                {
                    { 1.0, "Пријава" },
                    { 2.0, "Прифаќање на темата од студентот" },
                    { 3.0, "Валидирање од службата за студентски прашања" },
                    { 3.1, "Одобрение од продекан за настава" },
                    { 4.0, "Одобрение за оценка од ментор" },
                    { 5.0, "Забелешки од комисија" },
                    { 6.0, "Валидирање на услови за одбрана" },
                    { 7.0, "Одбрана" },
                    { 8.0, "Архива" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diplomas_DiplomaStatusId",
                table: "Diplomas",
                column: "DiplomaStatusId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Diplomas_Member1Id",
                table: "Diplomas",
                column: "Member1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Diplomas_Member2Id",
                table: "Diplomas",
                column: "Member2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Diplomas_MentorId",
                table: "Diplomas",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_Diplomas_StudentId",
                table: "Diplomas",
                column: "StudentId",
                unique: true,
                filter: "[StudentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DiplomaStatuses_StepSubStep",
                table: "DiplomaStatuses",
                column: "StepSubStep");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diplomas");

            migrationBuilder.DropTable(
                name: "DiplomaStatuses");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "Steps");
        }
    }
}
