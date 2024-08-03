using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiplomskiFinki.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diplomas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Student = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mentor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Member1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Member2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PresentationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diplomas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diplomas");
        }
    }
}
