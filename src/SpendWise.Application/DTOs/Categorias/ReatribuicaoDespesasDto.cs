namespace SpendWise.Application.DTOs.Categorias;

public class ReatribuirDespesasRequestDto
{
    public Guid CategoriaOrigemId { get; set; }
    public Guid CategoriaDestinoId { get; set; }
}

public class ReatribuirDespesasResponseDto
{
    public int QuantidadeDespesasMovidas { get; set; }
    public string CategoriaOrigemNome { get; set; } = string.Empty;
    public string CategoriaDestinoNome { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PreviewExclusaoCategoriaDto
{
    public Guid CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public int QuantidadeDespesas { get; set; }
    public decimal TotalDespesas { get; set; }
    public List<TransacaoResumoDto> DespesasVinculadas { get; set; } = new();
    public List<CategoriaSimplificadaDto> CategoriasAlternativas { get; set; } = new();
}

public class TransacaoResumoDto
{
    public Guid Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataTransacao { get; set; }
}

public class CategoriaSimplificadaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}
