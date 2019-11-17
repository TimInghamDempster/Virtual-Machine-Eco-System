module Terminals

open System.Text.RegularExpressions

let patterns = [
    ("BinaryOperator",Regex("^\+"));
    ("BinaryOperator",Regex("^-"));
    ("BinaryOperator",Regex("^\*"));
    ("BinaryOperator",Regex("^/"));
    ("BooleanValue",Regex("^true"));
    ("BooleanValue",Regex("^false"));
    ("BooleanOperator",Regex("^=="));
    ("BooleanOperator",Regex("^!="));
    ("BooleanOperator",Regex("^>"));
    ("BooleanOperator",Regex("^<"));
    ("BooleanOperator",Regex("^>="));
    ("BooleanOperator",Regex("^<="));
    ("BooleanConjunction",Regex("^&&"));
    ("BooleanConjunction",Regex("^||"));
    ("Conditional",Regex("^if"));
    ("OpenPeren",Regex("^\("));
    ("ClosePeren",Regex("^\)"));
    ("OpenBrace",Regex("^{"));
    ("CloseBrace",Regex("^}"));
    ("Label",Regex("^[a-zA-Z_][\w_-]*"));
    ("Integer",Regex("^[0-9]+"));
    ("Integer",Regex("^0x[0-9A-Fa-f]+"));
    ("ASM",Regex("^(?s)<asm>(.*?)</asm>"));
    ("Comment",Regex("^(//).+"));
    ("SemiColon",Regex("^;"));
    ("Apostrophe",Regex("^'"));
    ("SingleEquals",Regex("^="));
    ("Int",Regex("^int"));
    ("Bool",Regex("^bool"));
]
