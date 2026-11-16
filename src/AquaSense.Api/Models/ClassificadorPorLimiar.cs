namespace AquaSense.Api.Models;

/// <summary>
/// Classificador simples por limiares (valores didáticos, não oficiais):
/// abaixo de 10 cm = Normal, de 10 a 24,99 cm = Atenção, 25 cm ou mais = Crítico.
/// </summary>
public class ClassificadorPorLimiar : IClassificadorNivel
{
    public const double LimiteAtencaoCm = 10;
    public const double LimiteCriticoCm = 25;

    public NivelRisco Classificar(double nivelAguaCm)
    {
        if (nivelAguaCm < 0)
            throw new ArgumentOutOfRangeException(nameof(nivelAguaCm), "O nível de água não pode ser negativo.");

        if (nivelAguaCm >= LimiteCriticoCm) return NivelRisco.Critico;
        if (nivelAguaCm >= LimiteAtencaoCm) return NivelRisco.Atencao;
        return NivelRisco.Normal;
    }
}
