# Correcoes do Pipeline CI

## Problemas Identificados

### 1. Teste Falhando - Money.ToString()

**Erro:**
```
SpendWise.Domain.Tests.ValueObjects.MoneyTests.Money_ToString_ShouldFormatCorrectly
```

**Causa:**
- `Money.ToString()` usava `Valor:C` que e culture-specific
- No GitHub Actions (en-US): produzia "1,234.56"
- Teste esperava formato brasileiro: "1.234,56"

**Solucao:**
```csharp
using System.Globalization;

public override string ToString()
{
    var culture = CultureInfo.GetCultureInfo("pt-BR");
    return $"{Valor.ToString("N2", culture)} {Moeda}";
}
```

### 2. Warnings CS8618 - Propriedades Non-Nullable

**Erro:**
```
warning CS8618: Non-nullable property must contain a non-null value when exiting constructor
```

**Arquivos Afetados:**
- `FechamentoMensal.cs` - AnoMes
- `Transacao.cs` - Descricao, Valor, Usuario, Categoria
- `OrcamentoMensal.cs` - AnoMes, Valor, Usuario
- `Money.cs` - Moeda
- `Email.cs` - Valor

**Solucao:**
Inicializar propriedades nos construtores privados (usados pelo EF Core):

```csharp
// Antes
private Money() { }

// Depois
private Money()
{
    Moeda = "BRL";
}
```

### 3. Erros xUnit1012 - Null em Parametros Non-Nullable

**Erro:**
```
error xUnit1012: Null should not be used for type parameter of type 'string'
```

**Arquivos Afetados:**
- `CategoriaAdvancedTests.cs`
- `EmailTests.cs`
- `UsuarioAdvancedTests.cs`
- `MoneyTests.cs`
- `MetaTests.cs` (2 ocorrencias)
- `FechamentoMensalTests.cs`
- `OrcamentoMensalTests.cs`

**Solucao:**
Remover `[InlineData(null)]` e criar testes separados:

```csharp
// Antes
[Theory]
[InlineData("")]
[InlineData(" ")]
[InlineData(null)]
public void Test(string param) { }

// Depois
[Theory]
[InlineData("")]
[InlineData(" ")]
public void Test(string param) { }

[Fact]
public void Test_Null()
{
    var act = () => new Class(null!);
    act.Should().Throw<ArgumentException>();
}
```

## Arquivos Modificados

### Value Objects
1. `src/SpendWise.Domain/ValueObjects/Money.cs`
   - Adicionar `using System.Globalization`
   - Corrigir `ToString()` para usar pt-BR
   - Inicializar `Moeda` no construtor privado

2. `src/SpendWise.Domain/ValueObjects/Email.cs`
   - Inicializar `Valor` no construtor privado

### Entidades
3. `src/SpendWise.Domain/Entities/Transacao.cs`
   - Inicializar `Descricao`, `Valor`, `Usuario`, `Categoria`
   - Adicionar `= null!` nas navigation properties

4. `src/SpendWise.Domain/Entities/OrcamentoMensal.cs`
   - Inicializar `AnoMes`, `Valor`, `Usuario`
   - Adicionar `= null!` na navigation property

5. `src/SpendWise.Domain/Entities/FechamentoMensal.cs`
   - Inicializar `AnoMes` no construtor privado

### Testes
6. `tests/SpendWise.Domain.Tests/ValueObjects/MoneyTests.cs`
   - Remover `[InlineData(null)]`
   - Adicionar `Money_ShouldThrowException_WhenCurrencyIsNull()`

7. `tests/SpendWise.Domain.Tests/ValueObjects/EmailTests.cs`
   - Remover `[InlineData(null)]`
   - Adicionar `Email_ShouldThrowException_WhenEmailIsNull()`

8. `tests/SpendWise.Domain.Tests/Entities/CategoriaAdvancedTests.cs`
   - Remover `[InlineData(null)]`
   - Adicionar `AtualizarNome_DeveLancarException_QuandoNomeNull()`

9. `tests/SpendWise.Domain.Tests/Entities/UsuarioAdvancedTests.cs`
   - Remover `[InlineData(null)]`
   - Adicionar `AtualizarNome_DeveLancarException_QuandoNomeNull()`

10. `tests/SpendWise.Domain.Tests/Entities/MetaTests.cs`
    - Remover 2x `[InlineData(null)]`
    - Adicionar `CriarMeta_DeveLancarException_QuandoNomeNull()`
    - Adicionar `CriarMeta_DeveLancarException_QuandoDescricaoNull()`

11. `tests/SpendWise.Domain.Tests/Entities/FechamentoMensalTests.cs`
    - Remover `[InlineData(null)]`
    - Adicionar `CriarFechamentoMensal_DeveLancarException_QuandoAnoMesNull()`

12. `tests/SpendWise.Domain.Tests/Entities/OrcamentoMensalTests.cs`
    - Remover `[InlineData(null)]`
    - Adicionar `OrcamentoMensal_ShouldThrowException_WhenAnoMesIsNull()`

## Resultados

### Build Local
```
dotnet build SpendWise.sln --configuration Release
```
- Status: SUCCESS
- Warnings: 2 (conflito de versao EF Core - nao critico)
- Errors: 0

### Testes Local
```
dotnet test SpendWise.sln --configuration Release
```
- Total: 361 testes
- Passaram: 361
- Falharam: 0
- Ignorados: 0
- Duracao: 5.5s

### Pipeline CI
Aguardando execucao no GitHub Actions para confirmar que todas as correcoes funcionam no ambiente CI.

## Commit

**Hash:** 4ff4249
**Mensagem:** fix: corrigir erros de pipeline CI
**Status:** Pushed para repositorio remoto

## Proximos Passos

1. Aguardar pipeline CI completar
2. Verificar se todos os jobs passam:
   - Build
   - Code Quality
   - Unit Tests
   - Docker Build
3. Se necessario, fazer ajustes adicionais

## Referencias

- [xUnit1012](https://xunit.net/xunit.analyzers/rules/xUnit1012)
- [CS8618](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/nullable-warnings#nonnullable-reference-not-initialized)
- [CultureInfo](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)
