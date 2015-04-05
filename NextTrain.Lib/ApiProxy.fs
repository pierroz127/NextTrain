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

open System.Configuration
open HttpClient
open FSharp.Data
open System.Globalization

type TrainSchedules = XmlProvider<"""<?xml version="1.0" encoding="UTF-8"?><passages gare="87381905"><train><date mode="R">03/12/2015 18:22</date><num>ODET78</num><miss>ODET</miss><term>87758375</term></train><train><date mode="R">03/12/2015 18:26</date><num>UGUI63</num><miss>UGUI</miss><term>87382655</term></train></passages>""", Culture="fr-FR">

module ApiProxy =
    let getDepartureUrl = sprintf "http://api.transilien.com/gare/%d/depart/"
    let getDepartureArrivalUrl = sprintf "http://api.transilien.com/gare/%d/depart/%d/"
    
    let sendTrainRequest (config: NextTrain.Lib.Configuration) count url =
        printfn "GET %s" url
        let body = 
            createRequest Get url
            |> withBasicAuthentication config.ApiUserName config.ApiPassword
            |> getResponseBody
        printfn "response: %s" body
        let trains = TrainSchedules.Parse(body)
        if trains.Trains.Length = 0
        then 
            printfn "nothing returned by sncf api"
            []
        else trains.Trains 
            |> Seq.sortBy (fun t -> t.Date.Value) 
            |> Seq.take ([ count; trains.Trains.Length] |> List.min)
            |> Seq.toList

    let nextTrain config departureCode count = 
        getDepartureUrl departureCode
        |> sendTrainRequest config count

    let nextTrainTo config departureCode arrivalCode count = 
        getDepartureArrivalUrl departureCode arrivalCode
        |> sendTrainRequest config count
            
        
