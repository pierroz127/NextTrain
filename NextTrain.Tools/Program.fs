open System
open System.Configuration
open System.IO
open Quartz
open Quartz.Spi
open Quartz.Impl
open NextTrain.Lib

type ToolIdManager(fileName) =
    interface ITweetIdManager with
        member this.Save(sinceId: uint64) = 
            use fs = new FileStream(fileName, FileMode.Create, FileAccess.Write)
            use wr = new StreamWriter(fs)
            wr.WriteLine(sinceId)
            printfn "write sinceId=%d" sinceId

        member this.Read() = 
            if not (File.Exists(fileName))
            then None
            else
                use rdr = new StreamReader(fileName)
                let sinceId = UInt64.Parse(rdr.ReadLine())
                printfn "read sinceId=%d" sinceId
                Some(sinceId)

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
        }
        scheduler.JobFactory <- new TwitterJobFactory(config, TweetLogger())
        
        // define the job and tie it to our HelloJob class
        let lastIdManager = ToolIdManager(ConfigurationManager.AppSettings.["sinceIdFileName"])
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

