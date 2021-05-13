using System;
using Quartz;
using IQuartzJob = Quartz.IJob;

namespace laget.Quartz
{
    public interface IJob : IQuartzJob
    {
        ITrigger Trigger { get; }
    }

    public class Job
    {
        protected virtual TimeSpan Interval => TimeSpan.FromSeconds(60);
    }
}
