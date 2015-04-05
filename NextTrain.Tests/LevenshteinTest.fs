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

    [<Test>]
    member this.TestDistanceRoissyCDG() =
        let res = Levenshtein.relativeDistance "Aeroport Charles de Gaulle 1 - Roissy" "Roissy Charles de Gaulle"
        printfn "res = %f" res