﻿namespace Discover

open System
open FParsec

/// Shorthand for parser type.
type Parser<'t> = Parser<'t, unit>

module Parser =

    let special = set "(,)~&|<->∃∀."

    let isSpecial c =
        special.Contains(c)
            || Char.IsWhiteSpace(c)

    let parseName =
        many1Satisfy (isSpecial >> not)

    let parseVariable =
        parseName |>> Variable

    let parseTerm, parseTermRef =
        createParserForwardedToRef<Term, unit>()

    let parseTerms allowEmpty =
        let parsePresent =
            spaces
                >>. skipChar '('
                >>. sepBy1 parseTerm (skipChar ',' .>> spaces)
                .>> skipChar ')'
                |>> Seq.toArray
        if allowEmpty then
            attempt parsePresent
                <|> preturn Array.empty
        else
            parsePresent

    let parseApplication =
        pipe2 parseName (parseTerms false)
            (fun name (terms : _[]) ->
                Application (Function (name, terms.Length), terms))

    let parseFormula, parseFormulaRef =
        createParserForwardedToRef<Formula, unit>()

    let makeParser constants : Parser<_> =

        let parseConstant =
            let constantsSet = set constants
            parse {
                let! name = parseName
                if constantsSet.Contains(name) then
                    return Term.constant name
            }

        let parseTermActual =
            attempt parseApplication
                <|> attempt parseConstant
                <|> (parseVariable |>> Term)

        parseTermRef := parseTermActual

        let parseAtomic =
            pipe2 parseName (parseTerms true)
                (fun name (terms : _[]) ->
                    Formula (Predicate (name, terms.Length), terms))

        let parseUnary =
            skipChar '~'
                >>. parseFormula
                |>> Not

        let parseFormulaRaw =
            choice [
                attempt parseUnary
                parseAtomic
            ]

        let parseFormulaActual =
            let parseParenthesized =
                skipChar '('
                    >>. parseFormulaRaw
                    .>> skipChar ')'
            attempt parseParenthesized
                <|> parseFormulaRaw

        parseFormulaRef := parseFormulaActual
        parseFormulaActual

    /// Runs the given parser on the given string.
    let run parser str =
        let parser = parser .>> eof   // force consumption of entire string
        match run parser str with
            | Success (result, _, _) -> result
            | Failure (msg, _, _) -> failwith msg
