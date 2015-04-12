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
    let tryFindStationsWithoutCache =  Matcher.tryFindStations (TagCache(Map.empty)) "test"
    [<Test>]
    member this.TestCergySaintChristophe() =
        let chrono = Stopwatch.StartNew()
        match tryFindStationsWithoutCache "cergy saint christophe" with
        | Departure(s) -> Assert.AreEqual(87382499, s.Code)
        | _ -> failwith "Departure was expected"
        printfn "time = %dms" chrono.ElapsedMilliseconds

    [<Test>]
    member this.CharleDeGaulleSainGermain() = 
        let chrono = Stopwatch.StartNew()
        match tryFindStationsWithoutCache "Charles de gaulle etoile saint germain en laye" with
        | DepartureArrival(d, a) -> 
            Assert.AreEqual(87758003, d.Code)
            Assert.AreEqual(87758094, a.Code)
        | _ -> failwith "DepartureArrival was expected"
        printfn "time = %dms" chrono.ElapsedMilliseconds

    [<Test>]
    member this.TestSaintLazare() = 
        let chrono = Stopwatch.StartNew()
        match tryFindStationsWithoutCache "saint-lazare" with
        | Departure(s) -> Assert.AreEqual(87384008, s.Code)
        | _ -> failwith "Departure was expected"
        
    [<Test>]
    member this.TestCiteUGentilly() = 
        let chrono = Stopwatch.StartNew()
        match tryFindStationsWithoutCache "de la gare cite universitaire vers gentilly" with
        | DepartureArrival(d, a) -> 
            Assert.AreEqual(87758649, d.Code)
            Assert.AreEqual(87758656, a.Code)
        | _ -> failwith "DepartureArrival was expected"
        printfn "time = %dms" chrono.ElapsedMilliseconds

    [<Test>]
    member this.TestGareDeLyon() =
        let chrono = Stopwatch.StartNew()
        match tryFindStationsWithoutCache "gare de lyon" with
        | Departure(s) -> Assert.AreEqual(87686030, s.Code)
        | _ -> failwith "Departure was expected"
        printfn "time = %dms" chrono.ElapsedMilliseconds 


    [<Test>]
    member this.TestChateletRoissyCharlesDeGaulle() =
        let chrono = Stopwatch.StartNew()
        match tryFindStationsWithoutCache "chatelet les halles roissy charles de gaulle" with
        | DepartureArrival(d, a) -> 
            Assert.AreEqual(87758607, d.Code)
            Assert.AreEqual(87001479, a.Code)
        | _ -> failwith "DepartureArrival was expected"
        printfn "time = %dms" chrono.ElapsedMilliseconds 

    [<Test>]
    member this.TestTaggedDeparture() = 
        match tryFindStationsWithoutCache "cergy saint christophe #CerSC" with
        | TaggedDeparture(tag, s) -> 
            Assert.AreEqual("CerSC", tag)
            Assert.AreEqual(87382499, s.Code)
        | _ -> failwith "TaggedDeparture was expected"

    [<Test>]
    member this.TestTaggedDepartureArrival() = 
        match tryFindStationsWithoutCache "auber cergy saint christophe #AubCerSC" with
        | TaggedDepartureArrival(tag, d, a) -> 
            Assert.AreEqual("AubCerSC", tag)
            Assert.AreEqual(87382499, a.Code)
            Assert.AreEqual(87758599, d.Code)
        | _ -> failwith "TaggedDeparture was expected"

    [<Test>]
    member this.TestGetDepartureFromCache() =
        let tagCache = TagCache(Map.empty).add "pierroz" (DepartureTrip("cersc", 87382499))
        match Matcher.tryFindStations tagCache "pierroz" "#cersc" with
        | Departure(s) -> Assert.AreEqual(87382499, s.Code)
        | _ -> failwith "Departure was expected"

        let tagCache2 = tagCache.add "John" (DepartureArrivalTrip("ceraub", 123, 345))
        match Matcher.tryFindStations tagCache2 "Bob" "#cersc" with
        | NoResult -> Assert.IsTrue(true)
        | _ -> failwith "No result was expected"


        match Matcher.tryFindStations tagCache2 "John" "#ceraub" with
        | DepartureArrival(d, a) -> 
            Assert.AreEqual(123, d.Code)
            Assert.AreEqual(345, a.Code)
        | _ -> failwith "DepartureArrival was expected"


