using CasePan.Api.Observability;
using CasePan.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace CasePan.Api.Controllers;

[ApiController]
[Route("api/enderecos")]
public class EnderecosController : ControllerBase
{
    private readonly EnderecoService _svc;
    private readonly IControllerEventTracker _tracker;

    public EnderecosController(EnderecoService svc, IControllerEventTracker tracker)
    {
        _svc = svc;
        _tracker = tracker;
    }
    public sealed record LookupRequest(string Cep);
    public sealed record CreateRequest(string Cep, string Numero, string? Complemento);
    public sealed record UpdateRequest(string Logradouro, string Numero, string? Complemento, string Bairro, string Cidade, string Uf);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRequest req, CancellationToken ct)
    {
        const string okMsg = "Endereço cadastrado com sucesso.";
        const string errMsg = "Houve um erro ao cadastrar o endereço.";

        try
        {
            var id = await _svc.CriarPorCepAsync(req.Cep, req.Numero, req.Complemento, ct);

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "EnderecoCreated",
                userMessage: okMsg,
                payload: new { id, cep = req.Cep },
                outcome: "success",
                ct);

           
            return CreatedAtAction(nameof(GetById), new { id }, new { id, message = okMsg, correlationId = cid });
        }
        catch (Exception ex)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "EnderecoCreateFailed",
                userMessage: errMsg,
                payload: new { cep = req.Cep },
                outcome: "failure",
                ct,
                ex);

            return BadRequest(new { message = errMsg, correlationId = cid, error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var end = await _svc.ObterAsync(id, ct);

        if (end is null)
        {
            const string nfMsg = "Endereço não encontrado.";

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "EnderecoGetNotFound",
                userMessage: nfMsg,
                payload: new { id },
                outcome: "not_found",
                ct);

            return NotFound(new { message = nfMsg, correlationId = cid });
        }

        var okCid = await _tracker.TrackAsync(
            HttpContext,
            eventName: "EnderecoGetOk",
            userMessage: "Consulta de endereço realizada com sucesso.",
            payload: new { id },
            outcome: "success",
            ct);

        return Ok(new { correlationId = okCid, endereco = end });
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var items = await _svc.ListarAsync(ct);

        var cid = await _tracker.TrackAsync(
            HttpContext,
            eventName: "EnderecoListOk",
            userMessage: "Listagem de endereços realizada com sucesso.",
            payload: new { count = items.Count },
            outcome: "success",
            ct);

        return Ok(new { correlationId = cid, items });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRequest req, CancellationToken ct)
    {
        const string okMsg = "Endereço atualizado com sucesso.";
        const string nfMsg = "Endereço não encontrado.";
        const string errMsg = "Houve um erro ao atualizar o endereço.";

        try
        {
            await _svc.AtualizarAsync(id, req.Logradouro, req.Numero, req.Complemento, req.Bairro, req.Cidade, req.Uf, ct);

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "EnderecoUpdated",
                userMessage: okMsg,
                payload: new { id },
                outcome: "success",
                ct);

            return Ok(new { message = "Endereço Atualizado com sucesso.", correlationId = cid });
        }
        catch (KeyNotFoundException)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "EnderecoUpdateNotFound",
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
                eventName: "EnderecoUpdateFailed",
                userMessage: errMsg,
                payload: new { id },
                outcome: "failure",
                ct,
                ex);

            return BadRequest(new { message = errMsg, correlationId = cid, error = ex.Message });
        }
    }

    [HttpPost("lookup")]
    public async Task<IActionResult> Lookup([FromBody] LookupRequest req, CancellationToken ct)
    {
        const string okMsg = "Consulta de CEP realizada com sucesso.";
        const string notFoundMsg = "CEP não encontrado.";
        const string errMsg = "Houve um erro ao consultar o CEP.";

        try
        {
            var end = await _svc.LookupCepAsync(req.Cep, ct);

            var cid = await _tracker.TrackSuccessAsync(
                HttpContext,
                eventName: end is null ? "EnderecoLookupNotFound" : "EnderecoLookupOk",
                userMessage: end is null ? notFoundMsg : okMsg,
                payload: new { cep = req.Cep, found = end is not null },
                ct);

            // (Opcional) mensagem para o front via header, sem quebrar o ngFor
            Response.Headers["x-user-message"] = end is null ? notFoundMsg : okMsg;
            Response.Headers["x-correlation-id"] = cid;

            // ✅ IMPORTANTE: o front recebe SEMPRE array
            if (end is null)
                return Ok(Array.Empty<object>());

            return Ok(new[] { end });
        }
        catch (Exception ex)
        {
            var cid = await _tracker.TrackFailureAsync(
                HttpContext,
                eventName: "EnderecoLookupFailed",
                userMessage: errMsg  ,
                payload: new { cep = req.Cep },  ex,
                
                ct);

            Response.Headers["x-user-message"] = errMsg;
            Response.Headers["x-correlation-id"] = cid;

            // ✅ também devolve array para não estourar ngFor se o front atribuir mesmo em erro
            return StatusCode(500, Array.Empty<object>());
        }
    }


    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        const string okMsg = "Endereço removido com sucesso.";
        const string nfMsg = "Endereço não encontrado.";
        const string errMsg = "Houve um erro ao remover o endereço.";

        try
        {
            // 🔴 NÃO use "var ok = await ..." aqui (RemoverAsync é Task/void)
            await _svc.RemoverAsync(id, ct);

            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "EnderecoDeleted",
                userMessage: okMsg,
                payload: new { id },
                outcome: "success",
                ct);

            return Ok(new { message = "Endereço Excluído com sucesso.", correlationId = cid });
        }
        catch (KeyNotFoundException)
        {
            var cid = await _tracker.TrackAsync(
                HttpContext,
                eventName: "EnderecoDeleteNotFound",
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
                eventName: "EnderecoDeleteFailed",
                userMessage: errMsg,
                payload: new { id },
                outcome: "failure",
                ct,
                ex);

            return BadRequest(new { message = errMsg, correlationId = cid, error = ex.Message });
        }
    }
}
