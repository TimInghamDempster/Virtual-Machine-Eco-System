module Parser

type ASTNodeType =
    | test = 0
    | test2 = 1

type ASTNode =
    {
        _nodeType: ASTNodeType;
        _content: string;
        _children: ASTNode list
    }

let testInnerNode =
    {
        _nodeType = ASTNodeType.test;
        _content = "inner content";
        _children = [];
    }

let start =
    {
        _nodeType = ASTNodeType.test2;
        _content = "content";
        _children = [testInnerNode; testInnerNode];
    }
