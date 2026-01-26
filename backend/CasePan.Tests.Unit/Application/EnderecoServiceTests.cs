using CasePan.Application;
using CasePan.Application.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CasePan.Tests.Unit.Application;

public class EnderecoServiceTests
{
    [Fact]
    public async Task CriarPorCepAsync_deve_falhar_quando_viacep_retorna_erro()
    {
        var repo = new Mock<IEnderecoRepository>();
        var via = new Mock<IViaCepClient>();

        via.Setup(x => x.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new ViaCepResult { Erro = true });

        var svc = new EnderecoService(repo.Object, via.Object);

        var act = async () => await svc.CriarPorCepAsync("00000-000", "10", null, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*CEP não encontrado*");
    }
}
