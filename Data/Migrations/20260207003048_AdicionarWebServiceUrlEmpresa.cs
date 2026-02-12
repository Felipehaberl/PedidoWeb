using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PedidoWeb.Data.Migrations
{
    public partial class AdicionarWebServiceUrlEmpresa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebServiceUrl",
                table: "Empresas",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebServiceUrl",
                table: "Empresas");
        }
    }
}
