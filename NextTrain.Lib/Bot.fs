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

open System
open NextTrain.Lib

module Bot =
    let arrivalDeparturePluralIntroTemplate  = sprintf "Les %d prochains %s - %s: \n"
    let arrivalDepartureSingleTemplate = sprintf "Le prochain %s - %s est prévu %s"
    let arrivalDepartureNoResponseTemplate = sprintf "Désolé, je n'ai aucun horaire pour le prochain %s - %s"
    let departurePluralIntroTemplate = sprintf "Les %d prochains trains au départ de %s:\n"
    let departureSingleTemplate = sprintf "Prochain train au départ de %s à direction de %s, prévu %s"
    let departureScheduleTemplate = sprintf "-%s, prévu %s"
    let departureNoResponseTemplate = sprintf "Désolé, je n'ai aucun horaire prévu pour un prochain train au départ de %s" 

    let getTime (dt: DateTime) = dt.ToString("le dd/MM/yyyy à HH:mm")
        
    let processRequest config query = 
        let chrono = System.Diagnostics.Stopwatch.StartNew()
        let processDepartureArrival (departure: Matcher.Result) (arrival: Matcher.Result) =
            printfn "next train %s %s" departure.StationName arrival.StationName
            let res = ApiProxy.nextTrainTo config departure.Code arrival.Code 3 
            match res with
            | [] -> printfn "0"; res, Some(arrivalDepartureNoResponseTemplate departure.StationName arrival.StationName)
            | [x] -> printfn "1"; res, Some(arrivalDepartureSingleTemplate departure.StationName arrival.StationName (getTime x.Date.Value))
            | times ->
                let intro = arrivalDeparturePluralIntroTemplate times.Length departure.StationName arrival.StationName
                let schedule = String.Join("\n", times |> List.map(fun x -> getTime x.Date.Value) |> List.toArray)
                res, Some(intro + schedule)
                    
        let processDeparture (departure: Matcher.Result) =  
            printfn "next train %s" departure.StationName
            let res = ApiProxy.nextTrain config departure.Code 3 
            match res with
            | [] -> res, Some(departureNoResponseTemplate departure.StationName)
            | [x] -> res, Some(departureSingleTemplate departure.StationName (Station.getNameFromUicCode x.Term) (getTime x.Date.Value))
            | times -> 
                let intro = departurePluralIntroTemplate times.Length departure.StationName
                let schedules = String.Join("\n", times |> List.map(fun x -> departureScheduleTemplate (Station.getNameFromUicCode x.Term) (getTime x.Date.Value)))
                res, Some(intro + schedules)
        
        let stations = Matcher.tryFindStations query 
        let (apiResults, response) = 
            match stations with
            | [r1;r2] -> processDepartureArrival r1 r2 
            | [r] -> processDeparture r
            | _ -> [], None

        let logDepartureName = if stations.Length > 0 then stations.[0].StationName else "NULL"
        let logDepartureCode = if stations.Length > 0 then stations.[0].Code else -1
        let logArrivalName = if stations.Length > 1 then stations.[1].StationName else "NULL"
        let logArrivalCode = if stations.Length > 1 then stations.[1].Code else -1
        let logApiResults = if apiResults.Length > 0 then String.Join(";", apiResults |> List.map(fun x -> x.Date.Value.ToString("u"))) else ""
        let logMessage = 
            sprintf @"""%s"";%s;%d;%s;%d;%dms;%s" query logDepartureName logDepartureCode logArrivalName logArrivalCode chrono.ElapsedMilliseconds logApiResults
        config.Log logMessage
        
        response

