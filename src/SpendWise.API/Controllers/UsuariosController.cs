using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendWise.Application.DTOs;
using SpendWise.Application.Commands.Usuario;
using SpendWise.Application.Queries.Usuario;
using SpendWise.API.Extensions;
using MediatR;

namespace SpendWise.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsuariosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obter perfil do usuário logado
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioDto>> GetProfile()
    {
        var usuarioId = User.GetUserId();
        var query = new GetUsuarioByIdQuery(usuarioId);
        var usuario = await _mediator.Send(query);
        
        if (usuario == null)
            return NotFound();
            
        return Ok(usuario);
    }

    /// <summary>
    /// Obter usuário por ID (apenas o próprio)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetById(Guid id)
    {
        var usuarioId = User.GetUserId();
        
        // Só permite acessar o próprio perfil
        if (id != usuarioId)
            return Forbid();
            
        var query = new GetUsuarioByIdQuery(id);
        var usuario = await _mediator.Send(query);
        
        if (usuario == null)
            return NotFound();
            
        return Ok(usuario);
    }

    /// <summary>
    /// Criar novo usuário (registro)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UsuarioDto>> Create([FromBody] CreateUsuarioDto createDto)
    {
        var command = new CreateUsuarioCommand(
            createDto.Nome,
            createDto.Email,
            createDto.Password,
            createDto.RendaMensal
        );

        var usuario = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
    }

    /// <summary>
    /// Atualizar perfil próprio
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UsuarioDto>> Update(Guid id, [FromBody] UpdateUsuarioDto updateDto)
    {
        var usuarioId = User.GetUserId();
        
        // Só permite atualizar o próprio perfil
        if (id != usuarioId)
            return Forbid();
            
        var command = new UpdateUsuarioCommand(
            id,
            updateDto.Nome,
            updateDto.RendaMensal
        );

        var usuario = await _mediator.Send(command);
        
        if (usuario == null)
            return NotFound();
            
        return Ok(usuario);
    }

    /// <summary>
    /// Deletar conta própria
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var usuarioId = User.GetUserId();
        
        // Só permite deletar a própria conta
        if (id != usuarioId)
            return Forbid();
            
        var command = new DeleteUsuarioCommand(id);
        var success = await _mediator.Send(command);
        
        if (!success)
            return NotFound();
            
        return NoContent();
    }
}
