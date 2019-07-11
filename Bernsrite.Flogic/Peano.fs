﻿namespace Bernsrite.Flogic

/// http://intrologic.stanford.edu/public/section.php?section=section_09_04
module Peano =

    let language =
        {
            Constants =
                [| Constant "0" |]
            Functions =
                [| Function ("s", 1) |]
            Predicates =
                [|
                    Predicate ("=", 2)
                    Predicate ("+", 3)
                    Predicate ("*", 3)
                |]
        }

    let parser =
        language |> Language.makeParser

    let private parse = Array.map (Parser.run parser)

    // http://intrologic.stanford.edu/public/section.php?section=section_11_06
    let equalsAxioms =
        parse [|
            "=(0,0)"
            "∀x.¬=(0,s(x))"
            "∀x.¬=(s(x),0)"
            "∀x.∀y.(=(x,y) ⇒ =(s(x),s(y)))"
            "∀x.∀y.(=(s(x),s(y)) ⇒ =(x,y))"
        |]

    let plusAxioms =
        parse [|
            "∀y.+(0,y,y)"
            "∀x.∀y.∀z.(+(x,y,z) ⇒ +(s(x),y,s(z)))"
            "∀x.∀y.∀z.∀w.((+(x,y,z) ∧ ¬=(z,w)) ⇒ ¬+(x,y,w))"
        |]

    let timesAxioms =
        parse [|
            "∀y.*(0,y,0)"
            "∀x.∀y.∀z.∀w.((*(x,y,z) ∧ +(y,z,w)) ⇒ *(s(x),y,w))"
            "∀x.∀y.∀z.∀w.((*(x,y,z) ∧ ¬=(z,w)) ⇒ ¬*(x,y,w))"
        |]

    let axioms =
        [|
            yield! equalsAxioms
            yield! plusAxioms
            yield! timesAxioms
        |]
