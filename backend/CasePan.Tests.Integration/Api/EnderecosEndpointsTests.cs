using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CasePan.Tests.Integration.Api;

public class EnderecosEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public EnderecosEndpointsTests(ApiFactory factory)
        => _client = factory.CreateClient();

    private sealed record IdResponse(Guid Id);
    private sealed record MessageResponse(string? Message, string? CorrelationId);
    private sealed record GetByIdResponse(string? CorrelationId, EnderecoDto? Endereco);
    private sealed record ListResponse(string? CorrelationId, List<EnderecoDto>? Items);

    private sealed class EnderecoDto
    {
        public Guid Id { get; set; }
        public string Cep { get; set; } = "";
        public string Logradouro { get; set; } = "";
        public string Numero { get; set; } = "";
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = "";
        public string Cidade { get; set; } = "";
        public string Uf { get; set; } = "";
    }

    private sealed class ViaCepResultDto
    {
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }
        public bool Erro { get; set; }
    }

    [Fact]
    public async Task Fluxo_completo_endereco_deve_criar_obter_atualizar_listar_e_remover()
    {
        // POST /api/enderecos (cria por CEP via FakeViaCepClient)
        var payload = new
        {
            cep = "01001-000",
            numero = "100",
            complemento = "Apto 11"
        };

        var post = await _client.PostAsJsonAsync("/api/enderecos", payload);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await post.Content.ReadFromJsonAsync<IdResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();

        // GET by id (envelope: { correlationId, endereco })
        var get = await _client.GetAsync($"/api/enderecos/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var getBody = await get.Content.ReadFromJsonAsync<GetByIdResponse>();
        getBody.Should().NotBeNull();
        getBody!.CorrelationId.Should().NotBeNullOrWhiteSpace();
        getBody.Endereco.Should().NotBeNull();

        var end = getBody.Endereco!;
        end.Id.Should().Be(created.Id);
        end.Cep.Should().Be("01001000"); // sanitizado
        end.Logradouro.Should().Be("Rua Teste");
        end.Bairro.Should().Be("Centro");
        end.Cidade.Should().Be("São Paulo");
        end.Uf.Should().Be("SP");
        end.Numero.Should().Be("100");
        end.Complemento.Should().Be("Apto 11");

        // LIST
        var list = await _client.GetAsync("/api/enderecos");
        list.StatusCode.Should().Be(HttpStatusCode.OK);

        var listBody = await list.Content.ReadFromJsonAsync<ListResponse>();
        listBody.Should().NotBeNull();
        listBody!.CorrelationId.Should().NotBeNullOrWhiteSpace();
        listBody.Items.Should().NotBeNull();
        listBody.Items!.Any(x => x.Id == created.Id).Should().BeTrue();

        // PUT
        var updatePayload = new
        {
            logradouro = "Rua Atualizada",
            numero = "200",
            complemento = (string?)null,
            bairro = "Bairro Novo",
            cidade = "Santo André",
            uf = "sp" // deve virar SP
        };

        var put = await _client.PutAsJsonAsync($"/api/enderecos/{created.Id}", updatePayload);
        put.StatusCode.Should().Be(HttpStatusCode.OK);

        var putBody = await put.Content.ReadFromJsonAsync<MessageResponse>();
        putBody.Should().NotBeNull();
        putBody!.CorrelationId.Should().NotBeNullOrWhiteSpace();

        // GET novamente para validar update
        var get2 = await _client.GetAsync($"/api/enderecos/{created.Id}");
        get2.StatusCode.Should().Be(HttpStatusCode.OK);

        var get2Body = await get2.Content.ReadFromJsonAsync<GetByIdResponse>();
        get2Body.Should().NotBeNull();
        get2Body!.Endereco.Should().NotBeNull();

        var end2 = get2Body.Endereco!;
        end2.Logradouro.Should().Be("Rua Atualizada");
        end2.Numero.Should().Be("200");
        end2.Complemento.Should().BeNull();
        end2.Bairro.Should().Be("Bairro Novo");
        end2.Cidade.Should().Be("Santo André");
        end2.Uf.Should().Be("SP");

        // DELETE
        var del = await _client.DeleteAsync($"/api/enderecos/{created.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);

        var delBody = await del.Content.ReadFromJsonAsync<MessageResponse>();
        delBody.Should().NotBeNull();
        delBody!.CorrelationId.Should().NotBeNullOrWhiteSpace();

        // GET após delete => 404
        var get3 = await _client.GetAsync($"/api/enderecos/{created.Id}");
        get3.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_endereco_com_cep_inexistente_deve_retornar_400()
    {
        var payload = new
        {
            cep = "00000-000", // FakeViaCepClient => Erro=true
            numero = "1",
            complemento = (string?)null
        };

        var post = await _client.PostAsJsonAsync("/api/enderecos", payload);
        post.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Lookup_deve_retornar_array_vazio_quando_cep_nao_encontrado()
    {
        var req = new { cep = "00000-000" };

        var resp = await _client.PostAsJsonAsync("/api/enderecos/lookup", req);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        // contrato: SEMPRE array
        var body = await resp.Content.ReadFromJsonAsync<List<ViaCepResultDto>>();
        body.Should().NotBeNull();
        body!.Count.Should().Be(0);

        resp.Headers.Contains("x-user-message").Should().BeTrue();
        resp.Headers.Contains("x-correlation-id").Should().BeTrue();
    }

    [Fact]
    public async Task Lookup_deve_retornar_array_com_1_item_quando_cep_existe()
    {
        var req = new { cep = "01001-000" };

        var resp = await _client.PostAsJsonAsync("/api/enderecos/lookup", req);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<List<ViaCepResultDto>>();
        body.Should().NotBeNull();
        body!.Count.Should().Be(1);

        body[0].Logradouro.Should().Be("Rua Teste");
        body[0].Bairro.Should().Be("Centro");
        body[0].Localidade.Should().Be("São Paulo");
        body[0].Uf.Should().Be("SP");

        resp.Headers.Contains("x-user-message").Should().BeTrue();
        resp.Headers.Contains("x-correlation-id").Should().BeTrue();
    }
}
