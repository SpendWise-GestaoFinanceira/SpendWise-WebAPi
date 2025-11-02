using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SpendWise.Application.Mappings;
using SpendWise.Application.Services;
using SpendWise.Application.Validators.BusinessRules;

namespace SpendWise.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddAutoMapper(typeof(MappingProfile));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Add Application Services (Utilities only)
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IOrcamentoCalculoService, OrcamentoCalculoService>();

        // Add Business Rules (CQRS Pattern)
        services.AddScoped<IBusinessRule, TemporalValidationRule>();
        services.AddScoped<IBusinessRule, CategoryLimitValidationRule>();
        services.AddScoped<IBusinessRule, BudgetValidationRule>();
        services.AddScoped<IBusinessRule, PriorityValidationRule>();

        return services;
    }
}
