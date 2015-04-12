namespace NextTrain.Lib

open Newtonsoft.Json

type Trip =
| DepartureTrip of string * int
| DepartureArrivalTrip of string * int * int

type TagCache(map: Map<string, Map<string, Trip>>) =
    new() = TagCache(Map.empty)

    member this.add user trip = 
        let tag = match trip with DepartureTrip(t, _) | DepartureArrivalTrip(t, _, _) -> t
        let usermap =
            match map.TryFind user with
            | Some(m) -> m |> Map.add tag trip
            | None -> Map.empty |> Map.add tag trip
        TagCache(map |> Map.add user usermap)
        
    member this.tryFindTrip user tag = 
        match map.TryFind user with
        | Some(usermap) -> usermap.TryFind tag
        | None -> None  

    member this.toJson() =
        JsonConvert.SerializeObject(map)

    member this.fromJson json = 
        TagCache(JsonConvert.DeserializeObject<Map<string, Map<string, Trip>>>(json))