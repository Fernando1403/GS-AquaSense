using System.Text.Json.Serialization;
using AquaSense.Api.Data;
using AquaSense.Api.Models;
using AquaSense.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco SQLite (arquivo aquasense.db criado na pasta de execução).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Injeção de dependência.
builder.Services.AddSingleton<IClassificadorNivel, ClassificadorPorLimiar>();
builder.Services.AddScoped<ILocalService, LocalService>();
builder.Services.AddScoped<ILeituraService, LeituraService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.InicializarAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
