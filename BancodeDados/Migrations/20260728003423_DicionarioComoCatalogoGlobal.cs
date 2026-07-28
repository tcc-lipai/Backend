using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class DicionarioComoCatalogoGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensDicionario");

            migrationBuilder.DropTable(
                name: "Dicionarios");

            migrationBuilder.CreateTable(
                name: "Palavras",
                columns: table => new
                {
                    IdPalavra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Palavras", x => x.IdPalavra);
                    table.ForeignKey(
                        name: "FK_Palavras_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Palavras_CategoriaId",
                table: "Palavras",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Palavras");

            migrationBuilder.CreateTable(
                name: "Dicionarios",
                columns: table => new
                {
                    IdDicionario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dicionarios", x => x.IdDicionario);
                    table.ForeignKey(
                        name: "FK_Dicionarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensDicionario",
                columns: table => new
                {
                    IdItemDicionario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    DicionarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensDicionario", x => x.IdItemDicionario);
                    table.ForeignKey(
                        name: "FK_ItensDicionario_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "IdCategoria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItensDicionario_Dicionarios_DicionarioId",
                        column: x => x.DicionarioId,
                        principalTable: "Dicionarios",
                        principalColumn: "IdDicionario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dicionarios_UsuarioId",
                table: "Dicionarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensDicionario_CategoriaId",
                table: "ItensDicionario",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensDicionario_DicionarioId",
                table: "ItensDicionario",
                column: "DicionarioId");
        }
    }
}
