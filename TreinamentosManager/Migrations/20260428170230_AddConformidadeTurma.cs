using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreinamentosManager.Migrations
{
    /// <inheritdoc />
    public partial class AddConformidadeTurma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Conformidade",
                table: "Turmas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Conformidade",
                table: "Turmas");
        }
    }
}
