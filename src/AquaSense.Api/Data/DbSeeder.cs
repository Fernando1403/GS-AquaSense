using AquaSense.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaSense.Api.Data;

/// <summary>Cria o banco (se preciso) e insere dados de demonstração SIMULADOS.</summary>
public static class DbSeeder
{
    public static async Task InicializarAsync(AppDbContext db)
    {
        // Se existirem migrations (dotnet ef migrations add), aplica; senão cria o esquema direto.
        if (db.Database.GetMigrations().Any())
            await db.Database.MigrateAsync();
        else
            await db.Database.EnsureCreatedAsync();

        if (await db.Locais.AnyAsync())
            return;

        var local = new Local
        {
            Nome = "Ponto Piloto (provisório) - São Paulo",
            Latitude = -23.5505,
            Longitude = -46.6333
        };

        // Leituras simuladas das últimas horas, só para a demonstração.
        var agora = DateTime.UtcNow;
        double[] niveis = { 2, 4, 6, 9, 12, 18, 27, 22, 14, 8 };
        for (var i = 0; i < niveis.Length; i++)
        {
            local.Leituras.Add(new Leitura
            {
                NivelAgua = niveis[i],
                DataHora = agora.AddMinutes(-(niveis.Length - i) * 20)
            });
        }

        db.Locais.Add(local);
        await db.SaveChangesAsync();
    }
}
