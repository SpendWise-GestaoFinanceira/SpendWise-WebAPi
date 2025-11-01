namespace SpendWise.Application.DTOs.Transacoes;

public class FiltroTransacoesDto
{
    public Guid? CategoriaId { get; set; }
    public string? Tipo { get; set; } // "Receita" ou "Despesa"
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? BuscaTextual { get; set; }
    public decimal? ValorMinimo { get; set; }
    public decimal? ValorMaximo { get; set; }
}

public class PaginacaoDto
{
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
    public string? OrdenarPor { get; set; } = "Data";
    public string? Direcao { get; set; } = "desc"; // "asc" ou "desc"
}

public class TransacoesPaginadasDto
{
    public List<TransacaoDto> Transacoes { get; set; } = new();
    public int TotalItens { get; set; }
    public int TotalPaginas { get; set; }
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public bool TemProximaPagina { get; set; }
    public bool TemPaginaAnterior { get; set; }
}
