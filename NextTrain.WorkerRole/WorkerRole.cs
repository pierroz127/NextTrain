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
                this.Run(this.cancellationTokenSource.Token);
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 1;


            bool result = base.OnStart();

            _scheduler = StdSchedulerFactory.GetDefaultScheduler();
            _scheduler.JobFactory = new TwitterJobFactory();
            _scheduler.Start();
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

        private void Run(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                Thread.Sleep(30 * 1000);
            }
        }
    }
}
