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
type BKTreeTest()=
    let assertSomeValue (expected: int) someValue = 
        match someValue with
        | None -> failwith "Expected some value"
        | Some(v) -> Assert.AreEqual(expected, v.dist)

    let assertNone (result: Match option) = 
        function 
        | Some(v) -> failwith "Expected none and get some value"
        | None -> Assert.IsTrue(true)
         
    [<Test>]
    member this.TestFindSomeValue() = 
        let patterns = [ "sartrouville"; "cergy"; "achères"; "saint-germain"; "poissy"; "houilles"; "conflans" ]
        let bktree = BKTree(patterns);
        assertSomeValue 1 (bktree.findClosest "sirtrouville" 3)
        assertSomeValue 1 (bktree.findClosest "cregy" 3)
        assertSomeValue 2 (bktree.findClosest "cinfalns" 3)
        
    [<Test>]
    member this.TestFindNone() = 
        let patterns = [ "sartrouville"; "cergy"; "achères"; "saint-germain"; "poissy"; "houilles"; "conflans" ]
        let bktree = BKTree(patterns);
        assertNone (bktree.findClosest "ygerc" 2) |> ignore
