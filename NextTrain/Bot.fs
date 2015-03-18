namespace NextTrain.Lib

open System
open NextTrain.Lib

module Bot =
    let arrivalDeparturePluralIntroTemplate  = sprintf "Les %d prochains %s - %s: \n"
    let arrivalDepartureSingleTemplate = sprintf "Le prochain %s - %s est prévu %s"
    let departurePluralIntroTemplate = sprintf "Les %d prochains trains au départ de %s:\n"
    let departureSingleTemplate = sprintf "Prochain train au départ de %s à direction de %s, prévu %s"
    let departureScheduleTemplate = sprintf "-%s, prévu %s"

    let getTime (dt: DateTime) = dt.ToString("le dd/MM/yyyy à HH:mm")
        
    let processRequest query = 
        let processDepartureArrival (departure: Matcher.Result) (arrival: Matcher.Result) =
            match ApiProxy.nextTrainTo departure.Code arrival.Code 3 with
            | [] -> None
            | [x] -> Some(arrivalDepartureSingleTemplate departure.StationName arrival.StationName (getTime x.Date.Value))
            | times ->
                let intro = arrivalDeparturePluralIntroTemplate times.Length departure.StationName arrival.StationName
                let schedule = String.Join("\n", times |> List.map(fun x -> getTime x.Date.Value) |> List.toArray)
                Some(intro + schedule)
                    
        let processDeparture (departure: Matcher.Result) =  
            match ApiProxy.nextTrain departure.Code 3 with
            | [] -> None
            | [x] -> Some(departureSingleTemplate departure.StationName (Station.getNameFromUicCode x.Term) (getTime x.Date.Value))
            | times -> 
                let intro = departurePluralIntroTemplate times.Length departure.StationName
                let schedules = String.Join("\n", times |> List.map(fun x -> departureScheduleTemplate (Station.getNameFromUicCode x.Term) (getTime x.Date.Value)))
                Some(intro + schedules)
        
        match Matcher.tryFindStations query with
        | [r1;r2] -> processDepartureArrival r1 r2 
        | [r] -> processDeparture r
        | _ -> None

        

