using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendWise.Application.Commands.Metas;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Metas;
using System.Security.Claims;

namespace SpendWise.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetasController : ControllerBase
{
    private readonly IMediator _mediator;

    public MetasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetUsuarioId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("Usuário não autenticado"));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MetaResumoDto>>> GetMetas([FromQuery] bool apenasAtivas = true)
    {
        var usuarioId = GetUsuarioId();
        var query = new GetMetasByUsuarioQuery(usuarioId, apenasAtivas);
        var metas = await _mediator.Send(query);
        return Ok(metas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MetaDto>> GetMeta(Guid id)
    {
        var query = new GetMetaByIdQuery(id);
        var meta = await _mediator.Send(query);

        if (meta == null)
            return NotFound();

        // Verificar se a meta pertence ao usuário logado
        var usuarioId = GetUsuarioId();
        if (meta.UsuarioId != usuarioId)
            return Forbid();

        return Ok(meta);
    }

    [HttpGet("vencidas")]
    public async Task<ActionResult<IEnumerable<MetaResumoDto>>> GetMetasVencidas()
    {
        var usuarioId = GetUsuarioId();
        var query = new GetMetasVencidasQuery(usuarioId);
        var metas = await _mediator.Send(query);
        return Ok(metas);
    }

    [HttpGet("alcancadas")]
    public async Task<ActionResult<IEnumerable<MetaResumoDto>>> GetMetasAlcancadas()
    {
        var usuarioId = GetUsuarioId();
        var query = new GetMetasAlcancadasQuery(usuarioId);
        var metas = await _mediator.Send(query);
        return Ok(metas);
    }

    [HttpGet("estatisticas")]
    public async Task<ActionResult<EstatisticasMetasDto>> GetEstatisticasMetas()
    {
        var usuarioId = GetUsuarioId();
        var query = new GetEstatisticasMetasQuery(usuarioId);
        var estatisticas = await _mediator.Send(query);
        return Ok(estatisticas);
    }

    [HttpGet("resumo")]
    public async Task<ActionResult<IEnumerable<MetaResumoDto>>> GetMetasResumo([FromQuery] int limite = 5)
    {
        var usuarioId = GetUsuarioId();
        var query = new GetMetasResumoQuery(usuarioId, limite);
        var metas = await _mediator.Send(query);
        return Ok(metas);
    }

    [HttpPost]
    public async Task<ActionResult<MetaDto>> CreateMeta([FromBody] CreateMetaCommand command)
    {
        var usuarioId = GetUsuarioId();
        command = command with { UsuarioId = usuarioId };

        try
        {
            var meta = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetMeta), new { id = meta.Id }, meta);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MetaDto>> UpdateMeta(Guid id, [FromBody] UpdateMetaCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID da URL não coincide com o ID do comando");

        // Verificar se a meta existe e pertence ao usuário
        var metaExistente = await _mediator.Send(new GetMetaByIdQuery(id));
        if (metaExistente == null)
            return NotFound();

        var usuarioId = GetUsuarioId();
        if (metaExistente.UsuarioId != usuarioId)
            return Forbid();

        try
        {
            var meta = await _mediator.Send(command);
            return Ok(meta);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/progresso")]
    public async Task<ActionResult<MetaDto>> UpdateProgresso(Guid id, [FromBody] UpdateProgressoMetaCommand command)
    {
        if (id != command.MetaId)
            return BadRequest("ID da URL não coincide com o ID da meta");

        // Verificar se a meta existe e pertence ao usuário
        var metaExistente = await _mediator.Send(new GetMetaByIdQuery(id));
        if (metaExistente == null)
            return NotFound();

        var usuarioId = GetUsuarioId();
        if (metaExistente.UsuarioId != usuarioId)
            return Forbid();

        try
        {
            var meta = await _mediator.Send(command);
            return Ok(meta);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<MetaDto>> ToggleStatus(Guid id)
    {
        // Verificar se a meta existe e pertence ao usuário
        var metaExistente = await _mediator.Send(new GetMetaByIdQuery(id));
        if (metaExistente == null)
            return NotFound();

        var usuarioId = GetUsuarioId();
        if (metaExistente.UsuarioId != usuarioId)
            return Forbid();

        var command = new ToggleMetaStatusCommand(id);
        var meta = await _mediator.Send(command);
        return Ok(meta);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMeta(Guid id)
    {
        // Verificar se a meta existe e pertence ao usuário
        var metaExistente = await _mediator.Send(new GetMetaByIdQuery(id));
        if (metaExistente == null)
            return NotFound();

        var usuarioId = GetUsuarioId();
        if (metaExistente.UsuarioId != usuarioId)
            return Forbid();

        var command = new DeleteMetaCommand(id);
        var resultado = await _mediator.Send(command);

        if (!resultado)
            return NotFound();

        return NoContent();
    }
}
