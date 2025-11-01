using MediatR;
using SpendWise.Application.DTOs.Transacoes;
using SpendWise.Application.DTOs;
using SpendWise.Application.Queries.Transacoes;
using SpendWise.Domain.Interfaces;
using AutoMapper;

namespace SpendWise.Application.Handlers.Transacoes;

public class ListarTransacoesFiltrada : IRequestHandler<ListarTransacoesFiltradas, TransacoesPaginadasDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListarTransacoesFiltrada(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TransacoesPaginadasDto> Handle(ListarTransacoesFiltradas request, CancellationToken cancellationToken)
    {
        var filtros = request.Filtros;
        var paginacao = request.Paginacao;

        // Validar tamanho da página
        if (paginacao.TamanhoPagina > 100)
            paginacao.TamanhoPagina = 100;

        if (paginacao.TamanhoPagina <= 0)
            paginacao.TamanhoPagina = 10;

        if (paginacao.Pagina <= 0)
            paginacao.Pagina = 1;

        // Buscar transações com filtros
        var query = await _unitOfWork.Transacoes.GetAllAsync();
        
        // Filtrar por usuário
        var transacoesFiltradas = query.Where(t => t.UsuarioId == request.UsuarioId);

        // Aplicar filtros
        if (filtros.CategoriaId.HasValue)
        {
            transacoesFiltradas = transacoesFiltradas.Where(t => t.CategoriaId == filtros.CategoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Tipo))
        {
            transacoesFiltradas = transacoesFiltradas.Where(t => t.Tipo.ToString() == filtros.Tipo);
        }

        if (filtros.DataInicio.HasValue)
        {
            transacoesFiltradas = transacoesFiltradas.Where(t => t.DataTransacao >= filtros.DataInicio.Value);
        }

        if (filtros.DataFim.HasValue)
        {
            transacoesFiltradas = transacoesFiltradas.Where(t => t.DataTransacao <= filtros.DataFim.Value);
        }

        if (filtros.ValorMinimo.HasValue)
        {
            transacoesFiltradas = transacoesFiltradas.Where(t => t.Valor.Valor >= filtros.ValorMinimo.Value);
        }

        if (filtros.ValorMaximo.HasValue)
        {
            transacoesFiltradas = transacoesFiltradas.Where(t => t.Valor.Valor <= filtros.ValorMaximo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtros.BuscaTextual))
        {
            var termoBusca = filtros.BuscaTextual.ToLower();
            transacoesFiltradas = transacoesFiltradas.Where(t => 
                t.Descricao.ToLower().Contains(termoBusca) ||
                (t.Observacoes != null && t.Observacoes.ToLower().Contains(termoBusca))
            );
        }

        // Contagem total antes da paginação
        var totalItens = transacoesFiltradas.Count();
        var totalPaginas = (int)Math.Ceiling((double)totalItens / paginacao.TamanhoPagina);

        // Ordenação
        switch (paginacao.OrdenarPor?.ToLower())
        {
            case "valor":
                transacoesFiltradas = paginacao.Direcao?.ToLower() == "asc" 
                    ? transacoesFiltradas.OrderBy(t => t.Valor.Valor)
                    : transacoesFiltradas.OrderByDescending(t => t.Valor.Valor);
                break;
            case "descricao":
                transacoesFiltradas = paginacao.Direcao?.ToLower() == "asc" 
                    ? transacoesFiltradas.OrderBy(t => t.Descricao)
                    : transacoesFiltradas.OrderByDescending(t => t.Descricao);
                break;
            case "categoria":
                transacoesFiltradas = paginacao.Direcao?.ToLower() == "asc" 
                    ? transacoesFiltradas.OrderBy(t => t.Categoria != null ? t.Categoria.Nome : "")
                    : transacoesFiltradas.OrderByDescending(t => t.Categoria != null ? t.Categoria.Nome : "");
                break;
            default: // "data"
                transacoesFiltradas = paginacao.Direcao?.ToLower() == "asc" 
                    ? transacoesFiltradas.OrderBy(t => t.DataTransacao)
                    : transacoesFiltradas.OrderByDescending(t => t.DataTransacao);
                break;
        }

        // Paginação
        var transacoesPaginadas = transacoesFiltradas
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToList();

        // Mapear para DTOs
        var transacoesDto = _mapper.Map<List<TransacaoDto>>(transacoesPaginadas);

        return new TransacoesPaginadasDto
        {
            Transacoes = transacoesDto,
            TotalItens = totalItens,
            TotalPaginas = totalPaginas,
            PaginaAtual = paginacao.Pagina,
            TamanhoPagina = paginacao.TamanhoPagina,
            TemProximaPagina = paginacao.Pagina < totalPaginas,
            TemPaginaAnterior = paginacao.Pagina > 1
        };
    }
}
