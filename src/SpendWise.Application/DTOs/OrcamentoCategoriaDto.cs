namespace SpendWise.Application.DTOs;

public class OrcamentoCategoriaDto
{
    public Guid CategoriaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Limite { get; set; }
    public decimal Gasto { get; set; }
    public decimal PercentualUtilizado { get; set; }
    public StatusOrcamento Status { get; set; }
}

public class EstatisticasCategoriasDto
{
    public string AnoMes { get; set; } = string.Empty;
    public List<OrcamentoCategoriaDto> Categorias { get; set; } = new();
}
