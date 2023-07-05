using laget.Quartz.Abstractions;
using laget.Quartz.Attributes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if NET6_OR_GREATER
using Lifetime = Microsoft.Extensions.Hosting.IHostApplicationLifetime;
#else
using Lifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;
#endif

namespace laget.Quartz
{
    internal sealed class QuartzHostedService : IHostedService
    {
        private readonly Lifetime _applicationLifetime;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IOptions<QuartzHostedServiceOptions> _options;

        private IScheduler? _scheduler;
        internal Task? _startupTask;
        private bool _schedulerWasStarted;

        public QuartzHostedService(
            Lifetime applicationLifetime,
            ISchedulerFactory schedulerFactory,
            IOptions<QuartzHostedServiceOptions> options)
        {
            _applicationLifetime = applicationLifetime;
            _schedulerFactory = schedulerFactory;
            _options = options;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Require successful initialization for application startup to succeed
            _scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

            Log.Information($"{Assembly.GetEntryAssembly()?.ManifestModule.Name} is now starting, to safely close the application press CTRL+C once!");

            //RegisterJobs();
            //scheduler.ListenerManager.AddJobListener(new DefaultJobListener(), GroupMatcher<JobKey>.AnyGroup());
            //return _scheduler.Start(cancellationToken);

            // Sensible mode: proceed with startup, and have jobs start after application startup
            if (_options.Value.AwaitApplicationStarted)
            {
                // Follow the pattern from BackgroundService.StartAsync: https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/BackgroundService.cs

                _startupTask = AwaitStartupCompletionAndStartSchedulerAsync(cancellationToken);

                // If the task completed synchronously, await it in order to bubble potential cancellation/failure to the caller
                // Otherwise, return, allowing application startup to complete
                if (_startupTask.IsCompleted)
                {
                    await _startupTask.ConfigureAwait(false);
                }
            }
            else // Legacy mode: start jobs inline
            {
                _startupTask = StartSchedulerAsync(cancellationToken);
                await _startupTask.ConfigureAwait(false);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _scheduler.Shutdown(cancellationToken);
        }

        private void RegisterJobs()
        {
            var assembly = Assembly.GetEntryAssembly();
            var jobs = assembly?.DefinedTypes.Where(t => t.BaseType == typeof(Job) && !t.IsDefined(typeof(DisableRegistrationAttribute), false)).ToList();

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
