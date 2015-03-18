namespace NextTrain.Service

open System.ServiceProcess
open System.Runtime.Remoting
open System.Runtime.Remoting.Channels

type TrainWindowsService() =
    inherit ServiceBase(ServiceName = "NextTrainService")
  
    override x.OnStart(args) =
        printfn "hello"
    
    override x.OnStop() = ()

        