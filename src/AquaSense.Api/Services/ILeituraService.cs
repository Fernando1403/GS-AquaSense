using AquaSense.Api.Dtos;

namespace AquaSense.Api.Services;

public interface ILeituraService
{
    /// <summary>Registra uma leitura. Retorna null se o local não existir.</summary>
    Task<LeituraResponse?> RegistrarAsync(CriarLeituraRequest request, CancellationToken ct = default);

    /// <summary>Lista leituras de um local, da mais recente para a mais antiga.</summary>
    Task<List<LeituraResponse>> ListarPorLocalAsync(int localId, CancellationToken ct = default);

    /// <summary>Resumo (média, máximo e quantidade) por hora das leituras de um local.</summary>
    Task<List<ResumoHoraResponse>> ResumoPorHoraAsync(int localId, CancellationToken ct = default);
}
