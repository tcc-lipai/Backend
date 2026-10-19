using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaNivelVideoAlternativa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NivelDificuldade",
                table: "LicoesVideo",
                type: "int",
                nullable: false,
                defaultValue: 2); // Basico — conteúdo pré-existente, sem nível definido antes

            migrationBuilder.AddColumn<int>(
                name: "NivelDificuldade",
                table: "LicoesAlternativa",
                type: "int",
                nullable: false,
                defaultValue: 2); // Basico — conteúdo pré-existente, sem nível definido antes
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NivelDificuldade",
                table: "LicoesVideo");

            migrationBuilder.DropColumn(
                name: "NivelDificuldade",
                table: "LicoesAlternativa");
        }
    }
}
