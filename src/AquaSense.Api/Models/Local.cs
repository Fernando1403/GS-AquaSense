namespace AquaSense.Api.Models;

/// <summary>Ponto monitorado (ex.: um trecho de rua com histórico de alagamento).</summary>
public class Local
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    // Coordenadas aproximadas do ponto monitorado (nunca endereço residencial).
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public List<Leitura> Leituras { get; set; } = new();
}
