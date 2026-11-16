using AquaSense.Api.Dtos;

namespace AquaSense.Api.Services;

public interface ILocalService
{
    Task<List<LocalResponse>> ListarAsync(CancellationToken ct = default);
    Task<LocalResponse?> ObterAsync(int id, CancellationToken ct = default);
    Task<LocalResponse> CriarAsync(CriarLocalRequest request, CancellationToken ct = default);
}
