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

namespace NextTrain.Lib

module Levenshtein =
    let absoluteDistance (s1: string) (s2: string) = 
        let d = Array2D.zeroCreate (s1.Length + 1) (s2.Length + 1)
        [0 .. s1.Length] |> List.iter(fun i -> d.[i, 0] <- i) |> ignore
        [0 .. s2.Length] |> List.iter(fun j -> d.[0, j] <- j) |> ignore

        [1 .. s1.Length] 
        |> List.iter (fun i -> 
            [1 .. s2.Length]
            |> List.iter (fun j -> 
                let cost = if s1.[i-1] = s2.[j-1] then 0 else 1
                // deletion, insertion or substitution
                d.[i, j] <- ([ d.[i-1, j] + 1; d.[i, j-1] + 1; d.[i-1, j-1] + cost ] |> List.min)
                // transposition
                if (j > 1 && i > 1 && s1.[i-1] = s2.[j-2] && s1.[i-2] = s2.[j-1])
                then d.[i, j] <- ([ d.[i, j]; d.[i-2, j-2] + cost ] |> List.min)
                )
            )
        d.[s1.Length, s2.Length]
        
    let relativeDistance s1 s2 = 2.*(double)(absoluteDistance s1 s2)/ (double)(s1.Length + s2.Length)

