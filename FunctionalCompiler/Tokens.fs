module Tokens

open System.Text.RegularExpressions

type TokenTypes =
    | TagMarker = 0
    | OpenPeren = 1
    | ClosePeren = 2
    | OpenBrace = 3
    | CloseBrace = 4
    | DecimalInteger = 5
    | HexInteger = 6
    | ASM = 7
    | Comment = 8
    | Semicolon = 9
    | AssignmentToken = 10
    | Uninitialized = 11
    | Int = 12
    | Bool = 13
    | Plus = 14
    | Minus = 15
    | Mul = 16
    | Div = 17
    | True = 18
    | False = 19
    | Equality = 20
    | Inequality = 21
    | Greater = 22
    | Less = 23
    | GreaterEquals = 24
    | LessEquals = 25
    | LogicalAnd = 26
    | LogicalOr = 27
    | If = 28
    | Label = 29
    | Invalid = 30


let patterns = [
    (TokenTypes.TagMarker, Regex("^Tag"));
    (TokenTypes.OpenPeren, Regex("^\("));
    (TokenTypes.ClosePeren, Regex("^\)"));
    (TokenTypes.OpenBrace, Regex("^{"));
    (TokenTypes.CloseBrace, Regex("^}"));
    (TokenTypes.DecimalInteger, Regex("^[0-9]+"));
    (TokenTypes.HexInteger, Regex("^0x[0-9]+"));
    (TokenTypes.ASM, Regex("^<asm>"));
    (TokenTypes.Comment, Regex("^//"));
    (TokenTypes.Semicolon, Regex("^;"));
    (TokenTypes.AssignmentToken, Regex("^="));
    (TokenTypes.Uninitialized, Regex("^'"));
    (TokenTypes.Int, Regex("^int"));
    (TokenTypes.Bool, Regex("^bool"));
    (TokenTypes.Plus, Regex("^\+"));
    (TokenTypes.Minus, Regex("^-"));
    (TokenTypes.Mul, Regex("^\*"));
    (TokenTypes.Div, Regex("^/"));
    (TokenTypes.True, Regex("^true"));
    (TokenTypes.False, Regex("^false"));
    (TokenTypes.Equality, Regex("^=="));
    (TokenTypes.Inequality, Regex("^!="));
    (TokenTypes.Greater, Regex("^>"));
    (TokenTypes.Less, Regex("^<"));
    (TokenTypes.GreaterEquals, Regex("^>="));
    (TokenTypes.LessEquals, Regex("^<="));
    (TokenTypes.LogicalAnd, Regex("^&&"));
    (TokenTypes.LogicalOr, Regex("^||"));
    (TokenTypes.If, Regex("^if"));
    (TokenTypes.Label, Regex("^[a-zA-Z_][\w_-]*"));
]
