using CasePan.Api.Observability;
using CasePan.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace CasePan.Api.Controllers;

[ApiController]
[Route("api/pessoas-fisicas")]
public class PessoasFisicasController : ControllerBase
{
    private readonly PessoaFisicaService _svc;
    private readonly IControllerEventTracker _tracker;

    public PessoasFisicasController(PessoaFisicaService svc, IControllerEventTracker tracker)
    {
        _svc = svc;
        _tracker = tracker;
    }

    public sealed record CreateRequest(string Nome, string Cpf, string Cep, string Numero, string? Complemento = null);
    public sealed record UpdateRequest(string Nome, string Cpf);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest req, CancellationToken ct)
    {
        const string okMsg = "Cadastro de pessoa física realizado com sucesso.";
        const string errMsg = "Houve um erro ao cadastrar a pessoa física.";

        try
        {
            var id = await _svc.CriarAsync(req.Nome, req.Cpf, req.Cep, req.Numero, req.Complemento, ct);

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaFisicaCreated",
                userMessage: okMsg,
                payload: new { id, cpfLast4 = Last4(req.Cpf), cep = req.Cep },
                outcome: "success",
                ct);

            return CreatedAtAction(nameof(GetById), new { id }, new { id, message = okMsg, correlationId = cid });
        }
        catch (Exception ex)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaFisicaCreateFailed",
                userMessage: errMsg,
                payload: new { cpfLast4 = Last4(req.Cpf), cep = req.Cep },
                outcome: "failure",
                ct,
                ex);

            return BadRequest(new { message = errMsg, correlationId = cid, error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var pf = await _svc.ObterAsync(id, ct);

        if (pf is null)
        {
            const string nfMsg = "Pessoa física não encontrada.";

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaFisicaGetNotFound",
                userMessage: nfMsg,
                payload: new { id },
                outcome: "not_found",
                ct);

            return NotFound(new { message = nfMsg, correlationId = cid });
        }

        var okCid = await _tracker.TrackAsync(
            HttpContext,
            eventName: "PessoaFisicaGetOk",
            userMessage: "Consulta de pessoa física realizada com sucesso.",
            payload: new { id },
            outcome: "success",
            ct);

        return Ok(new { correlationId = okCid, pessoaFisica = pf });
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _svc.ListarAsync(ct);

        var cid = await _tracker.TrackAsync(
            HttpContext,
            eventName: "PessoaFisicaListOk",
            userMessage: "Listagem de pessoas físicas realizada com sucesso.",
            payload: new { count = items.Count },
            outcome: "success",
            ct);

        return Ok(new { correlationId = cid, items });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest req, CancellationToken ct)
    {
        const string okMsg = "Cadastro de pessoa física atualizado com sucesso.";
        const string nfMsg = "Pessoa física não encontrada.";
        const string errMsg = "Houve um erro ao atualizar a pessoa física.";

        try
        {
            await _svc.AtualizarAsync(id, req.Nome, req.Cpf, ct);

            var cid=   await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaFisicaUpdated",
                userMessage: okMsg,
                payload: new { id, cpfLast4 = Last4(req.Cpf) },
                outcome: "success",
                ct);

            return Ok(new { message = "Pessoa física atualizada com sucesso.", correlationId = cid });
        }
        catch (KeyNotFoundException)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaFisicaUpdateNotFound",
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
                eventName: "PessoaFisicaUpdateFailed",
                userMessage: errMsg,
                payload: new { id, cpfLast4 = Last4(req.Cpf) },
                outcome: "failure",
                ct,
                ex);

            return BadRequest(new { message = errMsg, correlationId = cid, error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        const string okMsg = "Cadastro de pessoa física removido com sucesso.";
        const string nfMsg = "Pessoa física não encontrada.";
        const string errMsg = "Houve um erro ao remover a pessoa física.";

        try
        {
            await _svc.RemoverAsync(id, ct);

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaFisicaDeleted",
                userMessage: okMsg,
                payload: new { id },
                outcome: "success",
                ct);
            return Ok(new { message = "Pessoa física Excluida com sucesso.", correlationId = cid });
        }
        catch (KeyNotFoundException)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "PessoaFisicaDeleteNotFound",
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
                eventName: "PessoaFisicaDeleteFailed",
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
