using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBiodiversityUtn.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarEsquemaAportes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RutaImagenApo",
                table: "Aportes");

            migrationBuilder.RenameColumn(
                name: "FechaApo",
                table: "Aportes",
                newName: "FechaCreacionApo");

            migrationBuilder.RenameColumn(
                name: "IdAportes",
                table: "Aportes",
                newName: "IdAporte");

            migrationBuilder.AlterColumn<int>(
                name: "EstadoApo",
                table: "Aportes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "DescripcionApo",
                table: "Aportes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacionApo",
                table: "Aportes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaArchivoApo",
                table: "Aportes",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TituloApo",
                table: "Aportes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaAprobacionApo",
                table: "Aportes");

            migrationBuilder.DropColumn(
                name: "RutaArchivoApo",
                table: "Aportes");

            migrationBuilder.DropColumn(
                name: "TituloApo",
                table: "Aportes");

            migrationBuilder.RenameColumn(
                name: "FechaCreacionApo",
                table: "Aportes",
                newName: "FechaApo");

            migrationBuilder.RenameColumn(
                name: "IdAporte",
                table: "Aportes",
                newName: "IdAportes");

            migrationBuilder.AlterColumn<string>(
                name: "EstadoApo",
                table: "Aportes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DescripcionApo",
                table: "Aportes",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaImagenApo",
                table: "Aportes",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
