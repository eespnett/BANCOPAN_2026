using CasePan.Api.Observability;
using CasePan.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace CasePan.Api.Controllers;

[ApiController]
[Route("api/pessoas-juridicas")]
public class PessoasJuridicasController : ControllerBase
{
    private readonly PessoaJuridicaService _svc;
    private readonly IControllerEventTracker _tracker;

    public PessoasJuridicasController(PessoaJuridicaService svc, IControllerEventTracker tracker)
    {
        _svc = svc;
        _tracker = tracker;
    }

    public sealed record CreateRequest(string RazaoSocial, string Cnpj, string Cep, string Numero, string? Complemento = null);
    public sealed record UpdateRequest(string RazaoSocial, string Cnpj);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest req, CancellationToken ct)
    {
        const string okMsg = "Cadastro de pessoa jurídica realizado com sucesso.";
        const string errMsg = "Houve um erro ao cadastrar a pessoa jurídica.";

        try
        {
            var id = await _svc.CriarAsync(req.RazaoSocial, req.Cnpj, req.Cep, req.Numero, req.Complemento, ct);

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaJuridicaCreated",
                userMessage: okMsg,
                payload: new { id, cnpjLast4 = Last4(req.Cnpj), cep = req.Cep },
                outcome: "success",
                ct);

            return CreatedAtAction(nameof(GetById), new { id }, new { id, message = okMsg, correlationId = cid });
        }
        catch (Exception ex)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaJuridicaCreateFailed",
                userMessage: errMsg,
                payload: new { cnpjLast4 = Last4(req.Cnpj), cep = req.Cep },
                outcome: "failure",
                ct,
                ex);

            return BadRequest(new { message = errMsg, correlationId = cid, error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var pj = await _svc.ObterAsync(id, ct);

        if (pj is null)
        {
            const string nfMsg = "Pessoa jurídica não encontrada.";

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaJuridicaGetNotFound",
                userMessage: nfMsg,
                payload: new { id },
                outcome: "not_found",
                ct);

            return NotFound(new { message = nfMsg, correlationId = cid });
        }

        var okCid = await _tracker.TrackAsync(
            HttpContext,
            eventName: "PessoaJuridicaGetOk",
            userMessage: "Consulta de pessoa jurídica realizada com sucesso.",
            payload: new { id },
            outcome: "success",
            ct);

        return Ok(new { correlationId = okCid, pessoaJuridica = pj });
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _svc.ListarAsync(ct);

        var cid = await _tracker.TrackAsync(
            HttpContext,
            eventName: "PessoaJuridicaListOk",
            userMessage: "Listagem de pessoas jurídicas realizada com sucesso.",
            payload: new { count = items.Count },
            outcome: "success",
            ct);

        return Ok(new { correlationId = cid, items });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest req, CancellationToken ct)
    {
        const string okMsg = "Cadastro de pessoa jurídica atualizado com sucesso.";
        const string nfMsg = "Pessoa jurídica não encontrada.";
        const string errMsg = "Houve um erro ao atualizar a pessoa jurídica.";

        try
        {
            await _svc.AtualizarAsync(id, req.RazaoSocial, req.Cnpj, ct);

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaJuridicaUpdated",
                userMessage: okMsg,
                payload: new { id, cnpjLast4 = Last4(req.Cnpj) },
                outcome: "success",
                ct);

            return Ok(new { message = "Pessoa Jurídica Excluida com sucesso.", correlationId = cid });
        }
        catch (KeyNotFoundException)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaJuridicaUpdateNotFound",
                userMessage: nfMsg,
                payload: new { id },
                outcome: "not_found",
                ct);

            return NotFound(new { message = nfMsg, correlationId = cid });
        }
        catch (Exception ex)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaJuridicaUpdateFailed",
                userMessage: errMsg,
                payload: new { id, cnpjLast4 = Last4(req.Cnpj) },
                outcome: "failure",
                ct,
                ex);

            return BadRequest(new { message = errMsg, correlationId = cid, error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        const string okMsg = "Cadastro de pessoa jurídica removido com sucesso.";
        const string nfMsg = "Pessoa jurídica não encontrada.";
        const string errMsg = "Houve um erro ao remover a pessoa jurídica.";

        try
        {
            await _svc.RemoverAsync(id, ct);

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaJuridicaDeleted",
                userMessage: okMsg,
                payload: new { id },
                outcome: "success",
                ct);

            return Ok(new { message = "Pessoa Jurídica Excluida com sucesso.", correlationId = cid });
        }
        catch (KeyNotFoundException)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaJuridicaDeleteNotFound",
                userMessage: nfMsg,
                payload: new { id },
                outcome: "not_found",
                ct);

            return NotFound(new { message = nfMsg, correlationId = cid });
        }
        catch (Exception ex)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaJuridicaDeleteFailed",
                userMessage: errMsg,
                payload: new { id },
                outcome: "failure",
                ct,
                ex);

            return BadRequest(new { message = errMsg, correlationId = cid, error = ex.Message });
        }
    }

    private static string Last4(string? value)
    {
        var digits = new string((value ?? "").Where(char.IsDigit).ToArray());
        return digits.Length <= 4 ? digits : digits[^4..];
    }
}
