using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class OfensivaEEfeitosDaLoja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasSeguidos",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "MultiplicadorMoedasAte",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MultiplicadorMoedasValor",
                table: "Usuarios",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OfensivaCongeladaAte",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaAtividadeData",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DuracaoDias",
                table: "Produtos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Produtos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorMultiplicador",
                table: "Produtos",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasSeguidos",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MultiplicadorMoedasAte",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MultiplicadorMoedasValor",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "OfensivaCongeladaAte",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UltimaAtividadeData",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DuracaoDias",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "ValorMultiplicador",
                table: "Produtos");
        }
    }
}
