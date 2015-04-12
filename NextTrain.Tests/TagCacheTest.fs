namespace NextTrain.Tests

open NUnit.Framework
open NextTrain.Lib

[<TestFixture>]
type TagCacheTest() =
    [<Test>]
    member this.TestAdd() = 
        let t = TagCache(Map.empty)
        let tt = t.add "pierre" (DepartureArrivalTrip("tag1", 123, 456))
        match tt.tryFindTrip "paul" "tag2" with
        | Some(trip) -> failwith "Expected None but get some value"
        | None -> Assert.IsTrue(true)

        match tt.tryFindTrip "pierre" "tag3" with
        | Some(trip) -> failwith "Expected None but get some value"
        | None -> Assert.IsTrue(true)

        match tt.tryFindTrip "pierre" "tag1" with
        | Some(trip) -> 
            match trip with
            | DepartureArrivalTrip(tag, departure, arrival) ->
                Assert.AreEqual("tag1", tag)
                Assert.AreEqual(123, departure)
                Assert.AreEqual(456, arrival)
            | _ -> failwith "DepartureArrival was expected in cache"
        | None -> failwith "Expected some value but get None"


    [<Test>]
    member this.TestSerialize() = 
        let t = TagCache(Map.empty)
        let tt = t.add "pierre" (DepartureArrivalTrip("tag1", 123, 456))
        let json = tt.toJson()
        printfn "%s" json
        Assert.AreEqual(@"{""pierre"":{""tag1"":{""Case"":""DepartureArrivalTrip"",""Fields"":[""tag1"",123,456]}}}", json)