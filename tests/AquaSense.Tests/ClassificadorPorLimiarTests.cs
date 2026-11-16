using AquaSense.Api.Models;
using Xunit;

namespace AquaSense.Tests;

public class ClassificadorPorLimiarTests
{
    private readonly ClassificadorPorLimiar _classificador = new();

    [Theory]
    [InlineData(0, NivelRisco.Normal)]
    [InlineData(9.9, NivelRisco.Normal)]
    [InlineData(10, NivelRisco.Atencao)]
    [InlineData(24.9, NivelRisco.Atencao)]
    [InlineData(25, NivelRisco.Critico)]
    [InlineData(80, NivelRisco.Critico)]
    public void Classificar_DeveRetornarNivelEsperado(double nivelCm, NivelRisco esperado)
    {
        Assert.Equal(esperado, _classificador.Classificar(nivelCm));
    }

    [Fact]
    public void Classificar_NivelNegativo_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _classificador.Classificar(-1));
    }

    [Fact]
    public void Leitura_Classificar_UsaOClassificadorRecebido()
    {
        var leitura = new Leitura { NivelAgua = 30 };

        Assert.Equal(NivelRisco.Critico, leitura.Classificar(_classificador));
    }
}
