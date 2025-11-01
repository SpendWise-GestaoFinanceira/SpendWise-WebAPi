using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendWise.Application.Commands.Transacoes;
using SpendWise.Application.DTOs;
using SpendWise.Application.DTOs.Transacoes;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Application.Common;
using SpendWise.API.Extensions;

namespace SpendWise.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransacoesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransacoesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransacaoDto>>> GetAll()
    {
        var usuarioId = User.GetUserId();
        var query = new GetTransacoesByUsuarioQuery(usuarioId);
        var transacoes = await _mediator.Send(query);
        return Ok(transacoes);
    }

    /// <summary>
    /// Buscar transações com filtros avançados e paginação
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResponse<TransacaoDto>>> SearchAdvanced(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] decimal? valorMinimo,
        [FromQuery] decimal? valorMaximo,
        [FromQuery] Guid? categoriaId,
        [FromQuery] string? tipo,
        [FromQuery] string? descricao,
        [FromQuery] string? observacoes,
        [FromQuery] string? orderBy = "DataTransacao",
        [FromQuery] bool ascending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var usuarioId = User.GetUserId();
        
        // Parse tipo enum
        Domain.Enums.TipoTransacao? tipoEnum = null;
        if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<Domain.Enums.TipoTransacao>(tipo, true, out var parsedTipo))
        {
            tipoEnum = parsedTipo;
        }

        var query = new GetTransacoesAdvancedQuery
        {
            UsuarioId = usuarioId,
            DataInicio = dataInicio,
            DataFim = dataFim,
            ValorMinimo = valorMinimo,
            ValorMaximo = valorMaximo,
            CategoriaId = categoriaId,
            Tipo = tipoEnum,
            Descricao = descricao,
            Observacoes = observacoes,
            OrderBy = orderBy,
            Ascending = ascending,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransacaoDto>> GetById(Guid id)
    {
        var usuarioId = User.GetUserId();
        var query = new GetTransacaoByIdQuery(id);
        var transacao = await _mediator.Send(query);

        if (transacao == null || transacao.UsuarioId != usuarioId)
            return NotFound();

        return Ok(transacao);
    }

    [HttpGet("categoria/{categoriaId}")]
    public async Task<ActionResult<IEnumerable<TransacaoDto>>> GetByCategoria(Guid categoriaId)
    {
        var usuarioId = User.GetUserId();
        var query = new GetTransacoesByCategoriaQuery(categoriaId);
        var transacoes = await _mediator.Send(query);
        
        // Filtrar apenas transações do usuário logado
        var transacoesUsuario = transacoes.Where(t => t.UsuarioId == usuarioId);
        return Ok(transacoesUsuario);
    }

    [HttpPost]
    public async Task<ActionResult<TransacaoDto>> Create([FromBody] CreateTransacaoCommand command)
    {
        var usuarioId = User.GetUserId();
        var commandWithUser = command with { UsuarioId = usuarioId };
        var transacao = await _mediator.Send(commandWithUser);
        return CreatedAtAction(nameof(GetById), new { id = transacao.Id }, transacao);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TransacaoDto>> Update(Guid id, [FromBody] UpdateTransacaoCommand command)
    {
        var usuarioId = User.GetUserId();
        
        if (id != command.Id)
            return BadRequest("ID mismatch");

        // Verificar ownership antes de atualizar
        var existingQuery = new GetTransacaoByIdQuery(id);
        var existing = await _mediator.Send(existingQuery);
        
        if (existing == null || existing.UsuarioId != usuarioId)
            return NotFound();

        var transacao = await _mediator.Send(command);
        return Ok(transacao);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var usuarioId = User.GetUserId();
        
        // Verificar ownership antes de deletar
        var existingQuery = new GetTransacaoByIdQuery(id);
        var existing = await _mediator.Send(existingQuery);
        
        if (existing == null || existing.UsuarioId != usuarioId)
            return NotFound();
            
        var command = new DeleteTransacaoCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }
    
    [HttpGet("periodo")]
    public async Task<ActionResult<IEnumerable<TransacaoDto>>> GetByPeriodo(
        [FromQuery] DateTime dataInicio, 
        [FromQuery] DateTime dataFim,
        [FromQuery] Guid? usuarioId = null)
    {
        var query = new GetTransacoesByPeriodoQuery(dataInicio, dataFim, usuarioId);
        var transacoes = await _mediator.Send(query);
        return Ok(transacoes);
    }

    /// <summary>
    /// Upload e pré-visualização de arquivo CSV para importação de transações
    /// </summary>
    [HttpPost("import/csv/preview")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PreVisualizacaoImportacaoDto>> PreviewImportacaoCsv(IFormFile arquivo)
    {
        var usuarioId = User.GetUserId();
        
        // Validações básicas
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest("Arquivo é obrigatório");
            
        if (!arquivo.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Arquivo deve ter extensão .csv");
            
        if (arquivo.Length > 5 * 1024 * 1024) // 5MB
            return BadRequest("Arquivo não pode ser maior que 5MB");
        
        using var stream = arquivo.OpenReadStream();
        var command = new ProcessarArquivoCsvCommand(usuarioId, stream, arquivo.FileName);
        var preview = await _mediator.Send(command);
        
        // Armazenar preview em cache para confirmação posterior (simulação)
        var idImportacao = Guid.NewGuid().ToString();
        // TODO: Refatorar para usar cache service apropriado via MediatR
        // ConfirmarImportacaoCsvHandler.ArmazenarImportacaoEmCache(idImportacao, preview.Importacao);
        
        // Adicionar ID da importação ao resultado
        preview.Importacao.StatusProcessamento = $"Pronto para importação (ID: {idImportacao})";
        
        return Ok(preview);
    }

    /// <summary>
    /// Confirmar e executar importação de transações do CSV
    /// </summary>
    [HttpPost("import/csv/confirm")]
    public async Task<ActionResult<ResultadoImportacaoDto>> ConfirmarImportacaoCsv([FromBody] ConfirmacaoImportacaoDto confirmacao)
    {
        var usuarioId = User.GetUserId();
        var command = new ConfirmarImportacaoCsvCommand(usuarioId, confirmacao);
        var resultado = await _mediator.Send(command);
        
        if (!resultado.Sucesso)
            return BadRequest(resultado);
            
        return Ok(resultado);
    }

    /// <summary>
    /// Exportar transações para CSV ou JSON
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportarTransacoes(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] Guid? categoriaId,
        [FromQuery] string? tipo,
        [FromQuery] string formato = "CSV",
        [FromQuery] bool incluirCabecalho = true,
        [FromQuery] string? ordenarPor = "DataTransacao",
        [FromQuery] bool crescente = false)
    {
        var usuarioId = User.GetUserId();
        
        var request = new ExportTransacoesRequestDto
        {
            DataInicio = dataInicio,
            DataFim = dataFim,
            CategoriaId = categoriaId,
            Tipo = tipo,
            Formato = formato,
            IncluirCabecalho = incluirCabecalho,
            OrdenarPor = ordenarPor,
            Crescente = crescente
        };

        var query = new ExportTransacoesQuery(usuarioId, request);
        var resultado = await _mediator.Send(query);

        return File(
            resultado.ConteudoArquivo,
            resultado.ContentType,
            resultado.NomeArquivo);
    }
}