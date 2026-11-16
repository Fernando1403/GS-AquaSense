using AquaSense.Api.Data;
using AquaSense.Api.Dtos;
using AquaSense.Api.Models;
using AquaSense.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AquaSense.Tests;

public class LeituraServiceTests
{
    private static AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static LeituraService CriarServico(AppDbContext db)
        => new(db, new ClassificadorPorLimiar());

    [Fact]
    public async Task Registrar_LocalInexistente_RetornaNull()
    {
        using var db = CriarContexto();
        var servico = CriarServico(db);

        var resultado = await servico.RegistrarAsync(new CriarLeituraRequest { LocalId = 99, NivelAgua = 5 });

        Assert.Null(resultado);
    }

    [Fact]
    public async Task Registrar_LocalExistente_SalvaEClassificaLeitura()
    {
        using var db = CriarContexto();
        db.Locais.Add(new Local { Id = 1, Nome = "Teste" });
        await db.SaveChangesAsync();
        var servico = CriarServico(db);

        var resultado = await servico.RegistrarAsync(new CriarLeituraRequest { LocalId = 1, NivelAgua = 30 });

        Assert.NotNull(resultado);
        Assert.Equal("Critico", resultado!.Nivel);
        Assert.Equal(1, await db.Leituras.CountAsync());
    }

    [Fact]
    public async Task ResumoPorHora_AgrupaCalculaMediaEMaximo()
    {
        using var db = CriarContexto();
        db.Locais.Add(new Local { Id = 1, Nome = "Teste" });
        var hora = new DateTime(2026, 11, 10, 14, 0, 0);
        db.Leituras.AddRange(
            new Leitura { LocalId = 1, NivelAgua = 10, DataHora = hora.AddMinutes(5) },
            new Leitura { LocalId = 1, NivelAgua = 20, DataHora = hora.AddMinutes(30) },
            new Leitura { LocalId = 1, NivelAgua = 40, DataHora = hora.AddHours(1) });
        await db.SaveChangesAsync();
        var servico = CriarServico(db);

        var resumo = await servico.ResumoPorHoraAsync(1);

        Assert.Equal(2, resumo.Count);
        Assert.Equal(15, resumo[0].NivelMedio);
        Assert.Equal(20, resumo[0].NivelMaximo);
        Assert.Equal(2, resumo[0].Quantidade);
        Assert.Equal(40, resumo[1].NivelMaximo);
    }
}
