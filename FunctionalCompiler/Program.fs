// Learn more about F# at http://fsharp.org

open System

let tupleToString (token: Lexer.token) =
    token._tokenType.ToString() + ":\n" + token._content + "\n"

[<EntryPoint>]
let main argv =
    let source = ReadSource.sourceCode argv.[0]
    let tokenList = Lexer.tokens source 0
    let tokenStringList =
        List.map tupleToString tokenList
    let tokens = 
        List.reduce (+) tokenStringList
    printfn "%s" tokens
    0 // return an integer exit code
