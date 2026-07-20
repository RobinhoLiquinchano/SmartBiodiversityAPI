using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBiodiversityUtn.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    IdCategorias = table.Column<string>(type: "text", nullable: false),
                    NombreCat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.IdCategorias);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    IdRoles = table.Column<string>(type: "text", nullable: false),
                    NombreRol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.IdRoles);
                });

            migrationBuilder.CreateTable(
                name: "Especies",
                columns: table => new
                {
                    IdEspecies = table.Column<string>(type: "text", nullable: false),
                    IdCategoriaEsp = table.Column<string>(type: "text", nullable: false),
                    NombreComunEsp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NombreCientificoEsp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DescripcionEsp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HabitatEsp = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EstadoEsp = table.Column<int>(type: "integer", nullable: false),
                    FechaRegistroEsp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especies", x => x.IdEspecies);
                    table.ForeignKey(
                        name: "FK_Especies_Categorias_IdCategoriaEsp",
                        column: x => x.IdCategoriaEsp,
                        principalTable: "Categorias",
                        principalColumn: "IdCategorias",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Avisos",
                columns: table => new
                {
                    IdAvisos = table.Column<string>(type: "text", nullable: false),
                    IdRolesAvi = table.Column<string>(type: "text", nullable: false),
                    TituloAvi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MensajeAvi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoriaAvi = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ActivoAvi = table.Column<bool>(type: "boolean", nullable: false),
                    FechaIniAvi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFinAvi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avisos", x => x.IdAvisos);
                    table.ForeignKey(
                        name: "FK_Avisos_Roles_IdRolesAvi",
                        column: x => x.IdRolesAvi,
                        principalTable: "Roles",
                        principalColumn: "IdRoles",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<string>(type: "text", nullable: false),
                    IdRolesU = table.Column<string>(type: "text", nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombres = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Correo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IntentosFallidos = table.Column<int>(type: "integer", nullable: false),
                    BloqueoHasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_IdRolesU",
                        column: x => x.IdRolesU,
                        principalTable: "Roles",
                        principalColumn: "IdRoles",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Multimedia",
                columns: table => new
                {
                    IdMultimedia = table.Column<string>(type: "text", nullable: false),
                    IdEspeciesMul = table.Column<string>(type: "text", nullable: false),
                    TipoArchivoMul = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RutaArchivoMul = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaMul = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Multimedia", x => x.IdMultimedia);
                    table.ForeignKey(
                        name: "FK_Multimedia_Especies_IdEspeciesMul",
                        column: x => x.IdEspeciesMul,
                        principalTable: "Especies",
                        principalColumn: "IdEspecies",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Aportes",
                columns: table => new
                {
                    IdAporte = table.Column<string>(type: "text", nullable: false),
                    IdUsuarioApo = table.Column<string>(type: "text", nullable: false),
                    TituloApo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DescripcionApo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RutaArchivoApo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    EstadoApo = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacionApo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaAprobacionApo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aportes", x => x.IdAporte);
                    table.ForeignKey(
                        name: "FK_Aportes_Usuarios_IdUsuarioApo",
                        column: x => x.IdUsuarioApo,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bitacora",
                columns: table => new
                {
                    IdLog = table.Column<string>(type: "text", nullable: false),
                    IdUsuarioBit = table.Column<string>(type: "text", nullable: false),
                    IdRolesBit = table.Column<string>(type: "text", nullable: false),
                    AccionBit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DetalleBit = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FechaBit = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bitacora", x => x.IdLog);
                    table.ForeignKey(
                        name: "FK_Bitacora_Roles_IdRolesBit",
                        column: x => x.IdRolesBit,
                        principalTable: "Roles",
                        principalColumn: "IdRoles",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bitacora_Usuarios_IdUsuarioBit",
                        column: x => x.IdUsuarioBit,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialContra",
                columns: table => new
                {
                    IdHistorialHco = table.Column<string>(type: "text", nullable: false),
                    IdUsuarioHco = table.Column<string>(type: "text", nullable: false),
                    PasswordHashHco = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaHco = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialContra", x => x.IdHistorialHco);
                    table.ForeignKey(
                        name: "FK_HistorialContra_Usuarios_IdUsuarioHco",
                        column: x => x.IdUsuarioHco,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    IdTokens = table.Column<string>(type: "text", nullable: false),
                    IdUsuarioTok = table.Column<string>(type: "text", nullable: false),
                    CodigoTok = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TipoTok = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FechaCreacionTok = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaExpiracionTok = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Usado = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.IdTokens);
                    table.ForeignKey(
                        name: "FK_Tokens_Usuarios_IdUsuarioTok",
                        column: x => x.IdUsuarioTok,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aportes_IdUsuarioApo",
                table: "Aportes",
                column: "IdUsuarioApo");

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_IdRolesAvi",
                table: "Avisos",
                column: "IdRolesAvi");

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_IdRolesBit",
                table: "Bitacora",
                column: "IdRolesBit");

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_IdUsuarioBit",
                table: "Bitacora",
                column: "IdUsuarioBit");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_NombreCat",
                table: "Categorias",
                column: "NombreCat",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Especies_IdCategoriaEsp",
                table: "Especies",
                column: "IdCategoriaEsp");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialContra_IdUsuarioHco",
                table: "HistorialContra",
                column: "IdUsuarioHco");

            migrationBuilder.CreateIndex(
                name: "IX_Multimedia_IdEspeciesMul",
                table: "Multimedia",
                column: "IdEspeciesMul");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_NombreRol",
                table: "Roles",
                column: "NombreRol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CodigoTok",
                table: "Tokens",
                column: "CodigoTok",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_IdUsuarioTok",
                table: "Tokens",
                column: "IdUsuarioTok");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdRolesU",
                table: "Usuarios",
                column: "IdRolesU");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aportes");

            migrationBuilder.DropTable(
                name: "Avisos");

            migrationBuilder.DropTable(
                name: "Bitacora");

            migrationBuilder.DropTable(
                name: "HistorialContra");

            migrationBuilder.DropTable(
                name: "Multimedia");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "Especies");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
