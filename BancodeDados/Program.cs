using Microsoft.EntityFrameworkCore;
using Backend.Data; // Certifique-se que o namespace da sua pasta Data é este

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração da String de Conexão (Lendo do appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();