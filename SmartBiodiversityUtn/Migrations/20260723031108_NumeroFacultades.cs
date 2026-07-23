using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBiodiversityUtn.Migrations
{
    /// <inheritdoc />
    public partial class NumeroFacultades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroFac",
                table: "Facultades",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroFac",
                table: "Facultades");
        }
    }
}
