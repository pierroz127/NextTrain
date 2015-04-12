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

open System.Text.RegularExpressions
open System
open NextTrain.Lib

type StationResult = {StationName: string; Code: int; Value: float; Index: int; Length: int}
    
type MatcherResult = 
| Departure of StationResult
| DepartureArrival of StationResult * StationResult
| TaggedDeparture of string * StationResult
| TaggedDepartureArrival of string * StationResult * StationResult
| NoResult

module Matcher =
    type StationPattern = {Station: Station.StifStation.Row; Patterns: string list}

    let rHashTag = new Regex("#(?<tag>\w+)")

    let preProcess (s: string) = 
        let s1 = s.Replace("-", " ")
        let s2 = Regex.Replace(s1, "^gare( de)?\s", "", RegexOptions.IgnoreCase)     
        let s3 = Regex.Replace(s2, "(^|\s)saint\s", " st ", RegexOptions.IgnoreCase)
        s3.ToLower().Trim()    
        
    let stationPatterns = 
        let extractStationPatterns (s: Station.StifStation.Row) = 
            let patterns = [ s.Libelle; s.Libelle_point_d_arret; s.Libelle_stif_info_voyageurs; s.Libelle_sms_gare; s.Nom_gare ] |> List.map(preProcess)
            if s.Other_names.Length > 0
            then patterns |> List.append (s.Other_names.Split(',') |> Seq.toList)
            else patterns


        Station.idfStations.Rows
        |> Seq.map(fun s -> {Station = s; Patterns = extractStationPatterns s})
        |> Seq.toList

    // map with patterns as keys and station type as value
    let stationMap = 
        stationPatterns
        |> List.fold (fun acc s -> 
            (s.Patterns 
            |> List.fold (fun acc1 pattern -> 
                if (not (acc1 |> Map.containsKey pattern)) then acc1 |> Map.add pattern s else acc1) acc)) 
            Map.empty 
            
    let bkTree = BKTree(stationPatterns |> List.fold(fun acc x -> x.Patterns @ acc) [])
    
    let tryFindStations (tagCache: TagCache) user sentence = 
        let reg = new Regex("\\s+")
        let words = reg.Split(preProcess sentence)

               
        let getClosestStation input index length = 
            let computeDistance (input: string) (stationNames: string list) =
                stationNames |> List.map(fun s -> Levenshtein.relativeDistance input s) |> List.min
                        
            match stationMap |> Map.tryFind input with
            | Some(x) ->  Some({StationName = x.Station.Libelle; 
                Code = x.Station.Code_uic; 
                Index = index; 
                Value = 0.;
                Length = length;})
//            | None -> stationPatterns |> List.map (fun s -> 
//                    {StationName = s.Station.Libelle; 
//                    Code = s.Station.Code_uic; 
//                    Index = index; 
//                    Value = (computeDistance input s.Patterns);
//                    Length = length;})
//                    |> List.sortBy (fun r -> r.Value)
//                    |> List.head
            | None -> 
                let m = bkTree.findClosest input ((int)(ceil 0.2 * (float)input.Length))
                match m with
                | Some(v) -> 
                    let s = stationMap |> Map.find v.value
                    Some(
                        {StationName = s.Station.Libelle; 
                        Code = s.Station.Code_uic; 
                        Index = index; 
                        Value = (float)v.dist/(float)input.Length; 
                        Length = length;})
                | None -> None   
                
           
        let tryMatchStation s index length acc1 = 
             match (getClosestStation s index length) with
             | Some(result) -> result :: acc1
             | None -> acc1  
            
        let findStationsFromSentenceByIndex acc1 index length = 
            let s = String.Join(" ", words |> Seq.skip index |> Seq.take length |> Seq.toArray)
            tryMatchStation s index length acc1

        let findStationsFromSentence acc window =
            [0 .. words.Length - window] 
            |> List.fold (fun acc1 index -> findStationsFromSentenceByIndex acc1 index window) acc 
            |> List.filter (fun r -> r.Value <= 0.2)
            |> List.sortBy (fun r -> r.Value)
        
        let rec trimOverlapping acc results = 
            match results with
            | head :: tail -> trimOverlapping (head :: acc) (tail |> List.filter (fun r -> (r.Index + r.Length - 1) < head.Index || r.Index > (head.Index + head.Length - 1) ) )
            | [] -> acc

        let getStationResultFromCode code = 
            {StationName = Station.getNameFromUicCode code; Code = code; Value = 0.; Index = 0; Length = 0;}

        let getResultFromTag tag = 
            match tagCache.tryFindTrip user tag with
            | Some(trip) -> 
                match trip with
                | DepartureTrip(tag, s) -> Departure(getStationResultFromCode s)
                | DepartureArrivalTrip(tag, d, a) -> DepartureArrival(getStationResultFromCode d, getStationResultFromCode a)
            | None -> NoResult

        let results =
            [1..4] 
            |> List.fold (fun acc window -> findStationsFromSentence acc window) []
            |> List.sortBy (fun r -> r.Value) 
            |> trimOverlapping []
            |> List.sortBy (fun r -> r.Index)

        let m = rHashTag.Match(sentence)
        if m.Success 
        then
            let tag = m.Groups.["tag"].Value
            match results with
            | [] -> getResultFromTag tag
            | [res] -> TaggedDeparture(tag, res)
            | d :: a :: tail -> TaggedDepartureArrival(tag, d, a)
        else 
            match results with
            | [] -> NoResult
            | [res] -> Departure(res)
            | d :: a :: tail -> DepartureArrival(d, a)


