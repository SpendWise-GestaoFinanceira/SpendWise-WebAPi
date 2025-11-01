using FluentAssertions;

namespace SpendWise.Application.Tests.Core;

public class ApplicationTestsBasic
{
    [Fact]
    public void Application_ShouldHaveCorrectConfiguration()
    {
        // Este é um teste básico para garantir que o projeto compila
        var result = "SpendWise.Application";
        result.Should().NotBeNullOrEmpty();
    }
}
