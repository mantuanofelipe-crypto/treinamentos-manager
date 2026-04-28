using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TreinamentosManager.Migrations
{
    /// <inheritdoc />
    public partial class AddConformidadeDetalhadaEComunicado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ComunicadoEnviadoEm",
                table: "Turmas",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComunicadoEnviadoPara",
                table: "Turmas",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TurmaConformidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TurmaId = table.Column<int>(type: "integer", nullable: false),
                    FichaInscricao = table.Column<string>(type: "text", nullable: false),
                    Pauta = table.Column<string>(type: "text", nullable: false),
                    Convite = table.Column<string>(type: "text", nullable: false),
                    ConfiguracaoHub = table.Column<string>(type: "text", nullable: false),
                    ScriptAula = table.Column<string>(type: "text", nullable: false),
                    MaterialAula = table.Column<string>(type: "text", nullable: false),
                    BancoQuestoes = table.Column<string>(type: "text", nullable: false),
                    Nuvem = table.Column<string>(type: "text", nullable: false),
                    PreenchimentoPauta = table.Column<string>(type: "text", nullable: false),
                    Certificado = table.Column<string>(type: "text", nullable: false),
                    JustificativaInstrutor = table.Column<string>(type: "text", nullable: true),
                    JustificativaCoordenacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurmaConformidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurmaConformidades_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TurmaConformidades_TurmaId",
                table: "TurmaConformidades",
                column: "TurmaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TurmaConformidades");

            migrationBuilder.DropColumn(
                name: "ComunicadoEnviadoEm",
                table: "Turmas");

            migrationBuilder.DropColumn(
                name: "ComunicadoEnviadoPara",
                table: "Turmas");
        }
    }
}
