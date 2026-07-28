using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoConquistasEContato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conquistas_Usuarios_UsuarioId",
                table: "Conquistas");

            migrationBuilder.DropIndex(
                name: "IX_Conquistas_UsuarioId",
                table: "Conquistas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Conquistas");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Conquistas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconeUrl",
                table: "Conquistas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioIdUsuario",
                table: "Conquistas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConquistasUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ConquistaId = table.Column<int>(type: "int", nullable: false),
                    DataConquista = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConquistasUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConquistasUsuario_Conquistas_ConquistaId",
                        column: x => x.ConquistaId,
                        principalTable: "Conquistas",
                        principalColumn: "IdConquista",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConquistasUsuario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MensagensContato",
                columns: table => new
                {
                    IdMensagem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Respondida = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagensContato", x => x.IdMensagem);
                    table.ForeignKey(
                        name: "FK_MensagensContato_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conquistas_UsuarioIdUsuario",
                table: "Conquistas",
                column: "UsuarioIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_ConquistasUsuario_ConquistaId",
                table: "ConquistasUsuario",
                column: "ConquistaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConquistasUsuario_UsuarioId",
                table: "ConquistasUsuario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensContato_UsuarioId",
                table: "MensagensContato",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conquistas_Usuarios_UsuarioIdUsuario",
                table: "Conquistas",
                column: "UsuarioIdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conquistas_Usuarios_UsuarioIdUsuario",
                table: "Conquistas");

            migrationBuilder.DropTable(
                name: "ConquistasUsuario");

            migrationBuilder.DropTable(
                name: "MensagensContato");

            migrationBuilder.DropIndex(
                name: "IX_Conquistas_UsuarioIdUsuario",
                table: "Conquistas");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Conquistas");

            migrationBuilder.DropColumn(
                name: "IconeUrl",
                table: "Conquistas");

            migrationBuilder.DropColumn(
                name: "UsuarioIdUsuario",
                table: "Conquistas");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Conquistas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Conquistas_UsuarioId",
                table: "Conquistas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conquistas_Usuarios_UsuarioId",
                table: "Conquistas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
