using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CasePan.Tests.Integration.Api;

public class PessoaJuridicaEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public PessoaJuridicaEndpointsTests(ApiFactory factory)
        => _client = factory.CreateClient();

    private sealed record IdResponse(Guid Id);
    private sealed record MessageResponse(string? Message, string? CorrelationId);
    private sealed record GetByIdResponse(string? CorrelationId, PessoaJuridicaDto? PessoaJuridica);
    private sealed record ListResponse(string? CorrelationId, List<PessoaJuridicaDto>? Items);

    private sealed class PessoaJuridicaDto
    {
        public Guid Id { get; set; }
        public string RazaoSocial { get; set; } = "";
        public string Cnpj { get; set; } = "";
        public Guid EnderecoId { get; set; }
    }

    [Fact]
    public async Task Fluxo_completo_pj_deve_criar_obter_atualizar_listar_e_remover()
    {
        // POST
        var payload = new
        {
            razaoSocial = "ACME LTDA",
            cnpj = "12.345.678/0001-99",
            cep = "01001-000",
            numero = "100",
            complemento = "Sala 1"
        };

        var post = await _client.PostAsJsonAsync("/api/pessoas-juridicas", payload);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await post.Content.ReadFromJsonAsync<IdResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();

        // GET by id (resposta envelopada: { correlationId, pessoaJuridica })
        var get = await _client.GetAsync($"/api/pessoas-juridicas/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var getBody = await get.Content.ReadFromJsonAsync<GetByIdResponse>();
        getBody.Should().NotBeNull();
        getBody!.CorrelationId.Should().NotBeNullOrWhiteSpace();
        getBody.PessoaJuridica.Should().NotBeNull();

        var pj = getBody.PessoaJuridica!;
        pj.Id.Should().Be(created.Id);
        pj.RazaoSocial.Should().Be("ACME LTDA");
        pj.Cnpj.Should().Be("12345678000199"); // sanitizado (14 dígitos)
        pj.EnderecoId.Should().NotBeEmpty();

        // LIST (resposta envelopada: { correlationId, items })
        var list = await _client.GetAsync("/api/pessoas-juridicas");
        list.StatusCode.Should().Be(HttpStatusCode.OK);

        var listBody = await list.Content.ReadFromJsonAsync<ListResponse>();
        listBody.Should().NotBeNull();
        listBody!.CorrelationId.Should().NotBeNullOrWhiteSpace();
        listBody.Items.Should().NotBeNull();
        listBody.Items!.Any(x => x.Id == created.Id).Should().BeTrue();

        // PUT
        var updatePayload = new
        {
            razaoSocial = "ACME ATUALIZADA",
            cnpj = "11.222.333/0001-44"
        };

        var put = await _client.PutAsJsonAsync($"/api/pessoas-juridicas/{created.Id}", updatePayload);
        put.StatusCode.Should().Be(HttpStatusCode.OK);

        var putBody = await put.Content.ReadFromJsonAsync<MessageResponse>();
        putBody.Should().NotBeNull();
        putBody!.CorrelationId.Should().NotBeNullOrWhiteSpace();

        // GET novamente (valida update)
        var get2 = await _client.GetAsync($"/api/pessoas-juridicas/{created.Id}");
        get2.StatusCode.Should().Be(HttpStatusCode.OK);

        var get2Body = await get2.Content.ReadFromJsonAsync<GetByIdResponse>();
        get2Body.Should().NotBeNull();
        get2Body!.PessoaJuridica.Should().NotBeNull();

        var pj2 = get2Body.PessoaJuridica!;
        pj2.RazaoSocial.Should().Be("ACME ATUALIZADA");
        pj2.Cnpj.Should().Be("11222333000144");

        // DELETE
        var del = await _client.DeleteAsync($"/api/pessoas-juridicas/{created.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);

        var delBody = await del.Content.ReadFromJsonAsync<MessageResponse>();
        delBody.Should().NotBeNull();
        delBody!.CorrelationId.Should().NotBeNullOrWhiteSpace();

        // GET após delete => 404
        var get3 = await _client.GetAsync($"/api/pessoas-juridicas/{created.Id}");
        get3.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_pj_com_cep_inexistente_deve_retornar_400()
    {
        var payload = new
        {
            razaoSocial = "Teste PJ",
            cnpj = "12345678000199",
            cep = "00000-000", // FakeViaCepClient => Erro=true
            numero = "1",
            complemento = (string?)null
        };

        var post = await _client.PostAsJsonAsync("/api/pessoas-juridicas", payload);
        post.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
