using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOrderAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProntoToPedidoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Pronto",
                table: "PedidoItens",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pronto",
                table: "PedidoItens");
        }
    }
}
