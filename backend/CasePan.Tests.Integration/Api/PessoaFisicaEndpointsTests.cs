using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CasePan.Tests.Integration.Api;

public class PessoaFisicaEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public PessoaFisicaEndpointsTests(ApiFactory factory)
        => _client = factory.CreateClient();

    private sealed record IdResponse(Guid Id);

    private sealed class PessoaFisicaDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = "";
        public string Cpf { get; set; } = "";
        public Guid EnderecoId { get; set; }
    }

    [Fact]
    public async Task Fluxo_completo_pf_deve_criar_obter_atualizar_e_remover()
    {
        // POST
        var payload = new
        {
            nome = "Eder Sousa",
            cpf = "123.456.789-01",
            cep = "01001-000",
            numero = "100",
            complemento = "Apto 11"
        };

        var post = await _client.PostAsJsonAsync("/api/pessoas-fisicas", payload);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await post.Content.ReadFromJsonAsync<IdResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();

        // GET
        var get = await _client.GetAsync($"/api/pessoas-fisicas/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var pf = await get.Content.ReadFromJsonAsync<PessoaFisicaDto>();
        pf.Should().NotBeNull();
        pf!.Id.Should().Be(created.Id);
        pf.Nome.Should().Be("Eder Sousa");
        pf.Cpf.Should().Be("12345678901");
        pf.EnderecoId.Should().NotBeEmpty();

        // PUT
        var updatePayload = new { nome = "Eder Atualizado", cpf = "987.654.321-00" };
        var put = await _client.PutAsJsonAsync($"/api/pessoas-fisicas/{created.Id}", updatePayload);
        put.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get2 = await _client.GetAsync($"/api/pessoas-fisicas/{created.Id}");
        var pf2 = await get2.Content.ReadFromJsonAsync<PessoaFisicaDto>();
        pf2!.Nome.Should().Be("Eder Atualizado");
        pf2.Cpf.Should().Be("98765432100");

        // DELETE
        var del = await _client.DeleteAsync($"/api/pessoas-fisicas/{created.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get3 = await _client.GetAsync($"/api/pessoas-fisicas/{created.Id}");
        get3.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_pf_com_cep_inexistente_deve_retornar_400()
    {
        var payload = new
        {
            nome = "Teste",
            cpf = "12345678901",
            cep = "00000-000", // fake simula "erro=true"
            numero = "1",
            complemento = (string?)null
        };

        var post = await _client.PostAsJsonAsync("/api/pessoas-fisicas", payload);
        post.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
