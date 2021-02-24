using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectTemplate.Quartz;
using Quartz;
using Serilog;

namespace ProjectTemplate.Jobs
{
    public class HelloJob : IJob, IScheduledJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Log.Information("Hello");
            return Task.CompletedTask;
        }

        public IJobDetail GetJobDetail()
        {
            return JobBuilder.Create<HelloJob>()
                .WithIdentity("HelloJob", "ProjectTemplate")
                .StoreDurably()
                .Build();
        }

        public IEnumerable<ITrigger> GetTriggers()
        {
            yield return TriggerBuilder.Create()
                .WithIdentity("HelloJob_Trigger1", "ProjectTemplate")
                .WithCronSchedule("*/30 * * * * ?")
                .ForJob("HelloJob", "ProjectTemplate")
                .Build();

            yield return TriggerBuilder.Create()
                .WithIdentity("HelloJob_RightNow", "ProjectTemplate")
                .StartNow()
                .ForJob("HelloJob", "ProjectTemplate")
                .Build();
        }
    }
}