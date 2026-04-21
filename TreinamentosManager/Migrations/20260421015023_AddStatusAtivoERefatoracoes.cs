using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreinamentosManager.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAtivoERefatoracoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Softwares");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "Softwares");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Instrutores",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Clientes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Instrutores");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Softwares",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Versao",
                table: "Softwares",
                type: "text",
                nullable: true);
        }
    }
}
