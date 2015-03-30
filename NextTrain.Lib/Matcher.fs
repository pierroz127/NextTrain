namespace NextTrain.Lib

open System.Text.RegularExpressions
open System
open NextTrain.Lib

module Matcher =
    type Result = {StationName: string; Code: int; Value: float; Index: int; Length: int}
    type StationPattern = {Station: Station.StifStation.Row; Patterns: string list}

    let preProcess (s: string) = 
        let s1 = s.Replace("-", " ")
        let s2 = Regex.Replace(s1, "^gare( de)?\s", "", RegexOptions.IgnoreCase)     
        let s3 = Regex.Replace(s2, "(^|\s)saint\s", " st ", RegexOptions.IgnoreCase)
        s3.ToLower().Trim()    
        
    let stationPatterns = 
        let extractStationPatterns (s: Station.StifStation.Row) = 
            [ s.Libelle; s.Libelle_point_d_arret; s.Libelle_stif_info_voyageurs; s.Libelle_sms_gare; s.Nom_gare ] |> List.map(preProcess)
            
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
            
    let tryFindStations (sentence: string) = 
        let reg = new Regex("\\s+")
        let words = reg.Split(preProcess sentence)
                
        let getClosestStation input index length = 
            let computeDistance (input: string) (stationNames: string list) =
                stationNames |> List.map(fun s -> Levenshtein.relativeDistance input s) |> List.min
                        
            match stationMap |> Map.tryFind input with
            | Some(x) ->  {StationName = x.Station.Libelle; 
                Code = x.Station.Code_uic; 
                Index = index; 
                Value = 0.;
                Length = length;}
            | None -> stationPatterns |> List.map (fun s -> 
                    {StationName = s.Station.Libelle; 
                    Code = s.Station.Code_uic; 
                    Index = index; 
                    Value = (computeDistance input s.Patterns);
                    Length = length;})
                    |> List.sortBy (fun r -> r.Value)
                    |> List.head
           
        let tryMatchStation s index length acc1 = 
             match (getClosestStation s index length) with
             | result when result.Value < 0.2 -> result :: acc1
             | _ -> acc1  
            
        let findStationsFromSentenceByIndex acc1 index length = 
            let s = String.Join(" ", words |> Seq.skip index |> Seq.take length |> Seq.toArray)
            tryMatchStation s index length acc1

        let findStationsFromSentence acc window =
            [0 .. words.Length - window] 
            |> List.fold (fun acc1 index -> findStationsFromSentenceByIndex acc1 index window) acc 
        
        let rec trimOverlapping acc results = 
            match results with
            | head :: tail -> trimOverlapping (head :: acc) (tail |> List.filter (fun r -> (r.Index + r.Length - 1) < head.Index || r.Index > (head.Index + head.Length - 1) ) )
            | [] -> acc

        [1..4] 
        |> List.fold (fun acc window -> findStationsFromSentence acc window) []
        |> List.sortBy (fun r -> r.Value) 
        |> trimOverlapping []
        |> List.sortBy (fun r -> r.Index)