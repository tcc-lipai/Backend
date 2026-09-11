using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class IntegracaoIaFeedbackEEstadoAdaptativo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoAdaptativoIA",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetalhesFonemasJson",
                table: "ProgressosFala",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackIA",
                table: "ProgressosFala",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ScoreAcustico",
                table: "ProgressosFala",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoAdaptativoIA",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DetalhesFonemasJson",
                table: "ProgressosFala");

            migrationBuilder.DropColumn(
                name: "FeedbackIA",
                table: "ProgressosFala");

            migrationBuilder.DropColumn(
                name: "ScoreAcustico",
                table: "ProgressosFala");
        }
    }
}
