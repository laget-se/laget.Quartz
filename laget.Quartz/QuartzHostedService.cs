using laget.Quartz.Abstractions;
using laget.Quartz.Attributes;
using laget.Quartz.Extensions;
using laget.Quartz.Listeners;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl.Matchers;
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
                // Follow the pattern from BackgroundService.StartAsync:
                // https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/BackgroundService.cs
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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stopped without having been started
            if (_scheduler is null || _startupTask is null)
            {
                return;
            }

            try
            {
                // Wait until any ongoing startup logic has finished or the graceful shutdown period is over
                await Task.WhenAny(_startupTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
            finally
            {
                if (_schedulerWasStarted && !cancellationToken.IsCancellationRequested)
                {
                    await _scheduler.Shutdown(_options.Value.WaitForJobsToComplete, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task RegisterJobs()
        {
            var assembly = Assembly.GetEntryAssembly();
            var jobs = assembly?.DefinedTypes.Where(t => t.BaseType == typeof(Job) && !t.IsDefined(typeof(DisableRegistrationAttribute), false)).ToList();

            Log.Information($"Quartz scheduler will register {jobs?.Count ?? 0} jobs");

            if (jobs == null) return;

            foreach (var job in jobs)
            {
                await ScheduleJob(Activator.CreateInstance(job) as Job).ConfigureAwait(false);
            }
        }

        private async Task ScheduleJob(Job entity)
        {
            if (_scheduler is null)
            {
                throw new InvalidOperationException("The scheduler should have been initialized first.");
            }

            Log.Information($"Quartz scheduler is configuring the scheduler for '{entity.Group}.{entity.Name}'");

            var job = JobBuilder
                .Create(entity.GetType())
                .WithIdentity(entity.Name, entity.Group)
                .Build();

            await _scheduler.ScheduleJob(job, entity.Trigger).ConfigureAwait(false);

            var trigger = await _scheduler.GetTrigger(entity.Trigger.Key);
            Log.Information($"The next occurrence of the '{entity.Group}.{entity.Name}' schedule (Trigger='{entity.Trigger.Key}', At='{trigger.GetNextFireTime()}')");
        }


        private async Task AwaitStartupCompletionAndStartSchedulerAsync(CancellationToken startupCancellationToken)
        {
            using var combinedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(startupCancellationToken, _applicationLifetime.ApplicationStarted);

            await Task.Delay(Timeout.InfiniteTimeSpan, combinedCancellationSource.Token) // Wait "indefinitely", until startup completes or is aborted
                .ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnCanceled) // Without an OperationCanceledException on cancellation
                .ConfigureAwait(false);

            if (!startupCancellationToken.IsCancellationRequested)
            {
                await StartSchedulerAsync(_applicationLifetime.ApplicationStopping).ConfigureAwait(false); // Startup has finished, but ApplicationStopping may still interrupt starting of the scheduler
            }
        }

        /// <summary>
        /// Starts the <see cref="IScheduler"/>, either immediately or after the delay configured in the <see cref="options"/>.
        /// </summary>
        private async Task StartSchedulerAsync(CancellationToken cancellationToken)
        {
            if (_scheduler is null)
            {
                throw new InvalidOperationException("The scheduler should have been initialized first.");
            }

            _schedulerWasStarted = true;

            // Avoid potential race conditions between ourselves and StopAsync, in case it has already made its attempt to stop the scheduler
            if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                return;
            }

            //TODO: Move this?
            if (_options.Value.RegisterDefaultJobListener)
                _scheduler.Register(new DefaultJobListener(), new IMatcher<JobKey>[] { GroupMatcher<JobKey>.AnyGroup() });
            if (_options.Value.RegisterDefaultSchedulerListener)
                _scheduler.Register(new DefaultSchedulerListener());
            if (_options.Value.RegisterDefaultTriggerListener)
                _scheduler.Register(new DefaultTriggerListener(), new IMatcher<TriggerKey>[] { GroupMatcher<TriggerKey>.AnyGroup() });

            if (_options.Value.StartDelay.HasValue)
            {
                await _scheduler.StartDelayed(_options.Value.StartDelay.Value, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await _scheduler.Start(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
