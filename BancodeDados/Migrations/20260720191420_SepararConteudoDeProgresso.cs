using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class SepararConteudoDeProgresso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LicoesAlternativa_Usuarios_UsuarioId",
                table: "LicoesAlternativa");

            migrationBuilder.DropIndex(
                name: "IX_LicoesAlternativa_UsuarioId",
                table: "LicoesAlternativa");

            migrationBuilder.DropColumn(
                name: "DataConclusao",
                table: "LicoesVideo");

            migrationBuilder.DropColumn(
                name: "DataInicio",
                table: "LicoesVideo");

            migrationBuilder.DropColumn(
                name: "ValorGanho",
                table: "LicoesVideo");

            migrationBuilder.DropColumn(
                name: "DataConclusao",
                table: "LicoesFala");

            migrationBuilder.DropColumn(
                name: "DataInicio",
                table: "LicoesFala");

            migrationBuilder.DropColumn(
                name: "FraseFalada",
                table: "LicoesFala");

            migrationBuilder.DropColumn(
                name: "DataConclusao",
                table: "LicoesAlternativa");

            migrationBuilder.DropColumn(
                name: "DataInicio",
                table: "LicoesAlternativa");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "LicoesAlternativa");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "LicoesVideo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NivelDificuldade",
                table: "LicoesFala",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "LicoesAlternativa",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Atividades",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LicoesVibracao",
                columns: table => new
                {
                    IdLicaoVibracao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Silaba = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instrucao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnidadeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicoesVibracao", x => x.IdLicaoVibracao);
                    table.ForeignKey(
                        name: "FK_LicoesVibracao_Unidades_UnidadeId",
                        column: x => x.UnidadeId,
                        principalTable: "Unidades",
                        principalColumn: "IdUnidade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressosAlternativa",
                columns: table => new
                {
                    IdProgresso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    LicaoAlternativaId = table.Column<int>(type: "int", nullable: false),
                    RespostaDada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Correta = table.Column<bool>(type: "bit", nullable: false),
                    PontuacaoObtida = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressosAlternativa", x => x.IdProgresso);
                    table.ForeignKey(
                        name: "FK_ProgressosAlternativa_LicoesAlternativa_LicaoAlternativaId",
                        column: x => x.LicaoAlternativaId,
                        principalTable: "LicoesAlternativa",
                        principalColumn: "IdLicaoAlternativa",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressosAlternativa_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressosFala",
                columns: table => new
                {
                    IdProgresso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    LicaoFalaId = table.Column<int>(type: "int", nullable: false),
                    TextoReconhecidoPelaIA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Correta = table.Column<bool>(type: "bit", nullable: false),
                    PontuacaoObtida = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressosFala", x => x.IdProgresso);
                    table.ForeignKey(
                        name: "FK_ProgressosFala_LicoesFala_LicaoFalaId",
                        column: x => x.LicaoFalaId,
                        principalTable: "LicoesFala",
                        principalColumn: "IdLicaoFala",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressosFala_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicoesVibracao_UnidadeId",
                table: "LicoesVibracao",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosAlternativa_LicaoAlternativaId",
                table: "ProgressosAlternativa",
                column: "LicaoAlternativaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosAlternativa_UsuarioId",
                table: "ProgressosAlternativa",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosFala_LicaoFalaId",
                table: "ProgressosFala",
                column: "LicaoFalaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosFala_UsuarioId",
                table: "ProgressosFala",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LicoesVibracao");

            migrationBuilder.DropTable(
                name: "ProgressosAlternativa");

            migrationBuilder.DropTable(
                name: "ProgressosFala");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "LicoesVideo");

            migrationBuilder.DropColumn(
                name: "NivelDificuldade",
                table: "LicoesFala");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "LicoesAlternativa");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Atividades");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataConclusao",
                table: "LicoesVideo",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicio",
                table: "LicoesVideo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "ValorGanho",
                table: "LicoesVideo",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataConclusao",
                table: "LicoesFala",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicio",
                table: "LicoesFala",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FraseFalada",
                table: "LicoesFala",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataConclusao",
                table: "LicoesAlternativa",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicio",
                table: "LicoesAlternativa",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "LicoesAlternativa",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LicoesAlternativa_UsuarioId",
                table: "LicoesAlternativa",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_LicoesAlternativa_Usuarios_UsuarioId",
                table: "LicoesAlternativa",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
