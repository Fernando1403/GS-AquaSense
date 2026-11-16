using AquaSense.Api.Data;
using AquaSense.Api.Dtos;
using AquaSense.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AquaSense.Api.Services;

public class LocalService : ILocalService
{
    private readonly AppDbContext _db;

    public LocalService(AppDbContext db) => _db = db;

    public async Task<List<LocalResponse>> ListarAsync(CancellationToken ct = default)
    {
        return await _db.Locais
            .AsNoTracking()
            .OrderBy(l => l.Nome)
            .Select(l => new LocalResponse
            {
                Id = l.Id,
                Nome = l.Nome,
                Latitude = l.Latitude,
                Longitude = l.Longitude
            })
            .ToListAsync(ct);
    }

    public async Task<LocalResponse?> ObterAsync(int id, CancellationToken ct = default)
    {
        return await _db.Locais
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new LocalResponse
            {
                Id = l.Id,
                Nome = l.Nome,
                Latitude = l.Latitude,
                Longitude = l.Longitude
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<LocalResponse> CriarAsync(CriarLocalRequest request, CancellationToken ct = default)
    {
        var local = new Local
        {
            Nome = request.Nome,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };

        _db.Locais.Add(local);
        await _db.SaveChangesAsync(ct);

        return new LocalResponse
        {
            Id = local.Id,
            Nome = local.Nome,
            Latitude = local.Latitude,
            Longitude = local.Longitude
        };
    }
}
