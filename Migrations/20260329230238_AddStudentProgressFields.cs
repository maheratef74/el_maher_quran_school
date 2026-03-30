using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElMaherQuranSchool.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProgressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PagesProgress",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointProgress",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PagesProgress",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PointProgress",
                table: "Students");
        }
    }
}
