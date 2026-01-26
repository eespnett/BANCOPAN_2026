using CasePan.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CasePan.Api.Controllers;

[ApiController]
[Route("api/pessoas-juridicas")]
public class PessoasJuridicasController : ControllerBase
{
    private readonly PessoaJuridicaService _svc;

    public PessoasJuridicasController(PessoaJuridicaService svc) => _svc = svc;

    public sealed record CreateRequest(string RazaoSocial, string Cnpj, string Cep, string Numero, string? Complemento);
    public sealed record UpdateRequest(string RazaoSocial, string Cnpj);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest req, CancellationToken ct)
    {
        try
        {
            var id = await _svc.CriarAsync(req.RazaoSocial, req.Cnpj, req.Cep, req.Numero, req.Complemento, ct);
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
        var pj = await _svc.ObterAsync(id, ct);
        return pj is null ? NotFound() : Ok(pj);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await _svc.ListarAsync(ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest req, CancellationToken ct)
    {
        try
        {
            await _svc.AtualizarAsync(id, req.RazaoSocial, req.Cnpj, ct);
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
