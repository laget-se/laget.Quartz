using System;
using System.Threading.Tasks;
using Quartz;

namespace laget.Quartz
{
    public abstract class Job : IJob
    {
        public abstract string Group { get; }
        public abstract string Name { get; }
        public abstract ITrigger Trigger { get; }

        public abstract Task ExecuteJob(IJobExecutionContext context);

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await ExecuteJob(context);
            }
            catch (Exception ex)
            {
                throw new JobExecutionException(ex);
            }
        }
    }
}