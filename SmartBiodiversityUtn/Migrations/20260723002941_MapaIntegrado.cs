using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBiodiversityUtn.Migrations
{
    /// <inheritdoc />
    public partial class MapaIntegrado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Facultades",
                columns: table => new
                {
                    IdFacultad = table.Column<string>(type: "text", nullable: false),
                    NombreFac = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Latitud = table.Column<double>(type: "double precision", nullable: false),
                    Longitud = table.Column<double>(type: "double precision", nullable: false),
                    DescripcionFac = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facultades", x => x.IdFacultad);
                });

            migrationBuilder.CreateTable(
                name: "EspecieFacultad",
                columns: table => new
                {
                    IdEspecieFacultad = table.Column<string>(type: "text", nullable: false),
                    IdEspecies = table.Column<string>(type: "text", nullable: false),
                    IdFacultad = table.Column<string>(type: "text", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EspecieFacultad", x => x.IdEspecieFacultad);
                    table.ForeignKey(
                        name: "FK_EspecieFacultad_Especies_IdEspecies",
                        column: x => x.IdEspecies,
                        principalTable: "Especies",
                        principalColumn: "IdEspecies",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EspecieFacultad_Facultades_IdFacultad",
                        column: x => x.IdFacultad,
                        principalTable: "Facultades",
                        principalColumn: "IdFacultad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EspecieFacultad_IdEspecies_IdFacultad",
                table: "EspecieFacultad",
                columns: new[] { "IdEspecies", "IdFacultad" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EspecieFacultad_IdFacultad",
                table: "EspecieFacultad",
                column: "IdFacultad");

            migrationBuilder.CreateIndex(
                name: "IX_Facultades_NombreFac",
                table: "Facultades",
                column: "NombreFac",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EspecieFacultad");

            migrationBuilder.DropTable(
                name: "Facultades");
        }
    }
}
