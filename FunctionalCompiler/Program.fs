// Learn more about F# at http://fsharp.org

let tupleToString (token: Lexer.token) =
    token._tokenType.ToString() + ":\n" + token._content + "\n"

// Takes an ast node and recursively writes the
// tree into a string
let rec drawTree indent (rootNode : Parser.ASTNode) = 
    let renderedChildren =
        List.map (drawTree (indent + indent)) rootNode._children
    let childrenString = 
        if renderedChildren.IsEmpty then ""
        else List.reduce (+) renderedChildren
    indent + rootNode._nodeType.ToString() + "\n" +
    indent + rootNode._content + "\n\n" +
    childrenString

[<EntryPoint>]
let main argv =
    let source = ReadSource.sourceCode argv.[0]
    let tokenList = Lexer.tokens source 0
    (* Uncomment to write the token stream to the console:
    let tokenStringList =
        List.map tupleToString tokenList
    let tokens = 
        List.reduce (+) tokenStringList
    printfn "%s" tokens*)
    let ast = Parser.start
    let tree = drawTree "    " ast
    printfn "%s" tree;
    0 // return an integer exit code
