using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TreinamentosManager.Migrations
{
    /// <inheritdoc />
    public partial class AddProficienciaECertificacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataExpiracaoACI",
                table: "Instrutores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InstrutorACPs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstrutorId = table.Column<string>(type: "text", nullable: false),
                    SoftwareId = table.Column<int>(type: "integer", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrutorACPs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrutorACPs_Instrutores_InstrutorId",
                        column: x => x.InstrutorId,
                        principalTable: "Instrutores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstrutorACPs_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstrutorProficiencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstrutorId = table.Column<string>(type: "text", nullable: false),
                    SoftwareId = table.Column<int>(type: "integer", nullable: false),
                    Estrelas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrutorProficiencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrutorProficiencias_Instrutores_InstrutorId",
                        column: x => x.InstrutorId,
                        principalTable: "Instrutores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstrutorProficiencias_Softwares_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Softwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstrutorACPs_InstrutorId",
                table: "InstrutorACPs",
                column: "InstrutorId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrutorACPs_SoftwareId",
                table: "InstrutorACPs",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrutorProficiencias_InstrutorId",
                table: "InstrutorProficiencias",
                column: "InstrutorId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrutorProficiencias_SoftwareId",
                table: "InstrutorProficiencias",
                column: "SoftwareId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstrutorACPs");

            migrationBuilder.DropTable(
                name: "InstrutorProficiencias");

            migrationBuilder.DropColumn(
                name: "DataExpiracaoACI",
                table: "Instrutores");
        }
    }
}
