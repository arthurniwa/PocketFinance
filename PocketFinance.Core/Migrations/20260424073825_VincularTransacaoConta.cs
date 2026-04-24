using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PocketFinance.Core.Migrations
{
    /// <inheritdoc />
    public partial class VincularTransacaoConta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContaId",
                table: "Transacoes",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContaId",
                table: "Transacoes");
        }
    }
}
