using CasePan.Domain;
using FluentAssertions;
using Xunit;

namespace CasePan.Tests.Unit.Domain;

public class PessoaFisicaTests
{
    [Fact]
    public void Deve_normalizar_cpf_removendo_nao_digitos()
    {
        var pf = new PessoaFisica("Nome", "123.456.789-01", Guid.NewGuid());
        pf.Cpf.Should().Be("12345678901");
    }

    [Fact]
    public void Deve_falhar_quando_cpf_nao_tem_11_digitos()
    {
        var act = () => new PessoaFisica("Nome", "123", Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*CPF inválido*");
    }

    [Fact]
    public void Deve_falhar_quando_nome_esta_vazio()
    {
        var act = () => new PessoaFisica(" ", "12345678901", Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*Nome é obrigatório*");
    }
}
