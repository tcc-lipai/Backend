using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaProgressoVibracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgressosVibracao",
                columns: table => new
                {
                    IdProgresso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    LicaoVibracaoId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressosVibracao", x => x.IdProgresso);
                    table.ForeignKey(
                        name: "FK_ProgressosVibracao_LicoesVibracao_LicaoVibracaoId",
                        column: x => x.LicaoVibracaoId,
                        principalTable: "LicoesVibracao",
                        principalColumn: "IdLicaoVibracao",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgressosVibracao_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosVibracao_LicaoVibracaoId",
                table: "ProgressosVibracao",
                column: "LicaoVibracaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosVibracao_UsuarioId",
                table: "ProgressosVibracao",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressosVibracao");
        }
    }
}
