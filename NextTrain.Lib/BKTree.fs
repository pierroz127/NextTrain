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

open NextTrain.Lib

type Tree = 
| Node of string * int * list<Tree>
| EmptyNode

type Match = { dist: int; value: string; }

type BKTree(patterns) = 
    let rec addToNode t x = 
        let rec addToChildren x d nodes acc = 
           match nodes with 
           | head :: tail -> 
                match head with 
                | Node(s1, w1, nodesChildren1) when w1 = d -> (addToNode head x) :: (acc @ tail)
                | Node(s2, w2, nodesChildren2) -> addToChildren x d tail (head :: acc)
                | EmptyNode -> acc
           | [] -> Node(x, d, []) :: acc

        match t with
        | Node(s, w, children) -> 
            let d = Levenshtein.absoluteDistance s x
            Node(s, w, addToChildren x d children [])
        | EmptyNode -> Node(x, 0, [])
     
    let tree = patterns |> List.fold(addToNode) EmptyNode 
    
    member this.prettyPrint() =
        let rec printNode depth node = 
            printf "|"
            [0 .. depth] |> List.map (fun i -> printf "--") |> ignore
            match node with
            | EmptyNode -> printfn "[]"
            | Node(s, w, children) -> printfn " %s (%d)" s w; children |> List.map(printNode (depth + 1)) |> ignore
        printNode 0 tree

    member this.findClosest input threshold = 
        //printfn "find closest pattern to %s with threshold %d" input threshold
        let rec findClosestNode node = 
            match node with
            | EmptyNode -> None
            | Node(s, _, children) -> 
                let d = Levenshtein.absoluteDistance s input
                //printfn "d(%s,%s) = %d" input s d
                if d = 0 then Some({dist=0; value=s}) 
                else
                    let cur = if d < threshold then Some({dist=d; value=s}) else None
                    let minDist = d - threshold
                    let maxDist = d + threshold
                    children 
                    |> List.filter (fun child -> 
                        match child with 
                        | Node(_, w, _) -> w >= minDist && w <= maxDist
                        | EmptyNode -> false)
                    |> List.fold(fun acc child -> 
                        let res = findClosestNode child
                        match acc, res with
                        | None, _ -> res
                        | _, None -> acc
                        | Some(r1), Some(r2) -> if r1.dist < r2.dist then acc else res) cur
        findClosestNode tree

            
