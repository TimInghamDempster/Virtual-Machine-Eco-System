module Tokens

open System.Text.RegularExpressions

type TokenTypes =
    | BinaryOperator = 0
    | BooleanValue = 1
    | BooleanOperator = 2
    | BooleanConjunction = 3
    | Conditional = 4
    | OpenPeren = 5
    | ClosePeren = 6
    | OpenBrace = 7
    | CloseBrace = 8
    | Label = 9
    | Integer = 10
    | ASM = 11
    | Comment = 12
    | SemiColon = 13
    | Apostrophe = 14
    | SingleEquals = 15
    | Int = 16
    | Bool = 17
    | Invalid = 18


let patterns = [
    (TokenTypes.BinaryOperator, Regex("^\+"));
    (TokenTypes.BinaryOperator, Regex("^-"));
    (TokenTypes.BinaryOperator, Regex("^\*"));
    (TokenTypes.BinaryOperator, Regex("^/"));
    (TokenTypes.BooleanValue, Regex("^true"));
    (TokenTypes.BooleanValue, Regex("^false"));
    (TokenTypes.BooleanOperator, Regex("^=="));
    (TokenTypes.BooleanOperator, Regex("^!="));
    (TokenTypes.BooleanOperator, Regex("^>"));
    (TokenTypes.BooleanOperator, Regex("^<"));
    (TokenTypes.BooleanOperator, Regex("^>="));
    (TokenTypes.BooleanOperator, Regex("^<="));
    (TokenTypes.BooleanConjunction, Regex("^&&"));
    (TokenTypes.BooleanConjunction, Regex("^||"));
    (TokenTypes.Conditional, Regex("^if"));
    (TokenTypes.OpenPeren, Regex("^\("));
    (TokenTypes.ClosePeren, Regex("^\)"));
    (TokenTypes.OpenBrace, Regex("^{"));
    (TokenTypes.CloseBrace, Regex("^}"));
    (TokenTypes.Label, Regex("^[a-zA-Z_][\w_-]*"));
    (TokenTypes.Integer, Regex("^[0-9]+"));
    (TokenTypes.Integer, Regex("^0x[0-9A-Fa-f]+"));
    (TokenTypes.ASM, Regex("^(?s)<asm>(.*?)</asm>"));
    (TokenTypes.Comment, Regex("^(//).+"));
    (TokenTypes.SemiColon, Regex("^;"));
    (TokenTypes.Apostrophe, Regex("^'"));
    (TokenTypes.SingleEquals, Regex("^="));
    (TokenTypes.Int, Regex("^int"));
    (TokenTypes.Bool, Regex("^bool"));
]
