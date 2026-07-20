using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBiodiversityUtn.Migrations
{
    /// <inheritdoc />
    public partial class ActuliazcionToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_CodigoTok",
                table: "Tokens");

            migrationBuilder.AlterColumn<string>(
                name: "IdUsuarioTok",
                table: "Tokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CodigoTok",
                table: "Tokens",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "CorreoTok",
                table: "Tokens",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CodigoTok",
                table: "Tokens",
                column: "CodigoTok");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CorreoTok",
                table: "Tokens",
                column: "CorreoTok");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_CodigoTok",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_CorreoTok",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "CorreoTok",
                table: "Tokens");

            migrationBuilder.AlterColumn<string>(
                name: "IdUsuarioTok",
                table: "Tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoTok",
                table: "Tokens",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CodigoTok",
                table: "Tokens",
                column: "CodigoTok",
                unique: true);
        }
    }
}
