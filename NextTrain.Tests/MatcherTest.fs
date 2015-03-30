namespace NextTrain.Tests

open NUnit.Framework
open NextTrain.Lib
open System.Diagnostics

[<TestFixture>]
type MatcherTest() = 
    [<Test>]
    member this.TestCergySaintChristophe() =
        let chrono = Stopwatch.StartNew()
        let r = Matcher.tryFindStations "cergy saint christophe"
        Assert.AreEqual(1, r.Length)
        Assert.AreEqual(87382499, r.Head.Code)
        printfn "time = %dms" chrono.ElapsedMilliseconds

    [<Test>]
    member this.CharleDeGaulleSainGermain() = 
        let chrono = Stopwatch.StartNew()
        let r = Matcher.tryFindStations "Charles de gaulle etoile saint germain en laye"
        Assert.AreEqual(2, r.Length)
        Assert.AreEqual(87758003, r.Head.Code)
        Assert.AreEqual(87758094, r.Tail.Head.Code)
        printfn "time = %dms" chrono.ElapsedMilliseconds

    [<Test>]
    member this.TestSaintLazare() = 
        let chrono = Stopwatch.StartNew()
        let r = Matcher.tryFindStations "saint-lazare"
        Assert.AreEqual(1, r.Length)
        Assert.AreEqual(87384008, r.Head.Code)
        printfn "time = %dms" chrono.ElapsedMilliseconds


    [<Test>]
    member this.TestCiteUGentilly() = 
        let chrono = Stopwatch.StartNew()
        let res = Matcher.tryFindStations("de la gare cite universitaire vers gentilly")
        Assert.AreEqual(2, res.Length)
        Assert.AreEqual(87758649, res.Head.Code)
        Assert.AreEqual(87758656, res.Tail.Head.Code)
        printfn "time = %dms" chrono.ElapsedMilliseconds

    [<Test>]
    member this.TestGareDeLyon() =
        let chrono = Stopwatch.StartNew()
        let res = Matcher.tryFindStations("gare de lyon")
        Assert.AreEqual(1, res.Length)
        Assert.AreEqual(87686030, res.Head.Code)
        printfn "time = %dms" chrono.ElapsedMilliseconds 
