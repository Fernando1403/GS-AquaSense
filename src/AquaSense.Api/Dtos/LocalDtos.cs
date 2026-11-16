using System.ComponentModel.DataAnnotations;

namespace AquaSense.Api.Dtos;

public class CriarLocalRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }
}

public class LocalResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
