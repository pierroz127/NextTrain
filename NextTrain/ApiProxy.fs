namespace NextTrain.Lib

open System.Configuration
open HttpClient
open FSharp.Data
open System.Globalization

type TrainSchedules = XmlProvider<"""<?xml version="1.0" encoding="UTF-8"?><passages gare="87381905"><train><date mode="R">03/12/2015 18:22</date><num>ODET78</num><miss>ODET</miss><term>87758375</term></train><train><date mode="R">03/12/2015 18:26</date><num>UGUI63</num><miss>UGUI</miss><term>87382655</term></train></passages>""", Culture="fr-FR">

module ApiProxy =
    let getDepartureUrl = sprintf "http://api.transilien.com/gare/%d/depart/"
    let getDepartureArrivalUrl = sprintf "http://api.transilien.com/gare/%d/depart/%d/"
    
    let apiUserName = ConfigurationManager.AppSettings.Item("sncfApiUserName")
    let apiPassword = ConfigurationManager.AppSettings.Item("sncfApiPassword")

    let sendTrainRequest count url =
        printfn "GET %s" url
        let body = 
            createRequest Get url
            |> withBasicAuthentication apiUserName apiPassword
            |> getResponseBody

        let trains = TrainSchedules.Parse(body)
        if trains.Trains.Length = 0
        then 
            printfn "nothing returned by sncf api"
            []
        else trains.Trains 
            |> Seq.sortBy (fun t -> t.Date.Value) 
            |> Seq.take count
            |> Seq.toList

    let nextTrain departureCode count = 
        getDepartureUrl departureCode
        |> sendTrainRequest count

    let nextTrainTo departureCode arrivalCode count = 
        getDepartureArrivalUrl departureCode arrivalCode
        |> sendTrainRequest count
            
        
