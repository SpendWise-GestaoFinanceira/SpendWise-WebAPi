using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendWise.Application.Commands.FechamentoMensal;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.FechamentoMensal;
using SpendWise.API.Extensions;

namespace SpendWise.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FechamentoMensalController : ControllerBase
{
    private readonly IMediator _mediator;

    public FechamentoMensalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obter fechamento por ID
    /// <summary>
    /// Obter fechamento por ID (apenas próprio)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FechamentoMensalDto>> GetById(Guid id)
    {
        var usuarioId = User.GetUserId();
        var query = new GetFechamentoMensalByIdQuery(id);
        var fechamento = await _mediator.Send(query);

        if (fechamento == null || fechamento.UsuarioId != usuarioId)
            return NotFound();

        return Ok(fechamento);
    }

    /// <summary>
    /// Obter meus fechamentos
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FechamentoMensalDto>>> GetMyFechamentos()
    {
        var usuarioId = User.GetUserId();
        var query = new GetFechamentosMensaisByUsuarioQuery(usuarioId);
        var fechamentos = await _mediator.Send(query);
        return Ok(fechamentos);
    }

    /// <summary>
    /// Obter meu fechamento de um mês específico
    /// </summary>
    [HttpGet("mes/{anoMes}")]
    public async Task<ActionResult<FechamentoMensalDto>> GetByMes(string anoMes)
    {
        var usuarioId = User.GetUserId();
        var query = new GetFechamentoMensalByUsuarioEAnoMesQuery(usuarioId, anoMes);
        var fechamento = await _mediator.Send(query);

        if (fechamento == null)
            return NotFound($"Não foi encontrado fechamento para o mês {anoMes}");

        return Ok(fechamento);
    }

    /// <summary>
    /// Fechar meu mês
    /// </summary>
    [HttpPost("fechar")]
    public async Task<ActionResult<FechamentoMensalDto>> FecharMes([FromBody] FecharMesRequest request)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var command = new FecharMesCommand(usuarioId, request.AnoMes);
            var fechamento = await _mediator.Send(command);
            
            return CreatedAtAction(nameof(GetById), new { id = fechamento.Id }, fechamento);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reabrir meu mês (funcionalidade especial)
    /// </summary>
    [HttpPost("reabrir")]
    public async Task<ActionResult> ReabrirMes([FromBody] ReabrirMesRequest request)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var command = new ReabrirMesCommand(usuarioId, request.AnoMes);
            var success = await _mediator.Send(command);
            
            if (!success)
                return NotFound("Fechamento não encontrado");
                
            return Ok(new { message = $"Mês {request.AnoMes} reaberto com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class FecharMesRequest
{
    public string AnoMes { get; set; } = string.Empty;
}

public class ReabrirMesRequest
{
    public string AnoMes { get; set; } = string.Empty;
}
