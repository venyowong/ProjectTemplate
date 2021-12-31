using Microsoft.AspNetCore.Http;
using ProjectTemplate.Quartz;
using Quartz;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectTemplate.Middlewares
{
    /// <summary>
    /// 该中间件用于提供手动触发任务的入口
    /// </summary>
    public class ManualTriggerMiddleware
    {
        private RequestDelegate requestDelegate;
        private IEnumerable<IScheduledJob> jobs;

        public ManualTriggerMiddleware(RequestDelegate requestDelegate, IEnumerable<IScheduledJob> jobs)
        {
            this.requestDelegate = requestDelegate;
            this.jobs = jobs;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue)
            {
                if (context.Request.Path.Value.ToLower().Contains("/run") && context.Request.Query.ContainsKey("job"))
                {
                    var jobName = context.Request.Query["job"].ToString().ToLower();
                    var scheduledJob = this.jobs.FirstOrDefault(x => x.GetType().Name.ToLower() == jobName);
                    if (scheduledJob != null)
                    {
                        var job = context.RequestServices.GetService(scheduledJob.GetJobDetail().JobType) as IJob;
                        await job.Execute(null);
                        context.Response.StatusCode = 200;
                        return;
                    }
                }
            }

            await requestDelegate.Invoke(context);
        }
    }
}
