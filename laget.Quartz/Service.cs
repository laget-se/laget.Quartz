using laget.Quartz.Extensions;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl.Matchers;
using Serilog;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace laget.Quartz
{
    public class Service : BackgroundService
    {
        private readonly IEnumerable<Job> _jobs;
        private readonly IScheduler _scheduler;

        public Service(
            IEnumerable<Job> jobs,
            IScheduler scheduler)
        {
            _jobs = jobs;
            _scheduler = scheduler;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Log.Information($"{Assembly.GetEntryAssembly()?.ManifestModule.Name} is now started, to safely close the application press CTRL+C once!");

            foreach (var job in _jobs)
            {
                Schedule(job);
            }

            _scheduler.ListenerManager.AddJobListener(new Listener(), GroupMatcher<JobKey>.AnyGroup());

            await _scheduler.Start(cancellationToken);
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
