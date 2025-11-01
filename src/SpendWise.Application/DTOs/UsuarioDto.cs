namespace SpendWise.Application.DTOs;

public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal RendaMensal { get; set; }
    public bool IsAtivo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUsuarioDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public decimal RendaMensal { get; set; }
}

public class UpdateUsuarioDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal RendaMensal { get; set; }
}
