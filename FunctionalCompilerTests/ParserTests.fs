module ParserTests

open Xunit

type ASTNodeType =
    | Program = 0
    | Label = 1
    | Equality = 2
    | Semicolon = 3
    | Expression = 4
    | Error = 5
    | Repeat = 6

type TokenTypes =
    | Label = 0
    | Equality = 1
    | Semicolon = 2

let nestedPatterns =
    Map.empty.
        Add(ASTNodeType.Program, [[ASTNodeType.Label; ASTNodeType.Expression; ASTNodeType.Label]]).
        Add(ASTNodeType.Expression, [[ASTNodeType.Semicolon; ASTNodeType.Equality; ASTNodeType.Semicolon]]).
        Add(ASTNodeType.Label,[[ASTNodeType.Label;]]).
        Add(ASTNodeType.Equality,[[ASTNodeType.Equality;]]).
        Add(ASTNodeType.Semicolon,[[ASTNodeType.Semicolon;]])

let simplePatterns =
    Map.empty.
           Add(ASTNodeType.Program, [[ASTNodeType.Label; ASTNodeType.Equality; ASTNodeType.Semicolon]]).
           Add(ASTNodeType.Label,[[ASTNodeType.Label;]]).
           Add(ASTNodeType.Equality,[[ASTNodeType.Equality;]]).
           Add(ASTNodeType.Semicolon,[[ASTNodeType.Semicolon;]])

let patternsWithRepeats =
    Map.empty.
            Add(ASTNodeType.Program, [[ASTNodeType.Repeat; ASTNodeType.Equality; ASTNodeType.Semicolon; ASTNodeType.Repeat]]).
            Add(ASTNodeType.Equality,[[ASTNodeType.Equality;]]).
            Add(ASTNodeType.Semicolon,[[ASTNodeType.Semicolon;]])

let ruleWithMultiplePatterns =
    Map.empty.
            Add(ASTNodeType.Program, [[ASTNodeType.Label]; [ASTNodeType.Equality; ASTNodeType.Semicolon]]).
            Add(ASTNodeType.Label,[[ASTNodeType.Label;]]).
            Add(ASTNodeType.Equality,[[ASTNodeType.Equality;]]).
            Add(ASTNodeType.Semicolon,[[ASTNodeType.Semicolon;]])

let (labelToken : Lexer.token<TokenTypes>) =
     {_tokenType = TokenTypes.Label; _content = ""}

let (equalityToken : Lexer.token<TokenTypes>) =
    {_tokenType = TokenTypes.Equality; _content = ""}

let (semicolonToken : Lexer.token<TokenTypes>) =
     {_tokenType = TokenTypes.Semicolon; _content = ""}

let tokenAstMappings =
    Map.empty.
           Add(TokenTypes.Label, ASTNodeType.Label).
           Add(TokenTypes.Equality, ASTNodeType.Equality).
           Add(TokenTypes.Semicolon, ASTNodeType.Semicolon)

[<Fact>]
let HandlesSimplePattern () =
    let tokens =
         [labelToken; equalityToken; semicolonToken]
    let ast =
        Parser.parseProgram tokens ASTNodeType.Program ASTNodeType.Error ASTNodeType.Repeat simplePatterns tokenAstMappings
    Assert.False(ast._nodeType = ASTNodeType.Error);

[<Fact>]
let RejectsIncorrectPattern () =
    let tokens =
        [labelToken; semicolonToken]
    let ast =
        Parser.parseProgram tokens ASTNodeType.Program ASTNodeType.Error ASTNodeType.Repeat simplePatterns tokenAstMappings
    Assert.True(ast._nodeType = ASTNodeType.Error);     
    
[<Fact>]
let RejectsIncompletePattern () =
    let tokens =
        [labelToken; equalityToken]
    let ast =
        Parser.parseProgram tokens ASTNodeType.Program ASTNodeType.Error ASTNodeType.Repeat simplePatterns tokenAstMappings
    Assert.True(ast._nodeType = ASTNodeType.Error);     

[<Fact>]
let HandlesNestedPattern () =
    let tokens =
           [labelToken; semicolonToken; equalityToken; semicolonToken; labelToken]
    let ast =
        Parser.parseProgram tokens ASTNodeType.Program ASTNodeType.Error ASTNodeType.Repeat nestedPatterns tokenAstMappings
    Assert.True(List.length ast._children = 3)
    Assert.True(ast._children.Item(1)._nodeType = ASTNodeType.Expression)
    Assert.True(List.length (ast._children.Item(1)._children) = 3)

[<Fact>]
let HandlesPatternWithRepeats () =
    let tokens =
        [equalityToken; semicolonToken; equalityToken; semicolonToken; equalityToken; semicolonToken]
    let ast =
        Parser.parseProgram tokens ASTNodeType.Program ASTNodeType.Error ASTNodeType.Repeat patternsWithRepeats tokenAstMappings
    Assert.True(List.length ast._children = 6);     
       
[<Fact>]
let HandlesRuleWithMultiplePatterns () =
    let tokens =
          [equalityToken; semicolonToken;]
    let ast =
        Parser.parseProgram tokens ASTNodeType.Program ASTNodeType.Error ASTNodeType.Repeat ruleWithMultiplePatterns tokenAstMappings
    Assert.True(List.length ast._children = 2);   
           
[<Fact>]
let HandlesExpressionsWithPrecedence () =
    Assert.True(false);