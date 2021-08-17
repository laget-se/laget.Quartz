using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using laget.Quartz.Extensions;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl.Matchers;
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
            Log.Information($"{Assembly.GetEntryAssembly()?.ManifestModule.Name} is now started, to safely close the application press CTRL+C once!");

            RegisterJobs();
            _scheduler.ListenerManager.AddJobListener(new Listener(), GroupMatcher<JobKey>.AnyGroup());

            return _scheduler.Start(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _scheduler.Shutdown(cancellationToken);
        }

        private void RegisterJobs()
        {
            var assembly = Assembly.GetEntryAssembly();
            var jobs = assembly?.DefinedTypes.Where(t => t.BaseType == typeof(Job)).ToList();
            
            Log.Information($"Quartz scheduler will register {jobs?.Count ?? 0} jobs");

            if (jobs == null) return;

            foreach (var job in jobs)
            {
                Schedule(Activator.CreateInstance(job) as Job);
            }
        }

        private async void Schedule(Job entity)
        {
            Log.Information($"Quartz scheduler is configuring the scheduler for '{entity.Group}.{entity.Name}'");

            var job = JobBuilder
                .Create(entity.GetType())
                .WithIdentity(entity.Name, entity.Group)
                .Build();

            await _scheduler.ScheduleJob(job, entity.Trigger);
            
            var trigger = await _scheduler.GetTrigger(entity.Trigger.Key);
            Log.Information($"The next occurrence of the '{entity.Group}.{entity.Name}' schedule (Trigger='{entity.Trigger.Key}', At='{trigger.GetNextFireTime()}')");
        }
    }
}
