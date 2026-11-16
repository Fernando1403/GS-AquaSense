using System.ComponentModel.DataAnnotations;

namespace AquaSense.Api.Dtos;

public class CriarLeituraRequest
{
    [Range(1, int.MaxValue)]
    public int LocalId { get; set; }

    /// <summary>Nível da água em centímetros.</summary>
    [Range(0, 1000)]
    public double NivelAgua { get; set; }

    /// <summary>Opcional. Se não informado, usa a hora atual (UTC).</summary>
    public DateTime? DataHora { get; set; }
}

public class LeituraResponse
{
    public int Id { get; set; }
    public int LocalId { get; set; }
    public double NivelAgua { get; set; }
    public DateTime DataHora { get; set; }
    public string Nivel { get; set; } = string.Empty;
}

public class ResumoHoraResponse
{
    public DateTime Hora { get; set; }
    public double NivelMedio { get; set; }
    public double NivelMaximo { get; set; }
    public int Quantidade { get; set; }
}
