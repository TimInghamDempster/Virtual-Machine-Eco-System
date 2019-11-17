module Lexer

open System.Text.RegularExpressions

let getLen (tuple: (string*string)) =
    let secondString = snd tuple
    secondString.Length

let longer (a : (string*string)) (b : (string*string)) =
    if(getLen b > getLen a) then b else a

let nextToken (source : string) index = 
    let subSource = source.Substring(index)
    let rMatch (name: string, regex : Regex) = 
        let m = regex.Match (subSource)
        (name, m.Value)
    let matches =
        List.map rMatch Terminals.patterns 
    let filteredMatches =
        List.filter (fun m -> not(System.String.Equals(snd m,""))) matches
    if filteredMatches.IsEmpty
        then ("", "")
    else
        List.reduce longer filteredMatches 

let rec tokens source index = 
    if index > source.ToString().Length
        then  []
        else
            let token = nextToken source index
            match token with
            |("", "") -> tokens source (index + 1)
            |_ -> token :: tokens source (index + getLen token)
   
    