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


// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "bin/Debug/HttpClient.dll"
#r "../packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "../packages/linqtotwitter.3.1.2/lib/net45/LinqToTwitterPcl.dll"
#r "System.Xml.Linq"
#r "System.Linq.Expressions"
#r "System.Runtime"

//#load "Station.fs"
#load "Levenshtein.fs"
#load "BKTree.fs"
//#load "ApiProxy.fs"
//#load "TwitterListener.fs"

open NextTrain.Lib

// Define your library scripting code here

//ApiProxy.nextTrain (Station.getUicCodeFromName "CERGY PREFECTURE") 5  
//|> List.map (fun t -> printfn "Destination: %s at %s" (Station.getNameFromUicCode t.Term) (t.Date.Value.ToString("MMMM dd, yyyy HH:mm:ss")))
//|> ignore
//
//let printTrainDeparture departure arrival count = 
//    ApiProxy.nextTrainTo departure arrival count 
//    |> List.map (fun t -> printfn "Destination: %s at %s" (Station.getNameFromUicCode t.Term) (t.Date.Value.ToString("MMMM dd, yyyy HH:mm:ss")))
//    |> ignore
//
//let printNextTrain station count = 
//    ApiProxy.nextTrain station count
//    |> List.map (fun t -> printfn "Destination: %s at %s" (Station.getNameFromUicCode t.Term) (t.Date.Value.ToString("MMMM dd, yyyy HH:mm:ss")))
//    |> ignore
//
//printTrainDeparture (Station.getUicCodeFromName "MAISONS LAFFITTE") (Station.getUicCodeFromName "SARTROUVILLE") 1
let patterns = [ "sartrouville"; "cergy"; "achères"; "saint-germain"; "poissy"; "houilles"; "conflans" ]
let bktree = BKTree(patterns);
bktree.prettyPrint()
bktree.findClosest "sirtrouville" 3
bktree.findClosest "cregy" 3
bktree.findClosest "cinfalns" 3