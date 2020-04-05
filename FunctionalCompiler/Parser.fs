module Parser

type ASTNode =
    {
        _nodeType: Parser_Generated.ASTNodeType;
        _content: string;
        _children: ASTNode list
    }

let isRuleTerminal nodeType =
    let patterns =
        Parser_Generated.patterns.Item(nodeType)
    if patterns.Length = 1 && patterns.Head.Length = 1
    then true
    else false

let doesElementMatchToken elementType tokenType =
    Parser_Generated.terminalMappings.Item(tokenType) = elementType

let IsError (_, node) =
    node._nodeType = Parser_Generated.ASTNodeType.Error

let IsErrorInList (l: list<(_* ASTNode)>) =
    List.exists (fun (_, node) -> node._nodeType = Parser_Generated.ASTNodeType.Error) l

let CountConsumedTokens(l: list<(_* ASTNode)>) =
    List.sumBy (fun (count, _) -> count) l

let selectLongest (countA, nodeA) (countB, nodeB) =
    if List.length nodeA._children >= List.length nodeB._children 
    then (countA, nodeA) 
    else (countB, nodeB)

let rec parseNode (tokens : list<Lexer.token>) nodeType tokenIndex =
    let patterns =
        Parser_Generated.patterns.Item(nodeType)

    let matchElement elementType index =
        if isRuleTerminal elementType
        then
            let token =  tokens.Item(index)
            let isCorrectToken = doesElementMatchToken elementType token._tokenType
            if isCorrectToken
            then (1, {_nodeType = elementType; _content = token._content; _children = []})
            else (1, {_nodeType = Parser_Generated.ASTNodeType.Error; _content = "Expected " + elementType.ToString(); _children = []})
        else parseNode tokens elementType index

    let rec matchPattern pattern index =
        match pattern with
        | [lastElement] -> [matchElement lastElement index]
        | head :: tail -> 
            if head = Parser_Generated.ASTNodeType.Repeat
            then
                let nextRepeatElement = 
                    List.findIndex (fun elm -> elm = Parser_Generated.ASTNodeType.Repeat) tail
                let repeatPattern =
                    List.take nextRepeatElement tail
                let restOfPattern =
                    List.skip (nextRepeatElement + 1) tail
                let rec repeat repeatStartIndex = 
                    let repeatNode = matchPattern repeatPattern (repeatStartIndex)
                    if IsErrorInList repeatNode
                    then []
                    else List.append repeatNode (repeat (repeatStartIndex + (CountConsumedTokens repeatNode)))
                let repeatContinuation nodesSoFar =
                    List.append nodesSoFar (matchPattern restOfPattern (index + (CountConsumedTokens nodesSoFar)))
                repeatContinuation (repeat index)
            else 
                let headNode =
                    matchElement head index
                let continuation (count, node) =
                   (count, node) :: matchPattern tail (index + count)
                (* VERY important, fail as soon as we find an error in a pattern, otherwise
                   recursive definitions will just keep recursing *)
                if IsError headNode
                then [headNode]
                else continuation headNode
        | [] -> []

    let buildNode nodeType content childList =
        let nodeTypeWithError =
            if  childList |> List.exists (fun (_, node) -> node._nodeType = Parser_Generated.ASTNodeType.Error)
            then Parser_Generated.ASTNodeType.Error
            else nodeType
        let tokensConsumed =
            List.reduce (+) (List.map (fun (count, _) -> count) childList)
        let node =
            { _nodeType = nodeTypeWithError; _content = content; _children = childList |> List.map (fun (_, node) -> node)}
        (tokensConsumed, node)

    let rec findMatchingPattern patternList =
        match patternList with
        | [head] -> 
            let children = matchPattern head tokenIndex
            buildNode nodeType "" children
        | head :: tail ->
            let headChildren = matchPattern head tokenIndex
            let headNode = buildNode nodeType "" headChildren
            if IsError headNode
            then
                let tailNode = findMatchingPattern tail
                (* If no patterns match at all then returning the pattern
                   most completed children is likely to be the one the user
                   intended, so we can have a relevant error message *)
                if IsError tailNode
                then selectLongest headNode tailNode
                else tailNode
            else headNode
            
    printfn "%i" tokenIndex
    findMatchingPattern patterns

let parseProgram tokens =
    let getTree (_, tree) =
        tree
    let parsedTree =
        parseNode tokens Parser_Generated.ASTNodeType.Program 0
    getTree parsedTree
