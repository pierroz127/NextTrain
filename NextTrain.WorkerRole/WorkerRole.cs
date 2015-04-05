//   Copyright 2015 Pierre Leroy
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;
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
                        .WithIntervalInSeconds(60)
                        .RepeatForever())
                    .Build();
            trigger.JobDataMap.Put("lastId", new BlobManager());
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
