module Lexer

open System.Text.RegularExpressions

let longer (a : string) (b : string) =
    if(b.Length > a.Length) then b else a

let nextToken (source : string) index = 
    let subSource = source.Substring(index)
    let rMatch (regex : Regex) = 
        let m = regex.Match (subSource)
        m.Value + "\n"
    let matches =
        List.map rMatch Terminals.patterns 
    let filteredMatches =
        List.filter (fun m -> not(System.String.Equals(m,"\n"))) matches
    if filteredMatches.IsEmpty
        then "\n"
    else
        List.reduce longer filteredMatches 

let rec tokens source index = 
    if index > source.ToString().Length
        then  []
        else
            let token = nextToken source index
            match token with
            |"\n" -> tokens source (index + token.Length)
            |_ -> token :: tokens source (index + token.Length - 1)
   
    