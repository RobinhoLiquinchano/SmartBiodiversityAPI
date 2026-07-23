using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBiodiversityUtn.Migrations
{
    /// <inheritdoc />
    public partial class NUevasEntidades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetallesFauna",
                columns: table => new
                {
                    IdEspecies = table.Column<string>(type: "text", nullable: false),
                    LongitudPromedioCm = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    EnvergaduraCm = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    PesoPromedioGramos = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    TipoPelajePlumaje = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DimorfismoSexual = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Dieta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PatronActividad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesFauna", x => x.IdEspecies);
                    table.ForeignKey(
                        name: "FK_DetallesFauna_Especies_IdEspecies",
                        column: x => x.IdEspecies,
                        principalTable: "Especies",
                        principalColumn: "IdEspecies",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesFlora",
                columns: table => new
                {
                    IdEspecies = table.Column<string>(type: "text", nullable: false),
                    AlturaPromedioM = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    AlturaMaximaM = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    DiametroTroncoCm = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    TipoCortezaTronco = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FormaCopa = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TipoHoja = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ColorFlorFruto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    HabitoCrecimiento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesFlora", x => x.IdEspecies);
                    table.ForeignKey(
                        name: "FK_DetallesFlora_Especies_IdEspecies",
                        column: x => x.IdEspecies,
                        principalTable: "Especies",
                        principalColumn: "IdEspecies",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesFauna");

            migrationBuilder.DropTable(
                name: "DetallesFlora");
        }
    }
}
