using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendWise.Application.Commands.Categorias;
using SpendWise.Application.DTOs;
using SpendWise.Application.DTOs.Categorias;
using SpendWise.Application.Queries.Categorias;
using SpendWise.API.Extensions;
using FluentValidation;

namespace SpendWise.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll()
    {
        var usuarioId = User.GetUserId();
        var query = new GetCategoriasByUsuarioQuery(usuarioId);
        var categorias = await _mediator.Send(query);
        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDto>> GetById(Guid id)
    {
        var usuarioId = User.GetUserId();
        var query = new GetCategoriaByIdQuery(id);
        var categoria = await _mediator.Send(query);
        
        if (categoria == null || categoria.UsuarioId != usuarioId)
            return NotFound();
            
        return Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create([FromBody] CreateCategoriaCommand command)
    {
        var usuarioId = User.GetUserId();
        var commandWithUser = command with { UsuarioId = usuarioId };
        var categoria = await _mediator.Send(commandWithUser);
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoriaDto>> Update(Guid id, [FromBody] UpdateCategoriaCommand command)
    {
        var usuarioId = User.GetUserId();
        
        if (id != command.Id)
            return BadRequest("ID mismatch");

        // Verificar ownership antes de atualizar
        var existingQuery = new GetCategoriaByIdQuery(id);
        var existing = await _mediator.Send(existingQuery);
        
        if (existing == null || existing.UsuarioId != usuarioId)
            return NotFound();

        var categoria = await _mediator.Send(command);
        return Ok(categoria);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var usuarioId = User.GetUserId();
        
        // Verificar ownership antes de deletar
        var existingQuery = new GetCategoriaByIdQuery(id);
        var existing = await _mediator.Send(existingQuery);
        
        if (existing == null || existing.UsuarioId != usuarioId)
            return NotFound();
            
        var command = new DeleteCategoriaCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }

    /// <summary>
    /// Preview da exclusão de categoria com informações sobre despesas vinculadas
    /// </summary>
    [HttpGet("{id}/preview-exclusao")]
    public async Task<ActionResult<PreviewExclusaoCategoriaDto>> GetPreviewExclusao(Guid id)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var query = new GetPreviewExclusaoCategoriaQuery(usuarioId, id);
            var preview = await _mediator.Send(query);
            return Ok(preview);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Reatribui todas as despesas de uma categoria para outra
    /// </summary>
    [HttpPost("reatribuir-despesas")]
    public async Task<ActionResult<ReatribuirDespesasResponseDto>> ReatribuirDespesas([FromBody] ReatribuirDespesasRequestDto request)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var command = new ReatribuirDespesasCommand(usuarioId, request.CategoriaOrigemId, request.CategoriaDestinoId);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui categoria com opção de reatribuir despesas
    /// </summary>
    [HttpDelete("{id}/com-reatribuicao")]
    public async Task<ActionResult> DeleteComReatribuicao(Guid id, [FromQuery] Guid? categoriaDestinoId = null)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var command = new DeleteCategoriaWithReatribuicaoCommand(usuarioId, id, categoriaDestinoId);
            var result = await _mediator.Send(command);
            
            if (!result)
                return NotFound();
                
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Busca categorias com informações de progresso de limite
    /// </summary>
    [HttpGet("com-progresso")]
    public async Task<ActionResult<IEnumerable<CategoriaComProgressoDto>>> GetComProgresso([FromQuery] DateTime? data = null)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var query = new GetCategoriasComProgressoQuery(usuarioId, data);
            var categorias = await _mediator.Send(query);
            return Ok(categorias);
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}