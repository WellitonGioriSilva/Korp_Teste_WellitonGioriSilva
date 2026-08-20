using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace faturamento_api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotaFiscalDataEmissaoEItens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ItensNotasFiscais_NotaFiscalId",
                table: "ItensNotasFiscais",
                column: "NotaFiscalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensNotasFiscais_NotasFiscais_NotaFiscalId",
                table: "ItensNotasFiscais",
                column: "NotaFiscalId",
                principalTable: "NotasFiscais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensNotasFiscais_NotasFiscais_NotaFiscalId",
                table: "ItensNotasFiscais");

            migrationBuilder.DropIndex(
                name: "IX_ItensNotasFiscais_NotaFiscalId",
                table: "ItensNotasFiscais");
        }
    }
}
