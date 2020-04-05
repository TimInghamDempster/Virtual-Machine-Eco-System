module Parser_Generated

type ASTNodeType =
    | Program = 0
    | CodeBlock = 1
    | Statement = 2
    | Declaration = 3
    | IntDeclaration = 4
    | BoolDeclaration = 5
    | Assignment = 6
    | IntAssignment = 7
    | BoolAssignment = 8
    | IntExpression = 9
    | Primitive = 10
    | Tag = 11
    | TagMarker = 12
    | BinaryOperator = 13
    | UnaryOperator = 14
    | BooleanValue = 15
    | BoolExpression = 16
    | BoolSubExpression = 17
    | BooleanOperator = 18
    | BooleanConjunction = 19
    | Conditional = 20
    | OpenPeren = 21
    | ClosePeren = 22
    | OpenBrace = 23
    | CloseBrace = 24
    | Integer = 25
    | DecimalInteger = 26
    | HexInteger = 27
    | ASM = 28
    | Comment = 29
    | Semicolon = 30
    | AssignmentToken = 31
    | Uninitialized = 32
    | Int = 33
    | Bool = 34
    | Plus = 35
    | Minus = 36
    | Mul = 37
    | Div = 38
    | True = 39
    | False = 40
    | Equality = 41
    | Inequality = 42
    | Greater = 43
    | Less = 44
    | GreaterEquals = 45
    | LessEquals = 46
    | LogicalAnd = 47
    | LogicalOr = 48
    | If = 49
    | Label = 50
    | Repeat = 51
    | Error = 52

let terminalMappings =
     Map.empty.
        Add(Tokens.TokenTypes.TagMarker, ASTNodeType.TagMarker).
        Add(Tokens.TokenTypes.OpenPeren, ASTNodeType.OpenPeren).
        Add(Tokens.TokenTypes.ClosePeren, ASTNodeType.ClosePeren).
        Add(Tokens.TokenTypes.OpenBrace, ASTNodeType.OpenBrace).
        Add(Tokens.TokenTypes.CloseBrace, ASTNodeType.CloseBrace).
        Add(Tokens.TokenTypes.DecimalInteger, ASTNodeType.DecimalInteger).
        Add(Tokens.TokenTypes.HexInteger, ASTNodeType.HexInteger).
        Add(Tokens.TokenTypes.ASM, ASTNodeType.ASM).
        Add(Tokens.TokenTypes.Comment, ASTNodeType.Comment).
        Add(Tokens.TokenTypes.Semicolon, ASTNodeType.Semicolon).
        Add(Tokens.TokenTypes.AssignmentToken, ASTNodeType.AssignmentToken).
        Add(Tokens.TokenTypes.Uninitialized, ASTNodeType.Uninitialized).
        Add(Tokens.TokenTypes.Int, ASTNodeType.Int).
        Add(Tokens.TokenTypes.Bool, ASTNodeType.Bool).
        Add(Tokens.TokenTypes.Plus, ASTNodeType.Plus).
        Add(Tokens.TokenTypes.Minus, ASTNodeType.Minus).
        Add(Tokens.TokenTypes.Mul, ASTNodeType.Mul).
        Add(Tokens.TokenTypes.Div, ASTNodeType.Div).
        Add(Tokens.TokenTypes.True, ASTNodeType.True).
        Add(Tokens.TokenTypes.False, ASTNodeType.False).
        Add(Tokens.TokenTypes.Equality, ASTNodeType.Equality).
        Add(Tokens.TokenTypes.Inequality, ASTNodeType.Inequality).
        Add(Tokens.TokenTypes.Greater, ASTNodeType.Greater).
        Add(Tokens.TokenTypes.Less, ASTNodeType.Less).
        Add(Tokens.TokenTypes.GreaterEquals, ASTNodeType.GreaterEquals).
        Add(Tokens.TokenTypes.LessEquals, ASTNodeType.LessEquals).
        Add(Tokens.TokenTypes.LogicalAnd, ASTNodeType.LogicalAnd).
        Add(Tokens.TokenTypes.LogicalOr, ASTNodeType.LogicalOr).
        Add(Tokens.TokenTypes.If, ASTNodeType.If).
        Add(Tokens.TokenTypes.Label, ASTNodeType.Label)

