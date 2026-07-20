using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RefatoracaoAuthProfissionalAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Senha",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "TipoUsuario",
                table: "Usuarios",
                newName: "SenhaHash");

            migrationBuilder.AddColumn<int>(
                name: "NivelDificuldade",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfissionalId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Profissionais",
                columns: table => new
                {
                    IdProfissional = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Registro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoAcesso = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profissionais", x => x.IdProfissional);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_ProfissionalId",
                table: "Usuarios",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Profissionais_CodigoAcesso",
                table: "Profissionais",
                column: "CodigoAcesso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profissionais_Email",
                table: "Profissionais",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Profissionais_ProfissionalId",
                table: "Usuarios",
                column: "ProfissionalId",
                principalTable: "Profissionais",
                principalColumn: "IdProfissional",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Profissionais_ProfissionalId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Profissionais");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_ProfissionalId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "NivelDificuldade",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ProfissionalId",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "SenhaHash",
                table: "Usuarios",
                newName: "TipoUsuario");

            migrationBuilder.AddColumn<string>(
                name: "Senha",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
