using CasePan.Application;
using CasePan.Application.Services;
using CasePan.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace CasePan.Tests.Unit.Application;

public class EnderecoServiceTests
{
    [Fact]
    public async Task ConsultarCepAsync_ComCepInvalido_DeveRetornarNull()
    {
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();
        var svc = new EnderecoService(endRepo.Object, viaCep.Object);

        var res = await svc.ConsultarCepAsync("123", CancellationToken.None);

        res.Should().BeNull();
        viaCep.Verify(v => v.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarPorCepAsync_QuandoViaCepNaoEncontrar_DeveLancar()
    {
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();

        viaCep.Setup(v => v.ConsultarAsync("00000000", It.IsAny<CancellationToken>()))
              .ReturnsAsync((ViaCepResult?)null);

        var svc = new EnderecoService(endRepo.Object, viaCep.Object);

        var act = async () => await svc.CriarPorCepAsync("00000000", "10", null, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CEP não encontrado*");
    }

    [Fact]
    public async Task CriarPorCepAsync_ComViaCepOk_DeveSalvarEndereco()
    {
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();
        ViaCepResult ViaCepResultreturn = new ViaCepResult();
        ViaCepResultreturn.Bairro = "Bairro Y";
        ViaCepResultreturn.Erro = false;
        ViaCepResultreturn.Logradouro = "Rua X";
        ViaCepResultreturn.Uf = "SP";
        ViaCepResultreturn.Localidade = "São Paulo";


        viaCep.Setup(v => v.ConsultarAsync("01001000", It.IsAny<CancellationToken>()))
              .ReturnsAsync(ViaCepResultreturn);

        var svc = new EnderecoService(endRepo.Object, viaCep.Object);

        var id = await svc.CriarPorCepAsync("01001000", "10", "Apto", CancellationToken.None);

        id.Should().NotBeEmpty();
        endRepo.Verify(r => r.AddAsync(It.IsAny<Endereco>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