let patterns =
     Map.empty.
        Add(ASTNodeType.Program,[[ASTNodeType.CodeBlock;]]).
        Add(ASTNodeType.CodeBlock,[[ASTNodeType.OpenBrace; ASTNodeType.Repeat; ASTNodeType.Statement; ASTNodeType.Repeat; ASTNodeType.CloseBrace;]]).
        Add(ASTNodeType.Statement,[[ASTNodeType.Declaration;];[ASTNodeType.Assignment;];[ASTNodeType.ASM;];[ASTNodeType.Conditional;];[ASTNodeType.Tag; ASTNodeType.Semicolon;];[ASTNodeType.Comment;]]).
        Add(ASTNodeType.Declaration,[[ASTNodeType.IntDeclaration;];[ASTNodeType.BoolDeclaration;]]).
        Add(ASTNodeType.IntDeclaration,[[ASTNodeType.Int; ASTNodeType.Repeat; ASTNodeType.Uninitialized; ASTNodeType.Repeat; ASTNodeType.Label; ASTNodeType.Semicolon;]]).
        Add(ASTNodeType.BoolDeclaration,[[ASTNodeType.Bool; ASTNodeType.Repeat; ASTNodeType.Uninitialized; ASTNodeType.Repeat; ASTNodeType.Label; ASTNodeType.Semicolon;]]).
        Add(ASTNodeType.Assignment,[[ASTNodeType.IntAssignment;];[ASTNodeType.BoolAssignment;]]).
        Add(ASTNodeType.IntAssignment,[[ASTNodeType.Label; ASTNodeType.AssignmentToken; ASTNodeType.IntExpression; ASTNodeType.Semicolon;]]).
        Add(ASTNodeType.BoolAssignment,[[ASTNodeType.Label; ASTNodeType.AssignmentToken; ASTNodeType.BoolExpression; ASTNodeType.Semicolon;]]).
        Add(ASTNodeType.IntExpression,[[ASTNodeType.Primitive; ASTNodeType.Repeat; ASTNodeType.BinaryOperator; ASTNodeType.Primitive; ASTNodeType.Repeat;]]).
        Add(ASTNodeType.Primitive,[[ASTNodeType.Integer;];[ASTNodeType.OpenPeren; ASTNodeType.IntExpression; ASTNodeType.ClosePeren;];[ASTNodeType.UnaryOperator; ASTNodeType.Primitive;];[ASTNodeType.Label;];[ASTNodeType.Tag;]]).
        Add(ASTNodeType.Tag,[[ASTNodeType.TagMarker; ASTNodeType.OpenPeren; ASTNodeType.Label; ASTNodeType.ClosePeren;]]).
        Add(ASTNodeType.TagMarker,[[ASTNodeType.TagMarker;]]).
        Add(ASTNodeType.BinaryOperator,[[ASTNodeType.Plus;];[ASTNodeType.Minus;];[ASTNodeType.Mul;];[ASTNodeType.Div;]]).
        Add(ASTNodeType.UnaryOperator,[[ASTNodeType.Minus;]]).
        Add(ASTNodeType.BooleanValue,[[ASTNodeType.True;];[ASTNodeType.False;]]).
        Add(ASTNodeType.BoolExpression,[[ASTNodeType.BoolSubExpression; ASTNodeType.Repeat; ASTNodeType.BooleanConjunction; ASTNodeType.BoolSubExpression; ASTNodeType.Repeat;]]).
        Add(ASTNodeType.BoolSubExpression,[[ASTNodeType.BooleanValue;];[ASTNodeType.Primitive; ASTNodeType.BooleanOperator; ASTNodeType.Primitive;];[ASTNodeType.OpenPeren; ASTNodeType.BoolExpression; ASTNodeType.ClosePeren;]]).
        Add(ASTNodeType.BooleanOperator,[[ASTNodeType.Equality;];[ASTNodeType.Inequality;];[ASTNodeType.Greater;];[ASTNodeType.Less;];[ASTNodeType.GreaterEquals;];[ASTNodeType.LessEquals;]]).
        Add(ASTNodeType.BooleanConjunction,[[ASTNodeType.LogicalAnd;];[ASTNodeType.LogicalOr;]]).
        Add(ASTNodeType.Conditional,[[ASTNodeType.If; ASTNodeType.OpenPeren; ASTNodeType.BoolExpression; ASTNodeType.ClosePeren; ASTNodeType.CodeBlock;]]).
        Add(ASTNodeType.OpenPeren,[[ASTNodeType.OpenPeren;]]).
        Add(ASTNodeType.ClosePeren,[[ASTNodeType.ClosePeren;]]).
        Add(ASTNodeType.OpenBrace,[[ASTNodeType.OpenBrace;]]).
        Add(ASTNodeType.CloseBrace,[[ASTNodeType.CloseBrace;]]).
        Add(ASTNodeType.Integer,[[ASTNodeType.DecimalInteger;];[ASTNodeType.HexInteger;]]).
        Add(ASTNodeType.DecimalInteger,[[ASTNodeType.DecimalInteger;]]).
        Add(ASTNodeType.HexInteger,[[ASTNodeType.HexInteger;]]).
        Add(ASTNodeType.ASM,[[ASTNodeType.ASM;]]).
        Add(ASTNodeType.Comment,[[ASTNodeType.Comment;]]).
        Add(ASTNodeType.Semicolon,[[ASTNodeType.Semicolon;]]).
        Add(ASTNodeType.AssignmentToken,[[ASTNodeType.AssignmentToken;]]).
        Add(ASTNodeType.Uninitialized,[[ASTNodeType.Uninitialized;]]).
        Add(ASTNodeType.Int,[[ASTNodeType.Int;]]).
        Add(ASTNodeType.Bool,[[ASTNodeType.Bool;]]).
        Add(ASTNodeType.Plus,[[ASTNodeType.Plus;]]).
        Add(ASTNodeType.Minus,[[ASTNodeType.Minus;]]).
        Add(ASTNodeType.Mul,[[ASTNodeType.Mul;]]).
        Add(ASTNodeType.Div,[[ASTNodeType.Div;]]).
        Add(ASTNodeType.True,[[ASTNodeType.True;]]).
        Add(ASTNodeType.False,[[ASTNodeType.False;]]).
        Add(ASTNodeType.Equality,[[ASTNodeType.Equality;]]).
        Add(ASTNodeType.Inequality,[[ASTNodeType.Inequality;]]).
        Add(ASTNodeType.Greater,[[ASTNodeType.Greater;]]).
        Add(ASTNodeType.Less,[[ASTNodeType.Less;]]).
        Add(ASTNodeType.GreaterEquals,[[ASTNodeType.GreaterEquals;]]).
        Add(ASTNodeType.LessEquals,[[ASTNodeType.LessEquals;]]).
        Add(ASTNodeType.LogicalAnd,[[ASTNodeType.LogicalAnd;]]).
        Add(ASTNodeType.LogicalOr,[[ASTNodeType.LogicalOr;]]).
        Add(ASTNodeType.If,[[ASTNodeType.If;]]).
        Add(ASTNodeType.Label,[[ASTNodeType.Label;]])
