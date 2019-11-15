// Learn more about F# at http://fsharp.org

open System

[<EntryPoint>]
let main argv =
    let source = ReadSource.sourceCode argv.[0]
    let tokenList = Lexer.tokens source 0
    let tokens =
        List.reduce (+) tokenList
    printfn "%s" tokens
    0 // return an integer exit code
