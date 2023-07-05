using Quartz;

namespace laget.Quartz.Extensions
{
    public static class SchedulerExtensions
    {
        public static void Register(this IScheduler scheduler, IJobListener listener, IMatcher<JobKey>[] matchers)
        {
            scheduler.ListenerManager.AddJobListener(listener);
        }

        public static void Register(this IScheduler scheduler, ISchedulerListener listener)
        {
            scheduler.ListenerManager.AddSchedulerListener(listener);
        }

        public static void Register(this IScheduler scheduler, ITriggerListener listener, IMatcher<TriggerKey>[] matchers)
        {
            scheduler.ListenerManager.AddTriggerListener(listener, matchers);
        }
    }
}
