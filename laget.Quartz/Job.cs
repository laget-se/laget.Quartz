using Quartz;
using System;
using System.Threading.Tasks;

namespace laget.Quartz
{
    public abstract class Job : IJob
    {
        public abstract string Group { get; }
        public abstract string Name { get; }
        public abstract ITrigger Trigger { get; }

        protected abstract Task ExecuteAsync(IJobExecutionContext context);

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await ExecuteAsync(context);
            }
            catch (Exception ex)
            {
                throw new JobExecutionException(ex);
            }
        }
    }
}