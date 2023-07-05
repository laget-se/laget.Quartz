using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace laget.Quartz.Extensions
{
    public static class ConfigureExtensions
    {
        public static IServiceCollection AddQuartzService(this IServiceCollection services, Action<QuartzHostedServiceOptions> configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }

            return services.AddSingleton<IHostedService, QuartzHostedService>();
        }
    }
}
