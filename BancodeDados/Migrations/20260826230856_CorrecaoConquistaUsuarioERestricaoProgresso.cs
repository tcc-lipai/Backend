using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CorrecaoConquistaUsuarioERestricaoProgresso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conquistas_Usuarios_UsuarioIdUsuario",
                table: "Conquistas");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressosAlternativa_LicoesAlternativa_LicaoAlternativaId",
                table: "ProgressosAlternativa");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressosFala_LicoesFala_LicaoFalaId",
                table: "ProgressosFala");

            migrationBuilder.DropIndex(
                name: "IX_ConquistasUsuario_UsuarioId",
                table: "ConquistasUsuario");

            migrationBuilder.DropIndex(
                name: "IX_Conquistas_UsuarioIdUsuario",
                table: "Conquistas");

            migrationBuilder.DropColumn(
                name: "UsuarioIdUsuario",
                table: "Conquistas");

            migrationBuilder.CreateIndex(
                name: "IX_ConquistasUsuario_UsuarioId_ConquistaId",
                table: "ConquistasUsuario",
                columns: new[] { "UsuarioId", "ConquistaId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressosAlternativa_LicoesAlternativa_LicaoAlternativaId",
                table: "ProgressosAlternativa",
                column: "LicaoAlternativaId",
                principalTable: "LicoesAlternativa",
                principalColumn: "IdLicaoAlternativa",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressosFala_LicoesFala_LicaoFalaId",
                table: "ProgressosFala",
                column: "LicaoFalaId",
                principalTable: "LicoesFala",
                principalColumn: "IdLicaoFala",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressosAlternativa_LicoesAlternativa_LicaoAlternativaId",
                table: "ProgressosAlternativa");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressosFala_LicoesFala_LicaoFalaId",
                table: "ProgressosFala");

            migrationBuilder.DropIndex(
                name: "IX_ConquistasUsuario_UsuarioId_ConquistaId",
                table: "ConquistasUsuario");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioIdUsuario",
                table: "Conquistas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConquistasUsuario_UsuarioId",
                table: "ConquistasUsuario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Conquistas_UsuarioIdUsuario",
                table: "Conquistas",
                column: "UsuarioIdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Conquistas_Usuarios_UsuarioIdUsuario",
                table: "Conquistas",
                column: "UsuarioIdUsuario",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressosAlternativa_LicoesAlternativa_LicaoAlternativaId",
                table: "ProgressosAlternativa",
                column: "LicaoAlternativaId",
                principalTable: "LicoesAlternativa",
                principalColumn: "IdLicaoAlternativa",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressosFala_LicoesFala_LicaoFalaId",
                table: "ProgressosFala",
                column: "LicaoFalaId",
                principalTable: "LicoesFala",
                principalColumn: "IdLicaoFala",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
