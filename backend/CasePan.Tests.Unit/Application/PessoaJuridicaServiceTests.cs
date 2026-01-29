using CasePan.Application;
using CasePan.Application.Services;
using CasePan.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace CasePan.Tests.Unit.Application;

public class PessoaJuridicaServiceTests
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
    public async Task CriarAsync_DeveCriarEnderecoEPessoaJuridica()
    {
        var pjRepo = new Mock<IPessoaJuridicaRepository>();
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();

        viaCep.Setup(v => v.ConsultarAsync("01001000", It.IsAny<CancellationToken>()))
              .ReturnsAsync(ViaCepOk());

        var svc = new PessoaJuridicaService(pjRepo.Object, endRepo.Object, viaCep.Object);

        var id = await svc.CriarAsync("Empresa X", "04252011000110", "01001000", "10", null, CancellationToken.None);

        id.Should().NotBeEmpty();
        endRepo.Verify(r => r.AddAsync(It.IsAny<Endereco>(), It.IsAny<CancellationToken>()), Times.Once);
        pjRepo.Verify(r => r.AddAsync(It.IsAny<PessoaJuridica>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_QuandoViaCepFalhar_DeveLancar()
    {
        var pjRepo = new Mock<IPessoaJuridicaRepository>();
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();

        viaCep.Setup(v => v.ConsultarAsync("00000000", It.IsAny<CancellationToken>()))
              .ReturnsAsync((ViaCepResult?)null);

        var svc = new PessoaJuridicaService(pjRepo.Object, endRepo.Object, viaCep.Object);

        var act = async () => await svc.CriarAsync("Empresa X", "04252011000110", "00000000", "10", null, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CEP não encontrado*");
    }

    [Fact]
    public async Task AtualizarAsync_QuandoNaoExiste_DeveLancarKeyNotFound()
    {
        var pjRepo = new Mock<IPessoaJuridicaRepository>();
        var endRepo = new Mock<IEnderecoRepository>();
        var viaCep = new Mock<IViaCepClient>();

        pjRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((PessoaJuridica?)null);

        var svc = new PessoaJuridicaService(pjRepo.Object, endRepo.Object, viaCep.Object);

        var act = async () => await svc.AtualizarAsync(Guid.NewGuid(), "Empresa Y", "04252011000110", CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
