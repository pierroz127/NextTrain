// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "bin/Debug/HttpClient.dll"
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "../packages/linqtotwitter.3.1.2/lib/net45/LinqToTwitterPcl.dll"
#r "System.Xml.Linq"
#r "System.Linq.Expressions"
#r "System.Runtime"

#load "Station.fs"
#load "ApiProxy.fs"
//#load "TwitterListener.fs"

open NextTrain

// Define your library scripting code here

ApiProxy.nextTrain (Station.getUicCodeFromName "CERGY PREFECTURE") 5  
|> List.map (fun t -> printfn "Destination: %s at %s" (Station.getNameFromUicCode t.Term) (t.Date.Value.ToString("MMMM dd, yyyy HH:mm:ss")))
|> ignore

let printTrainDeparture departure arrival count = 
    ApiProxy.nextTrainTo departure arrival count 
    |> List.map (fun t -> printfn "Destination: %s at %s" (Station.getNameFromUicCode t.Term) (t.Date.Value.ToString("MMMM dd, yyyy HH:mm:ss")))
    |> ignore

let printNextTrain station count = 
    ApiProxy.nextTrain station count
    |> List.map (fun t -> printfn "Destination: %s at %s" (Station.getNameFromUicCode t.Term) (t.Date.Value.ToString("MMMM dd, yyyy HH:mm:ss")))
    |> ignore

printTrainDeparture (Station.getUicCodeFromName "MAISONS LAFFITTE") (Station.getUicCodeFromName "SARTROUVILLE") 1
