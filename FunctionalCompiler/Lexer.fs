module Lexer

open System.Text.RegularExpressions

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
        List.filter (fun token -> not(System.String.Equals(token._content,""))) matches
    if filteredMatches.IsEmpty
        then {_tokenType =Tokens.TokenTypes.Invalid; _content = ""}
    else
        List.reduce longer filteredMatches 

let rec tokens source index = 
    if index > source.ToString().Length
        then  []
        else
            let token = nextToken source index
            match token._tokenType with
            | Tokens.TokenTypes.Invalid -> tokens source (index + 1)
            | _ -> token :: tokens source (index + token._content.Length)
   
    