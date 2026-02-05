using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PocketFinance.Core.Migrations
{
    /// <inheritdoc />
    public partial class CorrecaoTabela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EhReceita",
                table: "Transacoes",
                newName: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Transacoes",
                newName: "EhReceita");
        }
    }
}
