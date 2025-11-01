namespace SpendWise.Domain.Exceptions;

public class MesFechadoException : Exception
{
    public MesFechadoException(string anoMes) 
        : base($"Não é possível realizar operações no mês {anoMes} pois ele está fechado.")
    {
    }

    public MesFechadoException(string anoMes, string operacao) 
        : base($"Não é possível {operacao} no mês {anoMes} pois ele está fechado.")
    {
    }
}
