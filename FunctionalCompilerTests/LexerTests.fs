module LexerTests

open Xunit

(* Lexer is relatively straightforward so not much to test
    except for the special-case tokes*)
[<Fact>]
let HandlesASM () =
    let tokens =
        Lexer.tokens "<asm>int</asm>" 0
    Assert.True(List.length tokens = 1)
        
[<Fact>]
let HandlesComments () =
    let tokens =
        Lexer.tokens "\\int" 0
    Assert.True(List.length tokens = 1)
