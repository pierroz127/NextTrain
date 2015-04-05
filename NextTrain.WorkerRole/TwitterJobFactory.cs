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
