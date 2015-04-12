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

type BotResult = 
| Result of string
| TaggedResult of Trip * string
| EmptyBotResult


module Bot =
    let arrivalDeparturePluralIntroTemplate  = sprintf "Les %d prochains %s - %s: \n"
    let arrivalDepartureSingleTemplate = sprintf "Le prochain %s - %s est prévu %s"
    let arrivalDepartureNoResponseTemplate = sprintf "Désolé, je n'ai aucun horaire pour le prochain %s - %s"
    let departurePluralIntroTemplate = sprintf "Les %d prochains trains au départ de %s:\n"
    let departureSingleTemplate = sprintf "Prochain train au départ de %s à direction de %s, prévu %s"
    let departureScheduleTemplate = sprintf "-%s, prévu %s"
    let departureNoResponseTemplate = sprintf "Désolé, je n'ai aucun horaire prévu pour un prochain train au départ de %s" 

    let getTime (dt: DateTime) = dt.ToString("le dd/MM/yyyy à HH:mm")
        
    let processRequest config tagCache user query = 
        let chrono = System.Diagnostics.Stopwatch.StartNew()
        let processDepartureArrival (departure: StationResult) (arrival: StationResult) =
            printfn "next train %s %s" departure.StationName arrival.StationName
            let res = ApiProxy.nextTrainTo config departure.Code arrival.Code 3 
            match res with
            | [] -> printfn "0"; res, (arrivalDepartureNoResponseTemplate departure.StationName arrival.StationName)
            | [x] -> printfn "1"; res, (arrivalDepartureSingleTemplate departure.StationName arrival.StationName (getTime x.Date.Value))
            | times ->
                let intro = arrivalDeparturePluralIntroTemplate times.Length departure.StationName arrival.StationName
                let schedule = String.Join("\n", times |> List.map(fun x -> getTime x.Date.Value) |> List.toArray)
                res, intro + schedule
                    
        let processDeparture (departure: StationResult) =  
            printfn "next train %s" departure.StationName
            let res = ApiProxy.nextTrain config departure.Code 3 
            match res with
            | [] -> res, (departureNoResponseTemplate departure.StationName)
            | [x] -> res, (departureSingleTemplate departure.StationName (Station.getNameFromUicCode x.Term) (getTime x.Date.Value))
            | times -> 
                let intro = departurePluralIntroTemplate times.Length departure.StationName
                let schedules = String.Join("\n", times |> List.map(fun x -> departureScheduleTemplate (Station.getNameFromUicCode x.Term) (getTime x.Date.Value)))
                res, intro + schedules
      
        let matcherResult = Matcher.tryFindStations tagCache user query 
        
//        let logApiResults apiResults = if apiResults.Length > 0 then String.Join(";", apiResults |> List.map(fun x -> x.Date.Value.ToString("u"))) else ""
        
        match matcherResult with
        | DepartureArrival(dep, arr) -> 
            let apiResults, response = processDepartureArrival dep arr 
            config.Log (sprintf @"""%s"";%s;%d;%s;%d;%dms;%s" 
                query dep.StationName dep.Code arr.StationName arr.Code chrono.ElapsedMilliseconds
                (String.Join(";", apiResults |> List.map(fun x -> x.Date.Value.ToString("u")))))
            Result(response)          
        | TaggedDepartureArrival(tag, dep, arr) -> 
            let apiResults, response = processDepartureArrival dep arr 
            config.Log (sprintf @"""%s"";%s;%d;%s;%d;%dms;%s" 
                query dep.StationName dep.Code arr.StationName arr.Code chrono.ElapsedMilliseconds
                (String.Join(";", apiResults |> List.map(fun x -> x.Date.Value.ToString("u")))))
            TaggedResult(DepartureArrivalTrip(tag, dep.Code, arr.Code), response)
        | Departure(dep) ->
            let apiResults, response = processDeparture dep 
            config.Log (sprintf @"""%s"";%s;%d;NULL;-1;%dms;%s" 
                query dep.StationName dep.Code chrono.ElapsedMilliseconds 
                (String.Join(";", apiResults |> List.map(fun x -> x.Date.Value.ToString("u")))))
            Result(response)
        | TaggedDeparture(tag, dep) -> 
            let apiResults, response = processDeparture dep
            config.Log (sprintf @"""%s"";%s;%d;NULL;-1;%dms;%s" 
                query dep.StationName dep.Code chrono.ElapsedMilliseconds 
                (String.Join(";", apiResults |> List.map(fun x -> x.Date.Value.ToString("u")))))
            TaggedResult(DepartureTrip(tag, dep.Code), response)
        | _ -> EmptyBotResult

//        let logDepartureName, logDepartureCode, logArrivalName, logArrivalCode = 
//            match matcherResult with
//            | DepartureArrival(dep, arr) 
//            | TaggedDepartureArrival(_, dep, arr) -> dep.StationName, dep.Code, arr.StationName, arr.Code
//            | Departure(dep) 
//            | TaggedDeparture(_, dep) -> dep.StationName, dep.Code, "NULL", -1
//            | _ -> "NULL", -1, "NULL", -1
            
            
//        config.Log (sprintf @"""%s"";%s;%d;%s;%d;%dms;%s" 
//            query logDepartureName logDepartureCode logArrivalName logArrivalCode chrono.ElapsedMilliseconds logApiResults)
//        
//        newTagCache, response

