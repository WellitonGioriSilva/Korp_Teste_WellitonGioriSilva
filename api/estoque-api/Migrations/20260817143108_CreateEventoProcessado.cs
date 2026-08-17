using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace estoque_api.Migrations
{
    /// <inheritdoc />
    public partial class CreateEventoProcessado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventosProcessados",
                columns: table => new
                {
                    EventoId = table.Column<string>(type: "text", nullable: false),
                    EventoType = table.Column<string>(type: "text", nullable: false),
                    DataProcessamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosProcessados", x => new { x.EventoId, x.EventoType });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosProcessados");
        }
    }
}
