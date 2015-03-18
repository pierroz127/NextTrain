// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System
open System.Configuration
open System.IO
open Quartz
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
        
[<EntryPoint>]
let main argv = 
    try
        let scheduler = StdSchedulerFactory.GetDefaultScheduler();
        scheduler.Start()

        printfn "Starting service..."
        // define the job and tie it to our HelloJob class
        let lastIdManager = ToolIdManager(ConfigurationManager.AppSettings.["sinceIdFileName"])
        let job = JobBuilder.Create<TwitterJob>().WithIdentity("twitterJob", "twitterGroup").Build()
        let sched = Action<_>(fun (x: SimpleScheduleBuilder) -> x.WithIntervalInSeconds(45).RepeatForever() |> ignore)
        let trigger = TriggerBuilder.Create().WithIdentity("twitterTrigger", "twitterGroup").StartNow().WithSimpleSchedule(sched).Build()
        trigger.JobDataMap.Put("lastId", lastIdManager) |> ignore
        scheduler.ScheduleJob(job, trigger) |> ignore
        Console.ReadKey() |> ignore
        scheduler.Shutdown()
        0
    with
    | ex -> printfn "%s" (ex.ToString()); 1

