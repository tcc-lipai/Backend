using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ContatoDeProfissional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MensagensContato_Usuarios_UsuarioId",
                table: "MensagensContato");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "MensagensContato",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ProfissionalId",
                table: "MensagensContato",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MensagensContato_ProfissionalId",
                table: "MensagensContato",
                column: "ProfissionalId");

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensContato_Profissionais_ProfissionalId",
                table: "MensagensContato",
                column: "ProfissionalId",
                principalTable: "Profissionais",
                principalColumn: "IdProfissional");

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensContato_Usuarios_UsuarioId",
                table: "MensagensContato",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MensagensContato_Profissionais_ProfissionalId",
                table: "MensagensContato");

            migrationBuilder.DropForeignKey(
                name: "FK_MensagensContato_Usuarios_UsuarioId",
                table: "MensagensContato");

            migrationBuilder.DropIndex(
                name: "IX_MensagensContato_ProfissionalId",
                table: "MensagensContato");

            migrationBuilder.DropColumn(
                name: "ProfissionalId",
                table: "MensagensContato");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "MensagensContato",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MensagensContato_Usuarios_UsuarioId",
                table: "MensagensContato",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
