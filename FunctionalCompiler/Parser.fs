module Parser

type ASTNode<'NodeTypeType> =
    {
        _nodeType: 'NodeTypeType;
        _content: string;
        _children: ASTNode<'NodeTypeType> list
    }

let isRuleTerminal nodeType (patternDictionary : Map<'nodeTypeType, 'nodeTypeType list list>) =
    let patterns =
        patternDictionary.Item(nodeType)
    if patterns.Length = 1 && patterns.Head.Length = 1
    then true
    else false

let doesElementMatchToken elementType tokenType (tokenToAstMappings : Map<'tokenTypeType, 'astTypeType>) =
    tokenToAstMappings.Item(tokenType) = elementType

let isError (_, node) errorNodeType =
    node._nodeType = errorNodeType

let isErrorInList (l: list<(_* ASTNode<'ErrorNodeTyp>)>) errorNodeType =
    List.exists (fun (_, node) -> node._nodeType = errorNodeType) l

let CountConsumedTokens(l: list<(_* ASTNode<'ErrorNodeTyp>)>) =
    List.sumBy (fun (count, _) -> count) l

let selectLongest (countA, nodeA) (countB, nodeB) =
    if List.length nodeA._children >= List.length nodeB._children 
    then (countA, nodeA) 
    else (countB, nodeB)

let rec parseNode (tokens : list<Lexer.token<_>>) nodeType errorNodeType repeatNodeType tokenIndex (patternDictionary : Map<'nodeTypeType, 'nodeTypeType list list>) tokenToAstMappings =
    
    let patterns =
        patternDictionary.Item(nodeType)

    let matchElement elementType index =
        if isRuleTerminal elementType patternDictionary
        then
            if(List.length tokens > index)
            then
                let token =  tokens.Item(index  )
                let isCorrectToken = doesElementMatchToken elementType token._tokenType tokenToAstMappings
                if isCorrectToken
                then (1, {_nodeType = elementType; _content = token._content; _children = []})
                else (1, {_nodeType = errorNodeType; _content = "Expected " + elementType.ToString(); _children = []})
            else
                (1, {_nodeType = errorNodeType; _content = "Expected " + elementType.ToString(); _children = []})
        else parseNode tokens elementType errorNodeType repeatNodeType index patternDictionary tokenToAstMappings

    let rec matchPattern pattern index =
        match pattern with
        | [lastElement] -> [matchElement lastElement index]
        | head :: tail -> 
            if head = repeatNodeType
            then
                let nextRepeatElement = 
                    List.findIndex (fun elm -> elm = repeatNodeType) tail
                let repeatPattern =
                    List.take nextRepeatElement tail
                let restOfPattern =
                    List.skip (nextRepeatElement + 1) tail
                let rec repeat repeatStartIndex = 
                    let repeatNode = matchPattern repeatPattern (repeatStartIndex)
                    if isErrorInList repeatNode errorNodeType
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
                if isError headNode errorNodeType
                then [headNode]
                else continuation headNode
        | [] -> []

    let buildNode nodeType content childList =
        let nodeTypeWithError =
            if  childList |> List.exists (fun (_, node) -> node._nodeType = errorNodeType)
            then errorNodeType
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
            if isError headNode errorNodeType
            then
                let tailNode = findMatchingPattern tail
                (* If no patterns match at all then returning the pattern
                    most completed children is likely to be the one the user
                    intended, so we can have a relevant error message *)
                if isError tailNode errorNodeType
                then selectLongest headNode tailNode
                else tailNode
            else headNode
    findMatchingPattern patterns

let parseProgram tokens rootNodeType errorNodeType repeatNodeType patternDictionary tokenToAstMappings =
    let getTree (_, tree) =
        tree
    let parsedTree =
        parseNode tokens rootNodeType errorNodeType repeatNodeType 0 patternDictionary tokenToAstMappings
    getTree parsedTree
