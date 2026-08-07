using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AtividadesSalvas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AtividadesSalvas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    AtividadeId = table.Column<int>(type: "int", nullable: false),
                    DataSalva = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtividadesSalvas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtividadesSalvas_Atividades_AtividadeId",
                        column: x => x.AtividadeId,
                        principalTable: "Atividades",
                        principalColumn: "IdAtividade",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtividadesSalvas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtividadesSalvas_AtividadeId",
                table: "AtividadesSalvas",
                column: "AtividadeId");

            migrationBuilder.CreateIndex(
                name: "IX_AtividadesSalvas_UsuarioId_AtividadeId",
                table: "AtividadesSalvas",
                columns: new[] { "UsuarioId", "AtividadeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtividadesSalvas");
        }
    }
}
