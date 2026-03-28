using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PocketFinance.Core.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarMetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValorAlvo",
                table: "Metas",
                newName: "ValorMeta");

            migrationBuilder.RenameColumn(
                name: "DataLimite",
                table: "Metas",
                newName: "DataAlvo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValorMeta",
                table: "Metas",
                newName: "ValorAlvo");

            migrationBuilder.RenameColumn(
                name: "DataAlvo",
                table: "Metas",
                newName: "DataLimite");
        }
    }
}
