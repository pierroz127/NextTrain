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
