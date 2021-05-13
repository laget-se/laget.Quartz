using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;

namespace laget.Quartz
{
    public class Service : IHostedService
    {
        private readonly IScheduler _scheduler;

        public Service(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information($"{Assembly.GetExecutingAssembly().FullName} is now started, to safely close the application press CTRL+C once!");

            RegisterJobs();

            return _scheduler.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _scheduler.Shutdown(cancellationToken);
        }

        private void RegisterJobs()
        {
            var assembly = GetType().GetTypeInfo().Assembly;
            var jobs = assembly.DefinedTypes.Where(t => t.ImplementedInterfaces.Any(inter => inter == typeof(IJob))).ToList();

            foreach (var job in jobs)
            {
                Schedule(Activator.CreateInstance(job) as IJob);
            }
        }

        private async void Schedule(IJob entity)
        {
            var name = entity.GetType().FullName;

            Log.Information($"Configuring scheduler for {name}");

            var job = JobBuilder
                .Create(entity.GetType())
                .WithIdentity(name, $"{name}-Group")
                .Build();

            await _scheduler.ScheduleJob(job, entity.Trigger);

            var details = await _scheduler.GetTriggersOfJob(job.Key);
            Log.Information($"The next occurrence of the '{name}' schedule (Constant='{TimeSpan.FromSeconds(1)}') will be='{details.FirstOrDefault()?.GetNextFireTimeUtc()?.ToLocalTime().ToString(CultureInfo.CurrentCulture) ?? string.Empty}'");
        }
    }
}
