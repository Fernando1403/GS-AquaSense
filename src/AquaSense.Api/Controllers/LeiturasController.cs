using AquaSense.Api.Dtos;
using AquaSense.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AquaSense.Api.Controllers;

[ApiController]
[Route("leituras")]
public class LeiturasController : ControllerBase
{
    private readonly ILeituraService _leituras;

    public LeiturasController(ILeituraService leituras) => _leituras = leituras;

    /// <summary>Registra uma leitura de nível de água (usado pelo sensor ESP32).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LeituraResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeituraResponse>> Registrar(CriarLeituraRequest request, CancellationToken ct)
    {
        var leitura = await _leituras.RegistrarAsync(request, ct);
        if (leitura is null)
            return NotFound($"Local {request.LocalId} não encontrado.");

        return Created($"/locais/{leitura.LocalId}/leituras", leitura);
    }
}
