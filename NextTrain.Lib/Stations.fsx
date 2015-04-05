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

#r "../packages/Http.fs.1.5.1/lib/net40/HttpClient.dll"
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "System.Xml.Linq"
#load "Levenshtein.fs"
#load "Station.fs"
#load "BKTree.fs"
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
    AccessTokenSecret="";
    Log = fun s -> printfn "%s" s
}

Bot.processRequest config "La défense sartrouville"
Bot.processRequest config "st lazare maisons laffitte"