namespace SpendWise.Domain.Enums;

public enum StatusLimite
{
    SemLimite = 0,
    Normal = 1,
    Alerta = 2,      // >= 80%
    Excedido = 3     // >= 100%
}
