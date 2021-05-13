using Microsoft.Extensions.DependencyInjection;

namespace laget.Quartz.Extensions
{
    public static class ConfigureExtensions
    {
        public static void AddQuartzService(this IServiceCollection services)
        {
            services.AddHostedService<Service>();
        }
    }
}
