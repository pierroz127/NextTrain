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

open System
open System.Configuration
open System.IO
open Quartz
open Quartz.Spi
open Quartz.Impl
open NextTrain.Lib

type ToolIdManager(fileName, cacheName) =
    interface IDataManager with
        member this.SaveId(sinceId: uint64) = 
            use fs = new FileStream(fileName, FileMode.Create, FileAccess.Write)
            use wr = new StreamWriter(fs)
            wr.WriteLine(sinceId)
            printfn "write sinceId=%d" sinceId

        member this.ReadId() = 
            if not (File.Exists(fileName))
            then None
            else
                use rdr = new StreamReader(fileName)
                let sinceId = UInt64.Parse(rdr.ReadLine())
                printfn "read sinceId=%d" sinceId
                Some(sinceId)

        member this.SaveCache tagCache = 
            use fs = new FileStream(cacheName, FileMode.Create, FileAccess.Write)
            use wr = new StreamWriter(fs)
            wr.WriteLine(tagCache.toJson)

        member this.ReadCache() =
            use rdr = new StreamReader(cacheName)
            let tagCache = TagCache(Map.empty)
            tagCache.fromJson (rdr.ReadLine())
            
type TweetLogger() =
    interface ITweetLogger with
        member this.logDebug(msg: string) =
            printfn "%s DEBUG %s" (DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")) msg
        
        member this.logInfo(msg: string) =
            printfn "%s LOG %s" (DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")) msg
        
        member this.logWarn(msg: string) =
            printfn "%s WARN %s" (DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")) msg
        
        member this.logError(msg: string) =
            printfn "%s ERROR %s" (DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")) msg
        
type TwitterJobFactory(config) =
    interface IJobFactory with
        member this.NewJob(bundle: TriggerFiredBundle, scheduler: IScheduler) = 
            (new TwitterJob(config)) :> IJob
        member this.ReturnJob(job: IJob) = ()

[<EntryPoint>]
let main argv = 
    try
        let scheduler = StdSchedulerFactory.GetDefaultScheduler();
        let config = {
            ApiUserName = ConfigurationManager.AppSettings.Item("sncfApiUserName");
            ApiPassword = ConfigurationManager.AppSettings.Item("sncfApiPassword");
            ConsumerKey = ConfigurationManager.AppSettings.Item("consumerKey");
            ConsumerSecret = ConfigurationManager.AppSettings.Item("consumerSecret");
            AccessToken = ConfigurationManager.AppSettings.Item("accessToken");
            AccessTokenSecret = ConfigurationManager.AppSettings.Item("accessTokenSecret");
            Log = printfn "%s";
        }
        scheduler.JobFactory <- new TwitterJobFactory(config, TweetLogger())
        
        // define the job and tie it to our HelloJob class
        let lastIdManager = ToolIdManager(ConfigurationManager.AppSettings.["sinceIdFileName"], ConfigurationManager.AppSettings.["cacheFileName"])
        let job = JobBuilder.Create<TwitterJob>().WithIdentity("twitterJob", "twitterGroup").Build()
        let sched = Action<_>(fun (x: SimpleScheduleBuilder) -> x.WithIntervalInSeconds(45).RepeatForever() |> ignore)
        let trigger = TriggerBuilder.Create().WithIdentity("twitterTrigger", "twitterGroup").StartNow().WithSimpleSchedule(sched).Build()
        trigger.JobDataMap.Put("lastId", lastIdManager) |> ignore
        scheduler.ScheduleJob(job, trigger) |> ignore
        scheduler.Start()
        printfn "Starting service..."
        
        Console.ReadKey() |> ignore
        scheduler.Shutdown()
        0
    with
    | ex -> printfn "%s" (ex.ToString()); 1

