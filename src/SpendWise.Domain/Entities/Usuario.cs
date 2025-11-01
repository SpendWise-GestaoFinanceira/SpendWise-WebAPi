using SpendWise.Domain.ValueObjects;

namespace SpendWise.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; private set; }
    public Email Email { get; private set; }
    public string Senha { get; private set; }
    public decimal RendaMensal { get; private set; }
    public bool IsAtivo { get; private set; } = true;
    
    // Propriedades para reset de senha
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    // Relacionamentos
    public virtual ICollection<Categoria> Categorias { get; private set; } = new List<Categoria>();
    public virtual ICollection<Transacao> Transacoes { get; private set; } = new List<Transacao>();
    public virtual ICollection<Meta> Metas { get; private set; } = new List<Meta>();

    // Construtor privado para EF Core
    private Usuario() 
    { 
        Nome = string.Empty;
        Email = new Email("default@temp.com");
        Senha = string.Empty;
    }

    public Usuario(string nome, Email email, string passwordHash, decimal rendaMensal)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));
        
        if (rendaMensal < 0)
            throw new ArgumentException("Renda mensal não pode ser negativa", nameof(rendaMensal));

        Nome = nome;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Senha = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        RendaMensal = rendaMensal;
    }

    public Usuario(string nome, Email email, string passwordHash) : this(nome, email, passwordHash, 0)
    {
        // Construtor para registro simples sem renda mensal
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));
        
        Nome = nome;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarRendaMensal(decimal rendaMensal)
    {
        if (rendaMensal < 0)
            throw new ArgumentException("Renda mensal não pode ser negativa", nameof(rendaMensal));
        
        RendaMensal = rendaMensal;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Desativar()
    {
        IsAtivo = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ativar()
    {
        IsAtivo = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DefinirTokenResetSenha(string token, TimeSpan validPeriod)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token não pode ser vazio", nameof(token));

        PasswordResetToken = token;
        PasswordResetTokenExpiry = DateTime.UtcNow.Add(validPeriod);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsTokenResetSenhaValido(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(PasswordResetToken))
            return false;

        if (PasswordResetTokenExpiry == null || PasswordResetTokenExpiry < DateTime.UtcNow)
            return false;

        return PasswordResetToken == token;
    }

    public void ResetarSenha(string novaSenhaHash)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaHash))
            throw new ArgumentException("Hash da senha não pode ser vazio", nameof(novaSenhaHash));

        Senha = novaSenhaHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
