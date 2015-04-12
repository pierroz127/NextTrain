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

namespace NextTrain.Lib

open FSharp.Data

module Station =      
    type StifStation = CsvProvider<"sncf-gares-et-arrets-transilien-ile-de-france.csv", ";">

    let idfStations = StifStation.Load("sncf-gares-et-arrets-transilien-ile-de-france.csv")
    
    let length = idfStations.Rows |> Seq.length
    
    let getNameFromUicCode (code: int) = 
        let res = idfStations.Rows |> Seq.tryFind (fun s -> s.Code_uic = code)
        match res with
        | Some(s) -> s.Libelle
        | None -> "UNKNOWN"

    let getUicCodeFromName stationName = 
        let res = idfStations.Rows |> Seq.tryFind (fun s -> s.Libelle = stationName)
        match res with
        | Some(s) -> s.Code_uic
        | None -> 0

    let distinctStations keySelector = idfStations.Rows |> Seq.distinctBy(keySelector) |> Seq.map(keySelector)
    
    let distinctLibellesGare = distinctStations (fun s -> s.Libelle)
    let distinctLibellesPointDArret = distinctStations (fun s -> s.Libelle_point_d_arret)
    let distinctNomGare = distinctStations (fun s -> s.Nom_gare)
