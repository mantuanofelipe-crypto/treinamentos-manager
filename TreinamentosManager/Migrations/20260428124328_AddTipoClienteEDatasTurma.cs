using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TreinamentosManager.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoClienteEDatasTurma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoCliente",
                table: "Turmas",
                type: "text",
                nullable: false,
                defaultValue: "Turmas Gerenciável");

            migrationBuilder.CreateTable(
                name: "TurmaDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TurmaId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurmaDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurmaDatas_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TurmaDatas_TurmaId",
                table: "TurmaDatas",
                column: "TurmaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TurmaDatas");

            migrationBuilder.DropColumn(
                name: "TipoCliente",
                table: "Turmas");
        }
    }
}
