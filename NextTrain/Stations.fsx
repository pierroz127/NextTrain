
#r "../packages/Http.fs.1.5.1/lib/net40/HttpClient.dll"
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
//#r "../packages/linqtotwitter.3.1.2/lib/net45/LinqToTwitterPcl.dll"
#r "System.Xml.Linq"
//#r "System.Linq.Expressions"
//#r "System.Runtime"
#load "Levenshtein.fs"
#load "Station.fs"
#load "Matcher.fs"
#load "ApiProxy.fs"
#load "Bot.fs"

open NextTrain
printfn "Stations: %d" Station.length
//Station.distinctLibellesGare |> Seq.toList |> List.sort |> List.map(printfn "%s") |> ignore
//Station.distinctLibellesPointDArret |> Seq.toList |> List.sort |> List.map(printfn "%s") |> ignore

Levenshtein.absoluteDistance "toto" "tutu"

Matcher.tryFindStations "saint lazare"
//Matcher.tryFindStations "auber cergy st christophe"
//Matcher.tryFindStations "chatelet les halles"
//Matcher.tryFindStations "gare du nord saint michel"
//Matcher.tryFindStations "cite universitaire gentilly"
//
//Bot.processRequest "prochain cergy auber"
//Bot.processRequest "saint lazare gisors"