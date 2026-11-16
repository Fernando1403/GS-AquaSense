namespace AquaSense.Api.Models;

public interface IClassificadorNivel
{
    NivelRisco Classificar(double nivelAguaCm);
}
