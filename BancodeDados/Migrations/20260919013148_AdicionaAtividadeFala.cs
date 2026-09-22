using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAtividadeFala : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtividadeFalaId",
                table: "LicoesFala",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ordem",
                table: "LicoesFala",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AtividadesFala",
                columns: table => new
                {
                    IdAtividadeFala = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NivelDificuldade = table.Column<int>(type: "int", nullable: false),
                    UnidadeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtividadesFala", x => x.IdAtividadeFala);
                    table.ForeignKey(
                        name: "FK_AtividadesFala_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "IdUnidade",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicoesFala_AtividadeFalaId",
                table: "LicoesFala",
                column: "AtividadeFalaId");

            migrationBuilder.CreateIndex(
                name: "IX_AtividadesFala_UnidadeId",
                table: "AtividadesFala",
                column: "UnidadeId");

            migrationBuilder.AddForeignKey(
                name: "FK_LicoesFala_AtividadesFala_AtividadeFalaId",
                table: "LicoesFala",
                column: "AtividadeFalaId",
                principalTable: "AtividadesFala",
                principalColumn: "IdAtividadeFala",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LicoesFala_AtividadesFala_AtividadeFalaId",
                table: "LicoesFala");

            migrationBuilder.DropTable(
                name: "AtividadesFala");

            migrationBuilder.DropIndex(
                name: "IX_LicoesFala_AtividadeFalaId",
                table: "LicoesFala");

            migrationBuilder.DropColumn(
                name: "AtividadeFalaId",
                table: "LicoesFala");

            migrationBuilder.DropColumn(
                name: "Ordem",
                table: "LicoesFala");
        }
    }
}
