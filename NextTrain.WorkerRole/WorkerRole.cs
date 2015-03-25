using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using NextTrain.Lib;
using Quartz;
using Quartz.Impl;

namespace NextTrain.WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private IScheduler _scheduler;

        public override void Run()
        {
            Trace.TraceInformation("NextTrain.WorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
            _scheduler.Start();

            bool result = base.OnStart();


            Trace.TraceInformation("NextTrain.WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("NextTrain.WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            _scheduler.Shutdown();
            base.OnStop();

            Trace.TraceInformation("NextTrain.WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var job = JobBuilder.Create<TwitterJob>().WithIdentity("twitterJob", "twitterGroup").Build();
            var trigger =
                TriggerBuilder.Create()
                    .WithIdentity("twitterTrigger", "twitterGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(45)
                        .RepeatForever())
                    .Build();
            trigger.JobDataMap.Put("lastId", new LastIdManager());
            _scheduler.ScheduleJob(job, trigger);
        }
    }
}
