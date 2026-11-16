namespace AquaSense.Api.Models;

/// <summary>Medição de nível de água enviada pelo sensor (ESP32).</summary>
public class Leitura
{
    public int Id { get; set; }
    public int LocalId { get; set; }
    public Local? Local { get; set; }

    /// <summary>Nível da água em centímetros.</summary>
    public double NivelAgua { get; set; }

    /// <summary>Data e hora da coleta (UTC).</summary>
    public DateTime DataHora { get; set; }

    /// <summary>Classifica esta leitura usando a estratégia recebida (interface).</summary>
    public NivelRisco Classificar(IClassificadorNivel classificador)
        => classificador.Classificar(NivelAgua);
}
