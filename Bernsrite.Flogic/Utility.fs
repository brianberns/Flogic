﻿namespace Bernsrite.Flogic

open System

/// http://fssnip.net/6C/title/Permutation-and-Combination
module List =

    type ListBuilder() =
        member _.Bind(lst, f) = List.collect f lst
        member _.Return(x) = [x]
        member _.ReturnFrom(x) = x
        member _.Zero() = []
        member _.Combine(a, b) = a @ b
        member _.Delay(f) = f ()

    let list = ListBuilder()

    /// Permutations of N items taken from the given list.
    let rec permutations n lst = 

        let rec selections = function
            | [] -> []
            | x::xs ->
                (x, xs) :: list {
                    let! y, ys = selections xs 
                    return y, x::ys
                }

        match (n, lst) with
            | 0, _ -> [[]]
            | _, [] -> []
            | _, x::[] -> [[x]]
            | n, xs ->
                list {
                    let! y, ys = selections xs
                    let! zs = permutations (n-1) ys 
                    return y::zs
                }

    /// Combinations of N items taken from the given list.
    let rec combinations n lst = 

        let rec findChoices = function 
            | [] -> [] 
            | x::xs ->
                (x, xs) :: list {
                    let! y, ys = findChoices xs
                    return y, ys
                }

        list {
            if n = 0 then return! [[]]
            else
                let! z, r = findChoices lst
                let! zs = combinations (n-1) r 
                return z::zs
        }

    /// http://www.fssnip.net/2A/title/Cartesian-product-of-n-lists
    /// Takes an input like [[1;2;5]; [3;4]; [6;7]] and returns
    /// [[5; 3; 7]; [2; 3; 7]; [1; 3; 7]; [5; 4; 7]; [2; 4; 7];
    /// [1; 4; 7]; [5; 3; 6]; [2; 3; 6]; [1; 3; 6]; [5; 4; 6];
    /// [2; 4; 6]; [1; 4; 6]]
    let rec cartesian = function
        | h::[] ->
            List.fold (fun acc elem -> [elem]::acc) [] h
        | h::t ->
            List.fold (fun cacc celem ->
                (List.fold (fun acc elem -> (elem::celem)::acc) [] h) @ cacc
                ) [] (cartesian t)
        | _ -> []

/// https://stackoverflow.com/questions/7818277/is-there-a-standard-option-workflow-in-f
type OptionBuilder() =
    member _.Bind(v, f) = Option.bind f v
    member _.Return(v) = Some v
    member _.ReturnFrom(o) = o
    member _.Zero() = None

[<AutoOpen>]
module OptionAutoOpen =

    /// Builder for option monad.
    let opt = OptionBuilder()

module Seq =

    /// Applies a function to each item in a sequence, short-circuiting
    /// if the function fails.
    let tryFold folder state source =
        let folder' stateOpt item =
            stateOpt
                |> Option.bind (fun state ->
                    folder state item)
        Seq.fold folder' (Some state) source

module String =

    /// Concatenates the given items in a string, using the specified
    /// separator between each one.    
    let join separator (items : seq<_>) =
        let strs =
            items |> Seq.map (fun item -> item.ToString())
        String.Join(separator, strs)

module Map =

    /// Answers the set of keys present in the given map.
    let keys map =
        map
            |> Map.toSeq
            |> Seq.map fst
            |> set

module Print =

    /// Indents the given object to the given level.
    let indent level obj =
        sprintf "%s%s"
            (String(' ', 3 * level))
            (obj.ToString())

    /// ~(=(x,y)) -> x ~= y
    /// +(a,b,c) -> a + b = c
    let application (name : string) (args : _[]) isPositive =
        assert(name.Length > 0)
        match args.Length, Char.IsLetterOrDigit(name.[0]) with
            | 0, _ ->
                sprintf "%s%s"
                    (if isPositive then "" else "~")
                    name
            | 2, false ->
                sprintf "(%A %s%s %A)"
                    args.[0]
                    (if isPositive then "" else "~")
                    name
                    args.[1]
            | 3, false ->
                sprintf "(%A %s %A %s %A)"
                    args.[0]
                    name
                    args.[1]
                    (if isPositive then "=" else "~=")
                    args.[2]
            | _ ->
                sprintf "%s%s(%s)"
                    (if isPositive then "" else "~")
                    name
                    (args |> String.join ", ")


/// Interface for pretty printing.
type Printable =
    {
        Object : obj
        ToString : int (*level, 0-based*) -> string
    }
