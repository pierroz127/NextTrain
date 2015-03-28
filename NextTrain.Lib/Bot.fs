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
    let departureNoResponseTemplate = sprintf "Désolé, je n'ai aucun horaire prévu pour un procahin train départ de %s" 

    let getTime (dt: DateTime) = dt.ToString("le dd/MM/yyyy à HH:mm")
        
    let processRequest config query = 
        let processDepartureArrival (departure: Matcher.Result) (arrival: Matcher.Result) =
            printfn "next train %s %s" departure.StationName arrival.StationName
            match ApiProxy.nextTrainTo config departure.Code arrival.Code 3 with
            | [] -> printfn "0"; Some(arrivalDepartureNoResponseTemplate departure.StationName arrival.StationName)
            | [x] -> printfn "1"; Some(arrivalDepartureSingleTemplate departure.StationName arrival.StationName (getTime x.Date.Value))
            | times ->
                printfn "beaucoup"
                let intro = arrivalDeparturePluralIntroTemplate times.Length departure.StationName arrival.StationName
                let schedule = String.Join("\n", times |> List.map(fun x -> getTime x.Date.Value) |> List.toArray)
                Some(intro + schedule)
                    
        let processDeparture (departure: Matcher.Result) =  
            printfn "next train %s" departure.StationName
            match ApiProxy.nextTrain config departure.Code 3 with
            | [] -> Some(departureNoResponseTemplate departure.StationName)
            | [x] -> Some(departureSingleTemplate departure.StationName (Station.getNameFromUicCode x.Term) (getTime x.Date.Value))
            | times -> 
                let intro = departurePluralIntroTemplate times.Length departure.StationName
                let schedules = String.Join("\n", times |> List.map(fun x -> departureScheduleTemplate (Station.getNameFromUicCode x.Term) (getTime x.Date.Value)))
                Some(intro + schedules)
        
        match Matcher.tryFindStations query with
        | [r1;r2] -> processDepartureArrival r1 r2 
        | [r] -> processDeparture r
        | _ -> None

        

