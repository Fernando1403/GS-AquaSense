using AquaSense.Api.Dtos;
using AquaSense.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AquaSense.Api.Controllers;

[ApiController]
[Route("locais")]
public class LocaisController : ControllerBase
{
    private readonly ILocalService _locais;
    private readonly ILeituraService _leituras;

    public LocaisController(ILocalService locais, ILeituraService leituras)
    {
        _locais = locais;
        _leituras = leituras;
    }

    /// <summary>Lista os locais monitorados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<LocalResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LocalResponse>>> Listar(CancellationToken ct)
        => Ok(await _locais.ListarAsync(ct));

    /// <summary>Obtém um local pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LocalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocalResponse>> Obter(int id, CancellationToken ct)
    {
        var local = await _locais.ObterAsync(id, ct);
        return local is null ? NotFound() : Ok(local);
    }

    /// <summary>Cadastra um novo local monitorado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LocalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocalResponse>> Criar(CriarLocalRequest request, CancellationToken ct)
    {
        var criado = await _locais.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Lista as leituras do sensor de um local (mais recentes primeiro).</summary>
    [HttpGet("{id:int}/leituras")]
    [ProducesResponseType(typeof(List<LeituraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<LeituraResponse>>> ListarLeituras(int id, CancellationToken ct)
    {
        if (await _locais.ObterAsync(id, ct) is null)
            return NotFound();

        return Ok(await _leituras.ListarPorLocalAsync(id, ct));
    }

    /// <summary>Resumo por hora (média, máximo e quantidade) das leituras de um local.</summary>
    [HttpGet("{id:int}/resumo-por-hora")]
    [ProducesResponseType(typeof(List<ResumoHoraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ResumoHoraResponse>>> ResumoPorHora(int id, CancellationToken ct)
    {
        if (await _locais.ObterAsync(id, ct) is null)
            return NotFound();

        return Ok(await _leituras.ResumoPorHoraAsync(id, ct));
    }
}
