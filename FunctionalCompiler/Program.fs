open System

// Learn more about F# at http://fsharp.org

let tupleToString (token: Lexer.token<_>) =
    token._tokenType.ToString() + ":" + Environment.NewLine + token._content +  Environment.NewLine

(* Takes an ast node and recursively writes the
   tree into a string *)
let rec drawTree currentIndent baseIndent (rootNode : Parser.ASTNode<Parser_Generated.ASTNodeType>) = 
    printfn "%s" (currentIndent + rootNode._nodeType.ToString())
    printfn "%s" (currentIndent + rootNode._content )
    rootNode._children |> List.iter (drawTree (currentIndent + baseIndent) baseIndent) |> ignore

[<EntryPoint>]
let main argv =
    let source = ReadSource.sourceCode argv.[0]
    let tokenList = Lexer.tokens source 0
    (* Uncomment to write token list to output
    let tokenStringList =
        List.map tupleToString tokenList
    let tokens = 
        List.reduce (+) tokenStringList
    printfn "%s" tokens*)
    let ast = Parser.parseProgram tokenList Parser_Generated.ASTNodeType.Program Parser_Generated.ASTNodeType.Error Parser_Generated.ASTNodeType.Repeat Parser_Generated.patterns Parser_Generated.terminalMappings
    drawTree " " " " ast |> ignore
    0 // return an integer exit code
