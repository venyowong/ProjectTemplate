using Quartz;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ProjectTemplate.Quartz
{
    public abstract class UnrepeatableJob : IJob, IJobState
    {
        public static ConcurrentDictionary<string, bool> _runningJobs = new ConcurrentDictionary<string, bool>();

        public bool Running
        {
            get
            {
                _runningJobs.TryGetValue(this.GetType().FullName, out var running);
                return running;
            }
        }

        public Task Execute(IJobExecutionContext context)
        {
            if (this.Running)
            {
                return Task.CompletedTask;
            }

            var typeName = this.GetType().FullName;
            lock (this.GetType())
            {
                if (this.Running)
                {
                    return Task.CompletedTask;
                }

                if (!_runningJobs.ContainsKey(typeName))
                {
                    _runningJobs.TryAdd(typeName, true);
                }
                else
                {
                    _runningJobs[typeName] = true;
                }

                try
                {
                    this.ExecuteAsync(context).Wait();
                }
                catch (Exception e)
                {
                    Log.Error(e, $"发生未处理异常 job: {this.GetType().Name}");
                }

                _runningJobs[typeName] = false;

                return Task.CompletedTask;
            }
        }

        public abstract Task ExecuteAsync(IJobExecutionContext context);
    }
}
