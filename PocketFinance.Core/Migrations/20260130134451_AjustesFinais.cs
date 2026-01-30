using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PocketFinance.Core.Migrations
{
    /// <inheritdoc />
    public partial class AjustesFinais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Transacao",
                table: "Transacao");

            migrationBuilder.RenameTable(
                name: "Transacao",
                newName: "Transacoes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transacoes",
                table: "Transacoes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Transacoes",
                table: "Transacoes");

            migrationBuilder.RenameTable(
                name: "Transacoes",
                newName: "Transacao");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transacao",
                table: "Transacao",
                column: "Id");
        }
    }
}
