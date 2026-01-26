using CasePan.Domain;
using FluentAssertions;
using Xunit;

namespace CasePan.Tests.Unit.Domain;

public class EnderecoTests
{
    [Fact]
    public void Deve_falhar_quando_cep_nao_tem_8_digitos()
    {
        var act = () => new Endereco("123", "Rua X", "10", null, "Centro", "Cidade", "SP");
        act.Should().Throw<DomainException>().WithMessage("*CEP inválido*");
    }

    [Fact]
    public void Deve_falhar_quando_uf_nao_tem_2_caracteres()
    {
        var act = () => new Endereco("01001000", "Rua X", "10", null, "Centro", "Cidade", "S");
        act.Should().Throw<DomainException>().WithMessage("*UF inválida*");
    }

    [Fact]
    public void Deve_normalizar_uf_para_uppercase()
    {
        var e = new Endereco("01001000", "Rua X", "10", null, "Centro", "Cidade", "sp");
        e.Uf.Should().Be("SP");
    }
}
