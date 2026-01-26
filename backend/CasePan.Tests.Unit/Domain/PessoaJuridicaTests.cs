using CasePan.Domain;
using FluentAssertions;
using Xunit;

namespace CasePan.Tests.Unit.Domain;

public class PessoaJuridicaTests
{
    [Fact]
    public void Deve_normalizar_cnpj_removendo_nao_digitos()
    {
        var pj = new PessoaJuridica("Empresa X", "12.345.678/0001-90", Guid.NewGuid());
        pj.Cnpj.Should().Be("12345678000190");
    }

    [Fact]
    public void Deve_falhar_quando_cnpj_nao_tem_14_digitos()
    {
        var act = () => new PessoaJuridica("Empresa X", "123", Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*CNPJ inválido*");
    }

    [Fact]
    public void Deve_falhar_quando_razao_social_esta_vazia()
    {
        var act = () => new PessoaJuridica(" ", "12345678000190", Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*Razão social é obrigatória*");
    }
}
