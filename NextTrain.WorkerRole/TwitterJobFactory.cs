using System;
using Common.Logging.Configuration;
using Microsoft.FSharp.Core;
using Microsoft.WindowsAzure;
using NextTrain.Lib;
using Quartz;
using Quartz.Spi;
using Configuration = NextTrain.Lib.Configuration;

namespace NextTrain.WorkerRole
{
    public class TwitterJobFactory: IJobFactory
    {
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return new TwitterJob(getConfiguration(), new Logger());
        }

        public void ReturnJob(IJob job)
        {
            // not implemented for now...
        }

        private Configuration getConfiguration()
        {
            return new Configuration(
                CloudConfigurationManager.GetSetting("sncfApiUserName"),
                CloudConfigurationManager.GetSetting("sncfApiPassword"),
                CloudConfigurationManager.GetSetting("consumerKey"),
                CloudConfigurationManager.GetSetting("consumerSecret"),
                CloudConfigurationManager.GetSetting("accessToken"),
                CloudConfigurationManager.GetSetting("accessTokenSecret"), 
                FSharpFunc<string, Unit>.FromConverter(BlobManager.LogTweet));
        }
    }
}
