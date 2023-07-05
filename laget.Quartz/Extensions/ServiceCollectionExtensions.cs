using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace laget.Quartz.Extensions
{
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Configures Quartz services to underlying service collection.
        /// </summary>
        public static IServiceCollection AddQuartzHostedService(this IServiceCollection services, Action<QuartzHostedServiceOptions> configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }

            return services.AddSingleton<IHostedService, QuartzHostedService>();
        }
    }
}
