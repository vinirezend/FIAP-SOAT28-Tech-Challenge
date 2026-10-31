using Oficina.Application.Implementations.Services;
using Oficina.Application.Interfaces.Resources;
using Oficina.Application.Interfaces.Services;
using Oficina.Domain.Interfaces.Repositories;
using Oficina.Infrastructure.Configurations;
using Oficina.Infrastructure.Repositories;
using Oficina.Infrastructure.Resources;

namespace Oficina.Api.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services) => services
        .AddScoped<IUsuarioRepository, UsuarioRepository>()
        .AddScoped<IClienteRepository, ClienteRepository>()
        .AddScoped<IVeiculoRepository, VeiculoRepository>()
        .AddScoped<IInsumoRepository, InsumoRepository>()
        .AddScoped<IServicoRepository, ServicoRepository>()
        .AddScoped<IColaboradorRepository, ColaboradorRepository>()
        .AddScoped<IStatusRepository, StatusRepository>()
        .AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();

    public static IServiceCollection AddApplicationServices(this IServiceCollection services) => services
        .AddScoped<IClienteService, ClienteService>()
        .AddScoped<IVeiculoService, VeiculoService>()
        .AddScoped<IInsumoService, InsumoService>()
        .AddScoped<IServicoService, ServicoService>()
        .AddScoped<IColaboradorService, ColaboradorService>()
        .AddScoped<IStatusService, StatusService>()
        .AddScoped<IOrdemServicoService, OrdemServicoService>()
        .AddScoped<IOrcamentoService, OrcamentoService>()
        .AddScoped<IAcompanhamentoOrdemServicoService, AcompanhamentoOrdemServicoService>()
        .AddScoped<IEnvioLinkAssinadoService, EnvioLinkAssinadoService>();

    public static IServiceCollection AddResources(this IServiceCollection services)
    {
        services.AddOptions<EmailConfiguration>()
            .BindConfiguration(EmailConfiguration.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<LinkAssinadoConfiguration>()
            .BindConfiguration(LinkAssinadoConfiguration.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddHttpContextAccessor()
            .AddTransient<ITokenGenerator, TokenGenerator>()
            .AddSingleton<IPasswordHasher, PasswordHasher>()
            .AddScoped<ICurrentUser, CurrentUserService>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IEmailSender, EmailSender>()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<ILinkAssinadoGenerator, LinkAssinadoGenerator>();
    }
}
