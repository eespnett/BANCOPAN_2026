using CasePan.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CasePan.Api.Controllers;

[ApiController]
[Route("api/enderecos")]
public class EnderecosController : ControllerBase
{
    private readonly EnderecoService _svc;

    public EnderecosController(EnderecoService svc) => _svc = svc;

    public sealed record CreateRequest(string Cep, string Numero, string? Complemento);
    public sealed record UpdateRequest(string Logradouro, string Numero, string? Complemento, string Bairro, string Cidade, string Uf);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest req, CancellationToken ct)
    {
        try
        {
            var id = await _svc.CriarPorCepAsync(req.Cep, req.Numero, req.Complemento, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var end = await _svc.ObterAsync(id, ct);
        return end is null ? NotFound() : Ok(end);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await _svc.ListarAsync(ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest req, CancellationToken ct)
    {
        try
        {
            await _svc.AtualizarAsync(id, req.Logradouro, req.Numero, req.Complemento, req.Bairro, req.Cidade, req.Uf, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _svc.RemoverAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
