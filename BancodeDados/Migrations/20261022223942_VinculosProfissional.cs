using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class VinculosProfissional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VinculosProfissional",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ProfissionalId = table.Column<int>(type: "int", nullable: false),
                    DataVinculo = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VinculosProfissional", x => new { x.UsuarioId, x.ProfissionalId });
                    table.ForeignKey(
                        name: "FK_VinculosProfissional_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "IdProfissional",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VinculosProfissional_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VinculosProfissional_ProfissionalId",
                table: "VinculosProfissional",
                column: "ProfissionalId");

            // preserva os vinculos que ja existiam (1 profissional por aluno) antes de apagar a coluna
            migrationBuilder.Sql(
                "INSERT INTO VinculosProfissional (UsuarioId, ProfissionalId, DataVinculo) " +
                "SELECT IdUsuario, ProfissionalId, SYSUTCDATETIME() FROM Usuarios WHERE ProfissionalId IS NOT NULL");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Profissionais_ProfissionalId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_ProfissionalId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ProfissionalId",
                table: "Usuarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfissionalId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE u SET u.ProfissionalId = v.ProfissionalId FROM Usuarios u " +
                "CROSS APPLY (SELECT TOP 1 ProfissionalId FROM VinculosProfissional WHERE UsuarioId = u.IdUsuario ORDER BY DataVinculo) v");

            migrationBuilder.DropTable(
                name: "VinculosProfissional");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_ProfissionalId",
                table: "Usuarios",
                column: "ProfissionalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Profissionais_ProfissionalId",
                table: "Usuarios",
                column: "ProfissionalId",
                principalTable: "Profissionais",
                principalColumn: "IdProfissional",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
