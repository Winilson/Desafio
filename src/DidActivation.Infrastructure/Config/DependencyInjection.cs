using DidActivation.Application.Interfaces;
using DidActivation.Application.Services;
using DidActivation.Domain.Ports;
using DidActivation.Infrastructure.Partner;
using DidActivation.Infrastructure.Persistence;
using DidActivation.Infrastructure.Resilience;
using DidActivation.Infrastructure.Status;
using DidActivation.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace DidActivation.Infrastructure.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDidActivation(this IServiceCollection services)
        {
            services.AddSingleton<IActivationRepository, InMemoryActivationRepository>();
            services.AddSingleton<IStatusMappingEngine, StatusMappingEngine>();
            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

            services.AddSingleton<PartnerGateway>();
            services.AddSingleton<IPartnerGateway>(sp =>
            {
                var inner = sp.GetRequiredService<PartnerGateway>();
                return new ResilientPartnerGateway(inner);
            });

            services.AddScoped<IActivationService, ActivationService>();

            return services;
        }
    }
}
