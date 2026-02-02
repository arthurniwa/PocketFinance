using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PocketFinance.Core.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Transacoes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Transacoes");
        }
    }
}
