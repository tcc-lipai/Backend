using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class SubstituiAtividadeSalvaPorItemSalvo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtividadesSalvas");

            migrationBuilder.CreateTable(
                name: "ItensSalvos",
                columns: table => new
                {
                    IdItemSalvo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TipoItem = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    DataSalva = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensSalvos", x => x.IdItemSalvo);
                    table.ForeignKey(
                        name: "FK_ItensSalvos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItensSalvos_UsuarioId_TipoItem_ItemId",
                table: "ItensSalvos",
                columns: new[] { "UsuarioId", "TipoItem", "ItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensSalvos");

            migrationBuilder.CreateTable(
                name: "AtividadesSalvas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtividadeId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
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
    }
}
