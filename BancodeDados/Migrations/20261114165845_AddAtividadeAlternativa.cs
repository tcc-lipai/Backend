using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAtividadeAlternativa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtividadeAlternativaId",
                table: "LicoesAlternativa",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AtividadesAlternativa",
                columns: table => new
                {
                    IdAtividadeAlternativa = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NivelDificuldade = table.Column<int>(type: "int", nullable: false),
                    UnidadeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtividadesAlternativa", x => x.IdAtividadeAlternativa);
                    table.ForeignKey(
                        name: "FK_AtividadesAlternativa_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "IdUnidade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicoesAlternativa_AtividadeAlternativaId",
                table: "LicoesAlternativa",
                column: "AtividadeAlternativaId");

            migrationBuilder.CreateIndex(
                name: "IX_AtividadesAlternativa_UnidadeId",
                table: "AtividadesAlternativa",
                column: "UnidadeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LicoesAlternativa_AtividadesAlternativa_AtividadeAlternativaId",
                table: "LicoesAlternativa",
                column: "AtividadeAlternativaId",
                principalTable: "AtividadesAlternativa",
                principalColumn: "IdAtividadeAlternativa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LicoesAlternativa_AtividadesAlternativa_AtividadeAlternativaId",
                table: "LicoesAlternativa");

            migrationBuilder.DropTable(
                name: "AtividadesAlternativa");

            migrationBuilder.DropIndex(
                name: "IX_LicoesAlternativa_AtividadeAlternativaId",
                table: "LicoesAlternativa");

            migrationBuilder.DropColumn(
                name: "AtividadeAlternativaId",
                table: "LicoesAlternativa");
        }
    }
}
