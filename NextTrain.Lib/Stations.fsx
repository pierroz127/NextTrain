
#r "../packages/Http.fs.1.5.1/lib/net40/HttpClient.dll"
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "System.Xml.Linq"
#load "Levenshtein.fs"
#load "Station.fs"
#load "Matcher.fs"
#load "Configuration.fs"
#load "ApiProxy.fs"
#load "Bot.fs"

open NextTrain.Lib

printfn "Stations: %d" Station.length
//Station.distinctLibellesGare |> Seq.toList |> List.sort |> List.map(printfn "%s") |> ignore
//Station.distinctLibellesPointDArret |> Seq.toList |> List.sort |> List.map(printfn "%s") |> ignore

//Levenshtein.absoluteDistance "toto" "tutu"

//Matcher.tryFindStations "saint lazare"
//Matcher.tryFindStations "auber cergy st christophe"
//Matcher.tryFindStations "chatelet les halles"
//Matcher.tryFindStations "gare du nord saint michel"
//Matcher.tryFindStations "cite universitaire gentilly"
//
//Bot.processRequest "prochain cergy auber"
let config =  {
    ApiUserName=""; // put your ID here
    ApiPassword="";
    ConsumerKey="";
    ConsumerSecret="";
    AccessToken="";
    AccessTokenSecret=""
}

Bot.processRequest config "La défense sartrouville"
Bot.processRequest config "st lazare maisons laffitte"