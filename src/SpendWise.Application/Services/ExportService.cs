using SpendWise.Application.DTOs.Transacoes;
using System.Text;
using System.Globalization;

namespace SpendWise.Application.Services;

public interface IExportService
{
    Task<byte[]> ExportToCsvAsync(IEnumerable<TransacaoExportDto> transacoes, bool incluirCabecalho = true);
    Task<byte[]> ExportToJsonAsync(IEnumerable<TransacaoExportDto> transacoes);
    string GerarNomeArquivo(string formato, DateTime? dataInicio = null, DateTime? dataFim = null);
    string GerarDescricaoFiltros(ExportTransacoesRequestDto request);
}

public class ExportService : IExportService
{
    public Task<byte[]> ExportToCsvAsync(IEnumerable<TransacaoExportDto> transacoes, bool incluirCabecalho = true)
    {
        var csv = new StringBuilder();
        
        // Cabeçalho
        if (incluirCabecalho)
        {
            csv.AppendLine("Data,Descrição,Valor,Tipo,Categoria,Observações,Moeda");
        }
        
        // Dados
        foreach (var transacao in transacoes)
        {
            var linha = string.Join(",", 
                transacao.Data.ToString("dd/MM/yyyy"),
                EscaparCampoCSV(transacao.Descricao),
                transacao.Valor.ToString("F2", CultureInfo.InvariantCulture),
                EscaparCampoCSV(transacao.Tipo),
                EscaparCampoCSV(transacao.Categoria),
                EscaparCampoCSV(transacao.Observacoes ?? ""),
                transacao.Moeda
            );
            csv.AppendLine(linha);
        }
        
        return Task.FromResult(Encoding.UTF8.GetBytes(csv.ToString()));
    }
    
    public Task<byte[]> ExportToJsonAsync(IEnumerable<TransacaoExportDto> transacoes)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(transacoes, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        
        return Task.FromResult(Encoding.UTF8.GetBytes(json));
    }
    
    public string GerarNomeArquivo(string formato, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var periodo = "";
        
        if (dataInicio.HasValue && dataFim.HasValue)
        {
            periodo = $"_{dataInicio.Value:yyyyMMdd}_to_{dataFim.Value:yyyyMMdd}";
        }
        else if (dataInicio.HasValue)
        {
            periodo = $"_from_{dataInicio.Value:yyyyMMdd}";
        }
        else if (dataFim.HasValue)
        {
            periodo = $"_until_{dataFim.Value:yyyyMMdd}";
        }
        
        var extensao = formato.ToLower() == "json" ? "json" : "csv";
        return $"spendwise_transacoes{periodo}_{timestamp}.{extensao}";
    }
    
    public string GerarDescricaoFiltros(ExportTransacoesRequestDto request)
    {
        var filtros = new List<string>();
        
        if (request.DataInicio.HasValue)
            filtros.Add($"Data início: {request.DataInicio.Value:dd/MM/yyyy}");
            
        if (request.DataFim.HasValue)
            filtros.Add($"Data fim: {request.DataFim.Value:dd/MM/yyyy}");
            
        if (request.CategoriaId.HasValue)
            filtros.Add($"Categoria ID: {request.CategoriaId.Value}");
            
        if (!string.IsNullOrEmpty(request.Tipo))
            filtros.Add($"Tipo: {request.Tipo}");
            
        filtros.Add($"Ordenação: {request.OrdenarPor} ({(request.Crescente ? "Crescente" : "Decrescente")})");
        
        return filtros.Any() ? string.Join("; ", filtros) : "Sem filtros aplicados";
    }
    
    private string EscaparCampoCSV(string campo)
    {
        if (string.IsNullOrEmpty(campo))
            return "";
            
        // Se contém vírgula, aspas ou quebra de linha, precisa escapar
        if (campo.Contains(',') || campo.Contains('"') || campo.Contains('\n') || campo.Contains('\r'))
        {
            // Duplicar aspas internas e envolver em aspas
            return $"\"{campo.Replace("\"", "\"\"")}\"";
        }
        
        return campo;
    }
}
