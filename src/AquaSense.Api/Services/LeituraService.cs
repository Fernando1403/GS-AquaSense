using AquaSense.Api.Data;
using AquaSense.Api.Dtos;
using AquaSense.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaSense.Api.Services;

public class LeituraService : ILeituraService
{
    private readonly AppDbContext _db;
    private readonly IClassificadorNivel _classificador;

    public LeituraService(AppDbContext db, IClassificadorNivel classificador)
    {
        _db = db;
        _classificador = classificador;
    }

    public async Task<LeituraResponse?> RegistrarAsync(CriarLeituraRequest request, CancellationToken ct = default)
    {
        var localExiste = await _db.Locais.AnyAsync(l => l.Id == request.LocalId, ct);
        if (!localExiste)
            return null;

        var leitura = new Leitura
        {
            LocalId = request.LocalId,
            NivelAgua = request.NivelAgua,
            DataHora = request.DataHora?.ToUniversalTime() ?? DateTime.UtcNow
        };

        _db.Leituras.Add(leitura);
        await _db.SaveChangesAsync(ct);

        return ParaResponse(leitura);
    }

    public async Task<List<LeituraResponse>> ListarPorLocalAsync(int localId, CancellationToken ct = default)
    {
        var leituras = await _db.Leituras
            .AsNoTracking()
            .Where(l => l.LocalId == localId)
            .OrderByDescending(l => l.DataHora)
            .ToListAsync(ct);

        return leituras.Select(ParaResponse).ToList();
    }

    public async Task<List<ResumoHoraResponse>> ResumoPorHoraAsync(int localId, CancellationToken ct = default)
    {
        var leituras = await _db.Leituras
            .AsNoTracking()
            .Where(l => l.LocalId == localId)
            .ToListAsync(ct);

        // Agrupamento por hora feito com LINQ em memória.
        return leituras
            .GroupBy(l => new DateTime(l.DataHora.Year, l.DataHora.Month, l.DataHora.Day, l.DataHora.Hour, 0, 0))
            .OrderBy(g => g.Key)
            .Select(g => new ResumoHoraResponse
            {
                Hora = g.Key,
                NivelMedio = Math.Round(g.Average(l => l.NivelAgua), 2),
                NivelMaximo = g.Max(l => l.NivelAgua),
                Quantidade = g.Count()
            })
            .ToList();
    }

    private LeituraResponse ParaResponse(Leitura l) => new()
    {
        Id = l.Id,
        LocalId = l.LocalId,
        NivelAgua = l.NivelAgua,
        DataHora = l.DataHora,
        Nivel = l.Classificar(_classificador).ToString()
    };
}
