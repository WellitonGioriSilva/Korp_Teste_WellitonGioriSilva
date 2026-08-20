using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace faturamento_api.Migrations
{
    /// <inheritdoc />
    public partial class AddObservacaoNotaFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observacao",
                table: "NotasFiscais",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observacao",
                table: "NotasFiscais");
        }
    }
}
