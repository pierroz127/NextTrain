namespace NextTrain.Tests

open NUnit.Framework
open NextTrain.Lib

[<TestFixture>]
type LevenshteinTest() = 
    
    [<Test>]
    member this.TestAbsoluteDistance() = 
        Assert.AreEqual(1, (Levenshtein.absoluteDistance "tutu" "tuto"))
        Assert.AreEqual(2, (Levenshtein.absoluteDistance "tutu" "toto"))
        Assert.AreEqual(1, (Levenshtein.absoluteDistance "tutu" "tuut"))

    [<Test>]
    member this.TestRelativeDistance() = 
        Assert.AreEqual(0.25, (Levenshtein.relativeDistance "tutu" "tuto"))