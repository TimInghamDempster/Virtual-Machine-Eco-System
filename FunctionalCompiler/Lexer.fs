module Lexer

open System.Text.RegularExpressions
open System

type token = {_tokenType: Tokens.TokenTypes; _content: string}

let longer (a : token) (b : token) =
    if(b._content.Length > a._content.Length) then b else a

let nextToken (source : string) index = 
    let subSource = source.Substring(index)
    let rMatch (tokenType: Tokens.TokenTypes, regex : Regex) = 
        let m = regex.Match (subSource)
        {_tokenType = tokenType; _content = m.Value}
    let matches =
        List.map rMatch Tokens.patterns 
    let filteredMatches =
        List.filter (fun token -> not(System.String.Equals(token._content, ""))) matches
    if filteredMatches.IsEmpty
        then {_tokenType = Tokens.TokenTypes.Invalid; _content = ""}
    (* The ASM and comment blocks aren't strictly regular so we can only
       pick up the openings with the lexer.  We could tokenize the closing
       tokens and then use the parser to build them, but we would have to
       handle lots of tokens, some of which overlap with real tokens, so
       easier just to special case those two items*)
    elif List.exists (fun tok -> tok._tokenType = Tokens.TokenTypes.Comment) filteredMatches
        then
            let nextLineBreakIndex = subSource.IndexOf(Environment.NewLine)
            let commentLine = subSource.Substring(0, nextLineBreakIndex)
            {_tokenType = Tokens.TokenTypes.Comment; _content = commentLine}
    elif List.exists (fun tok -> tok._tokenType = Tokens.TokenTypes.ASM) filteredMatches
        then
            let asmCloseTag = "</asm>"
            let endOfASMIndex = subSource.IndexOf(asmCloseTag) + asmCloseTag.Length
            let asmContent = subSource.Substring(0, endOfASMIndex)
            {_tokenType = Tokens.TokenTypes.ASM; _content = asmContent}
    else
        (* Assuming we have at least one token and it isn't a special case we just
           take the longest token.  In a tie for length we go by order in the EBNF
           definition.  This is why the label rule has to be last in the EBNF, it
           will match to most of the keywords so we want to prefer the keyword if
           it is the same length as the label*)
        List.reduce longer filteredMatches 

let rec tokens source index = 
    if index > source.ToString().Length
        then  []
        else
            let token = nextToken source index
            match token._tokenType with
            | Tokens.TokenTypes.Invalid -> tokens source (index + 1)
            | _ -> token :: tokens source (index + token._content.Length)
   
    