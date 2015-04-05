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


    [<Test>]
    member this.TestChateletRoissyCharlesDeGaulle() =
        let chrono = Stopwatch.StartNew()
        let res = Matcher.tryFindStations("chatelet les halles roissy charles de gaulle")
        Assert.AreEqual(2, res.Length)
        Assert.AreEqual(87758607, res.Head.Code)
        Assert.AreEqual(87001479, res.Tail.Head.Code)
        printfn "time = %dms" chrono.ElapsedMilliseconds 