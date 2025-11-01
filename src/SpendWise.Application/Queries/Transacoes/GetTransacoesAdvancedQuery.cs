using MediatR;
using SpendWise.Application.Common;
using SpendWise.Application.DTOs;
using SpendWise.Domain.Enums;

namespace SpendWise.Application.Queries.Transacoes;

public class GetTransacoesAdvancedQuery : PaginatedRequest, IRequest<PaginatedResponse<TransacaoDto>>
{
    public Guid UsuarioId { get; set; }
    
    // Filtros de data
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    
    // Filtros de valor
    public decimal? ValorMinimo { get; set; }
    public decimal? ValorMaximo { get; set; }
    
    // Filtros de categoria e tipo
    public Guid? CategoriaId { get; set; }
    public TipoTransacao? Tipo { get; set; }
    
    // Filtros de texto
    public string? Descricao { get; set; }
    public string? Observacoes { get; set; }
    
    // Ordenação
    public string? OrderBy { get; set; } = "DataTransacao"; // DataTransacao, Valor, Descricao
    public bool Ascending { get; set; } = false; // Padrão: mais recentes primeiro
}
