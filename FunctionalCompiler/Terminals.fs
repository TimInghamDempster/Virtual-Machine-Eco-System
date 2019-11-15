module Terminals

open System.Text.RegularExpressions

let patterns = [Regex("^int"); Regex("^;"); Regex("^bool"); Regex("^="); Regex("^\+"); Regex("^-"); Regex("^\*"); Regex("^/"); Regex("^true"); Regex("^false"); Regex("^=="); Regex("^!="); Regex("^>"); Regex("^<"); Regex("^>="); Regex("^<="); Regex("^&&"); Regex("^||"); Regex("^if"); Regex("^\("); Regex("^\)"); Regex("^{"); Regex("^}"); Regex("^[a-zA-Z_][\w_-]*"); Regex("^[0-9]+"); Regex("^0x[0-9A-Fa-f]+"); Regex("^(?s)<asm>(.*?)</asm>"); Regex("^(//).+"); ]
