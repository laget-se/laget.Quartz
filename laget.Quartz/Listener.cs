using System.Threading;
using System.Threading.Tasks;
using laget.Quartz.Extensions;
using Quartz;
using Serilog;

namespace laget.Quartz
{
    public class Listener : IJobListener
    {
        public string Name => nameof(Listener);

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            Log.Information($"Vetoed '{context.JobDetail.Key}' (Reason='Trigger vetoed at {context.GetFireTime()}', Id='{context.FireInstanceId}')");
            return Task.CompletedTask;
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            Log.Information($"Executing '{context.JobDetail.Key}' (Reason='Trigger fired at {context.GetFireTime()}', Id='{context.FireInstanceId}')");
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            Log.Information($"Executed '{context.JobDetail.Key}' successfully (Reason='Trigger fired at {context.GetFireTime()}', Id='{context.FireInstanceId}')");
            return Task.CompletedTask;
        }
    }
}
