using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PedidoWeb.Data.Migrations
{
    public partial class AddValidarEstoqueClean : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ValidarEstoque",
                table: "Empresas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IntegracaoId",
                table: "CondicoesPagamento",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "IntegracaoId",
                table: "Clientes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidarEstoque",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "IntegracaoId",
                table: "CondicoesPagamento");

            migrationBuilder.DropColumn(
                name: "IntegracaoId",
                table: "Clientes");
        }
    }
}
