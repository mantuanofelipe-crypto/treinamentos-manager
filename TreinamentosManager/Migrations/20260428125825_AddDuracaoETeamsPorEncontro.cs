using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreinamentosManager.Migrations
{
    /// <inheritdoc />
    public partial class AddDuracaoETeamsPorEncontro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DuracaoHoras",
                table: "TurmaDatas",
                type: "numeric",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "TeamsMeetingId",
                table: "TurmaDatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamsMeetingUrl",
                table: "TurmaDatas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuracaoHoras",
                table: "TurmaDatas");

            migrationBuilder.DropColumn(
                name: "TeamsMeetingId",
                table: "TurmaDatas");

            migrationBuilder.DropColumn(
                name: "TeamsMeetingUrl",
                table: "TurmaDatas");
        }
    }
}
