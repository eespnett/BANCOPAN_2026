using CasePan.Application;
using CasePan.Application.Services;
using CasePan.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace CasePan.Tests.Unit.Application;

public class PessoaFisicaServiceTests
{
    private static ViaCepResult ViaCepOk() => new ViaCepResult
    {
       
        Logradouro = "Rua X",
        Bairro = "Bairro Y",
        Localidade = "São Paulo",
        Uf = "SP",
        Erro = false
    };


    [Fact]
    public async Task CriarAsync_DeveCriarEnderecoEPessoaFisica()
    {
        var pfRepo = new Mock<IPessoaFisicaRepository>();
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();

        viaCep.Setup(v => v.ConsultarAsync("01001000", It.IsAny<CancellationToken>()))
              .ReturnsAsync(ViaCepOk());

        var svc = new PessoaFisicaService(pfRepo.Object, endRepo.Object, viaCep.Object);

        var id = await svc.CriarAsync("Eder", "52998224725", "01001000", "123", null, CancellationToken.None);

        id.Should().NotBeEmpty();
        endRepo.Verify(r => r.AddAsync(It.IsAny<Endereco>(), It.IsAny<CancellationToken>()), Times.Once);
        pfRepo.Verify(r => r.AddAsync(It.IsAny<PessoaFisica>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_QuandoViaCepNaoEncontrar_DeveLancar()
    {
        var pfRepo = new Mock<IPessoaFisicaRepository>();
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();

        viaCep.Setup(v => v.ConsultarAsync("00000000", It.IsAny<CancellationToken>()))
              .ReturnsAsync((ViaCepResult?)null);

        var svc = new PessoaFisicaService(pfRepo.Object, endRepo.Object, viaCep.Object);

        var act = async () => await svc.CriarAsync("Eder", "52998224725", "00000000", "10", null, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CEP não encontrado*");
    }

    [Fact]
    public async Task AtualizarAsync_QuandoNaoExiste_DeveLancarKeyNotFound()
    {
        var pfRepo = new Mock<IPessoaFisicaRepository>();
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();

        pfRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((PessoaFisica?)null);

        var svc = new PessoaFisicaService(pfRepo.Object, endRepo.Object, viaCep.Object);

        var act = async () => await svc.AtualizarAsync(Guid.NewGuid(), "Novo Nome", "52998224725", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task RemoverAsync_QuandoNaoExiste_DeveLancarKeyNotFound()
    {
        var pfRepo = new Mock<IPessoaFisicaRepository>();
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();

        pfRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((PessoaFisica?)null);

        var svc = new PessoaFisicaService(pfRepo.Object, endRepo.Object, viaCep.Object);

        var act = async () => await svc.RemoverAsync(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
