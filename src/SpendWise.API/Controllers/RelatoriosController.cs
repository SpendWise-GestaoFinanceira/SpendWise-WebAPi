using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendWise.Application.Queries.Relatorios;
using SpendWise.Application.DTOs.Relatorios;
using SpendWise.API.Extensions;
using FluentValidation;
using System.Security.Claims;

namespace SpendWise.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<ComparativoMesesRequestDto> _comparativoValidator;
    private readonly ILogger<RelatoriosController> _logger;

    public RelatoriosController(
        IMediator mediator,
        IValidator<ComparativoMesesRequestDto> comparativoValidator,
        ILogger<RelatoriosController> logger)
    {
        _mediator = mediator;
        _comparativoValidator = comparativoValidator;
        _logger = logger;
    }

    /// <summary>
    /// Relatório resumo mensal
    /// </summary>
    [HttpGet("resumo-mensal/{anoMes}")]
    public async Task<ActionResult<ResumoMensalDto>> GetResumoMensal(string anoMes)
    {
        var usuarioId = User.GetUserId();
        var query = new GetResumoMensalQuery(usuarioId, anoMes);
        var resumo = await _mediator.Send(query);
        return Ok(resumo);
    }

    /// <summary>
    /// Relatório por categoria (período)
    /// </summary>
    [HttpGet("categorias")]
    public async Task<ActionResult<IEnumerable<CategoriaSummaryDto>>> GetRelatorioCategorias(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim)
    {
        var usuarioId = User.GetUserId();
        var query = new GetRelatorioCategoriasQuery(usuarioId, dataInicio, dataFim);
        var relatorio = await _mediator.Send(query);
        return Ok(relatorio);
    }

    /// <summary>
    /// Evolução de gastos (últimos 12 meses)
    /// </summary>
    [HttpGet("evolucao-gastos")]
    public async Task<ActionResult<EvolucaoGastosDto>> GetEvolucaoGastos()
    {
        var usuarioId = User.GetUserId();
        var query = new GetEvolucaoGastosQuery(usuarioId);
        var evolucao = await _mediator.Send(query);
        return Ok(evolucao);
    }

    /// <summary>
    /// Top categorias com maior gasto
    /// </summary>
    [HttpGet("top-categorias")]
    public async Task<ActionResult<IEnumerable<TopCategoriaDto>>> GetTopCategorias(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] int top = 5)
    {
        var usuarioId = User.GetUserId();
        var query = new GetTopCategoriasQuery(usuarioId, dataInicio, dataFim, top);
        var topCategorias = await _mediator.Send(query);
        return Ok(topCategorias);
    }

    /// <summary>
    /// Comparativo orçado vs realizado
    /// </summary>
    [HttpGet("orcado-vs-realizado/{anoMes}")]
    public async Task<ActionResult<OrcadoVsRealizadoDto>> GetOrcadoVsRealizado(string anoMes)
    {
        var usuarioId = User.GetUserId();
        var query = new GetOrcadoVsRealizadoQuery(usuarioId, anoMes);
        var comparativo = await _mediator.Send(query);
        return Ok(comparativo);
    }

    /// <summary>
    /// Gera um comparativo de receitas, despesas e saldo entre diferentes meses
    /// </summary>
    /// <param name="request">Parâmetros do comparativo</param>
    /// <returns>Relatório comparativo com estatísticas e tendências</returns>
    [HttpPost("comparativo-meses")]
    [ProducesResponseType(typeof(ComparativoMesesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ComparativoMesesResponseDto>> ComparativoMeses(
        [FromBody] ComparativoMesesRequestDto request)
    {
        try
        {
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var validationResult = await _comparativoValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var query = new ComparativoMesesQuery(usuarioId, request);
            var response = await _mediator.Send(query);

            _logger.LogInformation("Comparativo de meses gerado para usuário {UsuarioId}", usuarioId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar comparativo de meses");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Gera um comparativo simples entre dois meses específicos
    /// </summary>
    /// <param name="anoMes1">Primeiro mês no formato YYYY-MM</param>
    /// <param name="anoMes2">Segundo mês no formato YYYY-MM</param>
    /// <returns>Comparativo entre os dois meses</returns>
    [HttpGet("comparativo-dois-meses")]
    [ProducesResponseType(typeof(ComparativoMesesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ComparativoMesesResponseDto>> ComparativoDoisMeses(
        [FromQuery] string anoMes1,
        [FromQuery] string anoMes2)
    {
        try
        {
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Validar formato dos parâmetros
            if (!ValidarFormatoAnoMes(anoMes1) || !ValidarFormatoAnoMes(anoMes2))
            {
                return BadRequest(new { message = "Formato de ano/mês inválido. Use YYYY-MM" });
            }

            var (ano1, mes1) = ParseAnoMes(anoMes1);
            var (ano2, mes2) = ParseAnoMes(anoMes2);

            var request = new ComparativoMesesRequestDto
            {
                AnoInicio = Math.Min(ano1, ano2),
                MesInicio = ano1 < ano2 ? mes1 : (ano1 == ano2 ? Math.Min(mes1, mes2) : mes1),
                AnoFim = Math.Max(ano1, ano2),
                MesFim = ano1 > ano2 ? mes1 : (ano1 == ano2 ? Math.Max(mes1, mes2) : mes2),
                IncluirDetalhes = true
            };

            var query = new ComparativoMesesQuery(usuarioId, request);
            var response = await _mediator.Send(query);

            _logger.LogInformation("Comparativo entre dois meses gerado para usuário {UsuarioId}: {Mes1} vs {Mes2}",
                usuarioId, anoMes1, anoMes2);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar comparativo entre dois meses");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Gera um comparativo do ano atual com o ano anterior
    /// </summary>
    /// <returns>Comparativo anual</returns>
    [HttpGet("comparativo-ano-atual")]
    [ProducesResponseType(typeof(ComparativoMesesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ComparativoMesesResponseDto>> ComparativoAnoAtual()
    {
        try
        {
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var anoAtual = DateTime.Now.Year;
            var mesAtual = DateTime.Now.Month;

            var request = new ComparativoMesesRequestDto
            {
                AnoInicio = anoAtual - 1,
                MesInicio = 1,
                AnoFim = anoAtual,
                MesFim = mesAtual,
                IncluirDetalhes = false
            };

            var query = new ComparativoMesesQuery(usuarioId, request);
            var response = await _mediator.Send(query);

            _logger.LogInformation("Comparativo do ano atual gerado para usuário {UsuarioId}", usuarioId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar comparativo do ano atual");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    private bool ValidarFormatoAnoMes(string anoMes)
    {
        if (string.IsNullOrWhiteSpace(anoMes))
            return false;

        var partes = anoMes.Split('-');
        if (partes.Length != 2)
            return false;

        return int.TryParse(partes[0], out var ano) &&
               int.TryParse(partes[1], out var mes) &&
               ano >= 2000 && ano <= DateTime.Now.Year + 1 &&
               mes >= 1 && mes <= 12;
    }

    private (int ano, int mes) ParseAnoMes(string anoMes)
    {
        var partes = anoMes.Split('-');
        return (int.Parse(partes[0]), int.Parse(partes[1]));
    }
}
