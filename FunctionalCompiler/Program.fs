open System

// Learn more about F# at http://fsharp.org

let tupleToString (token: Lexer.token) =
    token._tokenType.ToString() + ":" + Environment.NewLine + token._content +  Environment.NewLine

// Takes an ast node and recursively writes the
// tree into a string
let rec drawTree currentIndent baseIndent (rootNode : Parser.ASTNode) = 
    printfn "%s" (currentIndent + rootNode._nodeType.ToString())
    printfn "%s" (currentIndent + rootNode._content )
    rootNode._children |> List.iter (drawTree (currentIndent + baseIndent) baseIndent) |> ignore

[<EntryPoint>]
let main argv =
    let source = ReadSource.sourceCode argv.[0]
    let tokenList = Lexer.tokens source 0
    (* Uncomment to write the token stream to the console:*)
    let tokenStringList =
        List.map tupleToString tokenList
    let tokens = 
        List.reduce (+) tokenStringList
    printfn "%s" tokens
    let ast = Parser.parseProgram tokenList
    drawTree " " " " ast |> ignore
    0 // return an integer exit code
