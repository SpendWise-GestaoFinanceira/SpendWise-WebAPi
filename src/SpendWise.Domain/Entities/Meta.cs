using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Entities;

public class Meta : BaseEntity
{
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public Money ValorObjetivo { get; private set; }
    public DateTime Prazo { get; private set; }
    public Money ValorAtual { get; private set; }
    public Guid UsuarioId { get; private set; }
    public bool IsAtiva { get; private set; } = true;
    public DateTime? DataAlcancada { get; private set; }

    // Relacionamentos
    public virtual Usuario Usuario { get; private set; } = null!;

    // Construtor privado para EF Core
    private Meta() 
    { 
        Nome = string.Empty;
        Descricao = string.Empty;
        ValorObjetivo = new Money(0);
        ValorAtual = new Money(0);
    }

    public Meta(string nome, string descricao, Money valorObjetivo, DateTime prazo, Guid usuarioId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da meta não pode ser vazio", nameof(nome));
        
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição da meta não pode ser vazia", nameof(descricao));

        if (valorObjetivo.Valor <= 0)
            throw new ArgumentException("Valor objetivo deve ser maior que zero", nameof(valorObjetivo));

        if (prazo <= DateTime.UtcNow.Date)
            throw new ArgumentException("Prazo deve ser no futuro", nameof(prazo));

        if (usuarioId == Guid.Empty)
            throw new ArgumentException("ID do usuário é obrigatório", nameof(usuarioId));

        Nome = nome;
        Descricao = descricao;
        ValorObjetivo = valorObjetivo;
        Prazo = prazo;
        UsuarioId = usuarioId;
        ValorAtual = new Money(0, valorObjetivo.Moeda);
    }

    // Métodos de negócio
    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da meta não pode ser vazio", nameof(nome));
        
        Nome = nome;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição da meta não pode ser vazia", nameof(descricao));
        
        Descricao = descricao;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarValorObjetivo(Money novoValorObjetivo)
    {
        if (novoValorObjetivo.Valor <= 0)
            throw new ArgumentException("Valor objetivo deve ser maior que zero", nameof(novoValorObjetivo));
        
        ValorObjetivo = novoValorObjetivo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarPrazo(DateTime novoPrazo)
    {
        if (novoPrazo <= DateTime.UtcNow.Date)
            throw new ArgumentException("Prazo deve ser no futuro", nameof(novoPrazo));
        
        Prazo = novoPrazo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdicionarProgresso(Money valorProgresso)
    {
        if (valorProgresso.Valor <= 0)
            throw new ArgumentException("Valor de progresso deve ser positivo", nameof(valorProgresso));
        
        ValorAtual = ValorAtual.Add(valorProgresso);
        
        // Verificar se a meta foi alcançada
        if (ValorAtual.Valor >= ValorObjetivo.Valor && DataAlcancada is null)
        {
            DataAlcancada = DateTime.UtcNow;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoverProgresso(Money valorProgresso)
    {
        if (valorProgresso.Valor <= 0)
            throw new ArgumentException("Valor de progresso deve ser positivo", nameof(valorProgresso));
        
        ValorAtual = ValorAtual.Subtract(valorProgresso);
        
        // Reset data alcançada se valor diminuiu abaixo do objetivo
        if (ValorAtual.Valor < ValorObjetivo.Valor)
        {
            DataAlcancada = null;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void Desativar()
    {
        IsAtiva = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reativar()
    {
        IsAtiva = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // Métodos de cálculo
    public double CalcularPercentualProgresso()
    {
        if (ValorObjetivo.Valor == 0) return 0;
        return Math.Min((double)(ValorAtual.Valor / ValorObjetivo.Valor) * 100, 100);
    }

    public Money CalcularValorRestante()
    {
        var restante = ValorObjetivo.Valor - ValorAtual.Valor;
        return new Money(Math.Max(restante, 0), ValorObjetivo.Moeda);
    }

    public int CalcularDiasRestantes()
    {
        var diasRestantes = (Prazo - DateTime.UtcNow.Date).Days;
        return Math.Max(diasRestantes, 0);
    }

    public DateTime? ProjetarDataAlcance(decimal mediaEconomiaMensal)
    {
        if (mediaEconomiaMensal <= 0) return null;
        
        var valorRestante = CalcularValorRestante().Valor;
        if (valorRestante <= 0) return DataAlcancada;
        
        var mesesNecessarios = (double)(valorRestante / mediaEconomiaMensal);
        return DateTime.UtcNow.AddMonths((int)Math.Ceiling(mesesNecessarios));
    }

    public string ObterStatusDescricao()
    {
        if (!IsAtiva) return "Inativa";
        if (DataAlcancada.HasValue) return "Alcançada";
        if (CalcularDiasRestantes() == 0) return "Vencida";
        if (CalcularDiasRestantes() <= 7) return "Próxima do vencimento";
        return "Em progresso";
    }
}
