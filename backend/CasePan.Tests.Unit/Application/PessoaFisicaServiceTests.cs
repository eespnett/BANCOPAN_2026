using CasePan.Application;
using CasePan.Application.Services;
using CasePan.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace CasePan.Tests.Unit.Application;

public class PessoaFisicaServiceTests
{
    [Fact]
    public async Task CriarAsync_deve_consultar_viacep_e_salvar_pf_e_endereco()
    {
        var pfRepo = new Mock<IPessoaFisicaRepository>();
        var endRepo = new Mock<IEnderecoRepository>();
        var via = new Mock<IViaCepClient>();

        via.Setup(x => x.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new ViaCepResult
           {
               Logradouro = "Rua Teste",
               Bairro = "Centro",
               Localidade = "São Paulo",
               Uf = "SP",
               Erro = false
           });

        var svc = new PessoaFisicaService(pfRepo.Object, endRepo.Object, via.Object);

        var id = await svc.CriarAsync("Eder Sousa", "12345678901", "01001-000", "100", null, CancellationToken.None);

        id.Should().NotBeEmpty();
        via.Verify(x => x.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        endRepo.Verify(x => x.AddAsync(It.IsAny<Endereco>(), It.IsAny<CancellationToken>()), Times.Once);
        pfRepo.Verify(x => x.AddAsync(It.IsAny<PessoaFisica>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
