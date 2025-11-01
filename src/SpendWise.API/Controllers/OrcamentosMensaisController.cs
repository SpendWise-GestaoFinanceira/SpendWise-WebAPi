using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendWise.Application.Commands.OrcamentosMensais;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.OrcamentosMensais;
using SpendWise.Application.Services;
using SpendWise.Domain.ValueObjects;
using SpendWise.API.Extensions;

namespace SpendWise.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrcamentosMensaisController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrcamentosMensaisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Busca um orçamento mensal específico por ID
    /// </summary>
    /// <param name="id">ID do orçamento mensal</param>
    /// <returns>Dados do orçamento mensal</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrcamentoMensalDto>> GetById(Guid id)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var query = new GetOrcamentoMensalByIdQuery(id, usuarioId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Orçamento mensal com ID {id} não foi encontrado");
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Busca todos os orçamentos mensais do usuário autenticado
    /// </summary>
    /// <returns>Lista de orçamentos mensais</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrcamentoMensalDto>>> GetByUsuario()
    {
        try
        {
            var usuarioId = User.GetUserId();
            var query = new GetOrcamentosMensaisByUsuarioQuery(usuarioId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Busca orçamento mensal por ano e mês específicos
    /// </summary>
    /// <param name="anoMes">Período no formato YYYY-MM</param>
    /// <returns>Dados do orçamento mensal</returns>
    [HttpGet("periodo/{anoMes}")]
    public async Task<ActionResult<OrcamentoMensalDto>> GetByAnoMes(string anoMes)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var query = new GetOrcamentoMensalByUsuarioEAnoMesQuery(usuarioId, anoMes);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"Orçamento mensal para o período {anoMes} não foi encontrado");
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Cria um novo orçamento mensal
    /// </summary>
    /// <param name="dto">Dados para criação do orçamento</param>
    /// <returns>Orçamento mensal criado</returns>
    [HttpPost]
    public async Task<ActionResult<OrcamentoMensalDto>> Create([FromBody] CreateOrcamentoMensalDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuarioId = User.GetUserId();
            var valorMoney = new Money(dto.Valor, dto.Moeda);
            
            var command = new CreateOrcamentoMensalCommand(
                usuarioId,
                dto.AnoMes,
                valorMoney);

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Atualiza um orçamento mensal existente
    /// </summary>
    /// <param name="id">ID do orçamento mensal</param>
    /// <param name="dto">Dados para atualização</param>
    /// <returns>Orçamento mensal atualizado</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrcamentoMensalDto>> Update(Guid id, [FromBody] UpdateOrcamentoMensalDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuarioId = User.GetUserId();
            var valorMoney = new Money(dto.Valor, dto.Moeda);
            
            var command = new UpdateOrcamentoMensalCommand(
                id,
                valorMoney,
                usuarioId);

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove um orçamento mensal
    /// </summary>
    /// <param name="id">ID do orçamento mensal</param>
    /// <returns>Confirmação de remoção</returns>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var command = new DeleteOrcamentoMensalCommand(id, usuarioId);
            
            var result = await _mediator.Send(command);
            
            if (result)
            {
                return NoContent();
            }
            
            return NotFound($"Orçamento mensal com ID {id} não foi encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Obter estatísticas detalhadas de uso do orçamento para um período específico
    /// </summary>
    /// <param name="anoMes">Período no formato YYYY-MM</param>
    /// <returns>Estatísticas de uso do orçamento</returns>
    [HttpGet("estatisticas/{anoMes}")]
    public async Task<ActionResult<Application.DTOs.OrcamentoCalculoResultado>> GetEstatisticas(string anoMes)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var query = new GetEstatisticasOrcamentoQuery(usuarioId, anoMes);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }

    /// <summary>
    /// Obter estatísticas detalhadas por categoria para alertas de limite
    /// </summary>
    /// <param name="anoMes">Período no formato YYYY-MM</param>
    /// <returns>Estatísticas de uso por categoria</returns>
    [HttpGet("estatisticas/categorias/{anoMes}")]
    public async Task<ActionResult<Application.DTOs.EstatisticasCategoriasDto>> GetEstatisticasCategorias(string anoMes)
    {
        try
        {
            var usuarioId = User.GetUserId();
            var query = new GetEstatisticasCategoriasQuery(usuarioId, anoMes);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
        }
    }
}
