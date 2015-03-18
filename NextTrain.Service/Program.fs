// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open NextTrain
//open LinqToTwitter

[<EntryPoint>]
let main argv = 
    try
//        TwitterListener.listen()
//        |> List.map(fun tweet ->  printfn "From: %s" tweet.User.ScreenNameResponse; printfn "%s" tweet.Text)
//        |> ignore
        //let res = NextTrain.Levenshtein.absoluteDistance "toto" "tutu"
        let res = 0
        if res = 2 
        then printfn "Levenshtein.distance(toto, tutu) OK"
        else printfn "Levenshtein.distance(toto, tutu) NOK, expected 2 but get %d" res
    with
    | ex -> printfn "%s" (ex.ToString());      
    0

