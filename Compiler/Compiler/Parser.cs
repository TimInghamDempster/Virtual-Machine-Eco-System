using System;
using System.Collections.Generic;

namespace Compiler
{
    enum ASTType
    {
        BinaryPlus,
        BinaryMinus,
        BinaryMul,
        BinaryDiv,
        UnaryMinus,
        IntExpression,
        Primitive,
        IntDecleration,
        UninitialisedIntDeclaration,
        Assignment,
        VariableName,
        ASM,
        Tag,
        Comment,
        BoolDecleration,
        UninitialisedBoolDeclaration,
        Conditional,
        BoolExpression,
        BoolSubExpression,
        CodeBlock,
        BoolValue,
    }

	class SyntaxNode
	{
		public List<SyntaxNode> Children { get; } = new List<SyntaxNode>();
		public ASTType Type { get; set; }
		public string Data { get; set; }

        public int StartTokenId { get; private set; }

        public SyntaxNode()
        {
            StartTokenId = Parser.TokenIndex;
        }
	}

	class Parser
	{
		List<Token> _tokenStream;
		public static int TokenIndex;

        private readonly ILogger _logger;

        public Parser(ILogger logger)
        {
            _logger = logger;
        }

		public bool Parse(List<Token> tokenStream, SyntaxNode programNode)
		{
			_tokenStream = tokenStream;

            return ParseCodeBlock(programNode);
		}

        private bool ParseCodeBlock(SyntaxNode programNode)
        {
            if (_tokenStream[TokenIndex].Type == TokenType.OpenBrace)
            {
                TokenIndex++;
                do
                {
                    if (_tokenStream[TokenIndex].Type == TokenType.CloseBrace)
                    {
                        TokenIndex++;
                        return true;
                    }
                } while (ParseStatement(programNode) || ParseComment(programNode));
            }

            return false;
        }

        private bool ParseComment(SyntaxNode programNode)
        {
            var token = _tokenStream[TokenIndex];
            if (token.Type == TokenType.Comment)
            {
                var commentNode = new SyntaxNode()
                {
                    Type =  ASTType.Comment,
                    Data = token.Data
                };

                programNode.Children.Add(commentNode);
                TokenIndex++;
                return true;
            }
            else
            {
                return false;
            }
        }

        bool ParseStatement(SyntaxNode parent)
        {
            int startIndex = TokenIndex;

            if(ParseDeclaration(parent) == true)
            {
                return true;
            }
            else
            {
                TokenIndex = startIndex;
                if(ParseAssignment(parent) == true)
                {
                    return true;
                }
                else
                {
                    TokenIndex = startIndex;
                    if (ParseASM(parent))
                    {
                        return true;
                    }
                    else
                    {
                        TokenIndex = startIndex;
                        if (ParseTag(parent, true))
                        {
                            return true;
                        }
                        else
                        {
                            TokenIndex = startIndex;
                            if (ParseConditional(parent))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            
            return false;
        }

        private bool ParseConditional(SyntaxNode parent)
        {
            if (_tokenStream[TokenIndex].Type != TokenType.If)
                return false;

            SyntaxNode conditionalNode = new SyntaxNode();
            TokenIndex++;
            if (_tokenStream[TokenIndex].Type != TokenType.OpenPerenthesis)
            {
                _logger.Log("Error: \"(\" expected");
                return false;
            }

            conditionalNode.Type = ASTType.Conditional;

            TokenIndex++;
            if (!ParseBoolExpression(conditionalNode))
            {
                _logger.Log("Error: if statement condition must evaluate to boolean value");
                return false;
            }

            if (_tokenStream[TokenIndex].Type != TokenType.ClosePerenthesis)
            {
                _logger.Log("Error: \")\" expected");
                return false;
            }

            TokenIndex++;
            var codeBlockNode = new SyntaxNode();
            if (!ParseCodeBlock(codeBlockNode))
            {
                _logger.Log("Error: if statement must be followed by a code block");
            }
            codeBlockNode.Type = ASTType.CodeBlock;
            conditionalNode.Children.Add(codeBlockNode);

            parent.Children.Add(conditionalNode);
            return true;
        }

        private bool ParseDeclaration(SyntaxNode parent)
        {
            if (ParseIntegerDeclaration(parent) == true)
            {
                return true;
            }
            else if (ParseBooleanDeclaration(parent) == true)
            {
                return true;
            }
            return false;
        }

        private bool ParseBooleanDeclaration(SyntaxNode parent)
        {
            bool initialise = true;
            string name = "";
            SyntaxNode declerationNode = new SyntaxNode();

            if (_tokenStream[TokenIndex].Type != TokenType.BoolDeclaration)
            {
                return false;
            }
            TokenIndex++;

            if (_tokenStream[TokenIndex].Type == TokenType.Prime)
            {
                initialise = false;
                TokenIndex++;
            }

            if (_tokenStream[TokenIndex].Type != TokenType.Label)
            {
                return false;
            }
            else
            {
                name = _tokenStream[TokenIndex].Data;
            }
            TokenIndex++;

            if (_tokenStream[TokenIndex].Type != TokenType.SemiColon)
            {
                Console.WriteLine("Error, semicolon expected");
                return false;
            }
            TokenIndex++;

            if (initialise == true)
            {
                declerationNode.Type = ASTType.BoolDecleration;
            }
            else
            {
                declerationNode.Type = ASTType.UninitialisedBoolDeclaration;
            }
            declerationNode.Data = name;
            parent.Children.Add(declerationNode);

            return true;
        }

        private bool ParseTag(SyntaxNode parent, bool isStatement)
        {
            if (_tokenStream[TokenIndex].Type == TokenType.Tag &&
                _tokenStream[TokenIndex + 1].Type == TokenType.OpenPerenthesis &&
                _tokenStream[TokenIndex + 2].Type == TokenType.Label &&
                _tokenStream[TokenIndex + 3].Type == TokenType.ClosePerenthesis)
            {
                var tagNode = new SyntaxNode()
                {
                    Type = ASTType.Tag,
                    Data = _tokenStream[TokenIndex + 2].Data
                };

                parent.Children.Add(tagNode);
                TokenIndex += 4;

                // A tag statement ends in a semicolon but a tag reference might not,
                // for a reference the semicolon will be handled by the statement it
                // appears in but for a tag statement we ARE the statement and need
                // to deal with it
                if (isStatement)
                {
                    if (_tokenStream[TokenIndex].Type == TokenType.SemiColon)
                    {
                        TokenIndex++;
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Error, missing semicolon");
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        private bool ParseIntegerDeclaration(SyntaxNode parent)
        {
            SyntaxNode declerationNode = new SyntaxNode();
            bool initialise = true;
            string name = "";

            if (_tokenStream[TokenIndex].Type != TokenType.IntDeclaration)
            {
                return false;
            }
            TokenIndex++;
            
            if (_tokenStream[TokenIndex].Type == TokenType.Prime)
            {
                initialise = false;
                TokenIndex++;
            }

            if (_tokenStream[TokenIndex].Type != TokenType.Label)
            {
                return false;
            }
            else
            {
                name = _tokenStream[TokenIndex].Data;
            }
            TokenIndex++;

            if (_tokenStream[TokenIndex].Type != TokenType.SemiColon)
            {
                Console.WriteLine("Error, semicolon expected");
                return false;
            }
            TokenIndex++;

            if (initialise == true)
            {
                declerationNode.Type = ASTType.IntDecleration;
            }
            else
            {
                declerationNode.Type = ASTType.UninitialisedIntDeclaration;
            }
            declerationNode.Data = name;
            parent.Children.Add(declerationNode);

            return true;
        }

        private bool ParseAssignment(SyntaxNode parent)
        {
            SyntaxNode assignmentNode = new SyntaxNode();
            string name = "";

            if (_tokenStream[TokenIndex].Type != TokenType.Label)
            {
                return false;
            }
            name = _tokenStream[TokenIndex].Data;
            TokenIndex++;

            if (_tokenStream[TokenIndex].Type != TokenType.Eqls)
            {
                return false;
            }
            TokenIndex++;

            assignmentNode.Type = ASTType.Assignment;
            assignmentNode.Data = name;

            if (!ParseIntExpression(assignmentNode, 0) && !ParseBoolExpression(assignmentNode))
            {
                return false;
            }

            if (_tokenStream[TokenIndex].Type != TokenType.SemiColon)
            {
                return false;
            }
            TokenIndex++;

            parent.Children.Add(assignmentNode);
            return true;
        }

        private bool ParseASM(SyntaxNode parent)
        {
            if(_tokenStream[TokenIndex].Type == TokenType.OpenASM)
            {
                SyntaxNode asmNode = new SyntaxNode();
                asmNode.Type = ASTType.ASM;
                asmNode.Data = _tokenStream[TokenIndex].Data;

                parent.Children.Add(asmNode);
                TokenIndex++;
                return true;
            }

            return false;
        }

        // BoolExpression => BooleanValue | Label
        private bool ParseBoolExpression(SyntaxNode parent)
        {
            SyntaxNode expressionNode = new SyntaxNode();
            expressionNode.Type = ASTType.BoolExpression;

            if (!ParseBoolSubExpression(expressionNode))
                return false;

            bool moreExpressions = _tokenStream[TokenIndex].Type == TokenType.BooleanConjunction;

            while (moreExpressions)
            {
                TokenIndex++;

                if (!ParseBoolSubExpression(expressionNode))
                {
                    _logger.Log("Error: boolean sub expression expected");
                    return false;
                }
                moreExpressions = _tokenStream[TokenIndex].Type == TokenType.BooleanConjunction;
            }

            parent.Children.Add(expressionNode); ;
            return true;
        }

        private bool ParseBoolSubExpression(SyntaxNode parent)
        {
            SyntaxNode expressionNode = new SyntaxNode();

            if (_tokenStream[TokenIndex].Type == TokenType.BooleanValue)
            {
                expressionNode.Data = _tokenStream[TokenIndex].Data;
                expressionNode.Type = ASTType.BoolValue;
                parent.Children.Add(expressionNode);
                TokenIndex++;
                return true;
            }
            else if (ParsePrimitive(expressionNode))
            {
                if (_tokenStream[TokenIndex].Type != TokenType.BooleanComparison)
                    return false;

                expressionNode.Data = _tokenStream[TokenIndex].Data;
                expressionNode.Type = ASTType.BoolSubExpression;

                TokenIndex++;

                if (!ParsePrimitive(expressionNode))
                {
                    _logger.Log("Error, value to compare to expected");
                }

                parent.Children.Add(expressionNode);
                return true;
            }
            else if (_tokenStream[TokenIndex].Type == TokenType.Label)
            {
                expressionNode.Type = ASTType.VariableName;
                expressionNode.Data = _tokenStream[TokenIndex].Data;
                parent.Children.Add(expressionNode);
                return true;
            }
            else if (_tokenStream[TokenIndex].Type == TokenType.OpenPerenthesis)
            {
                if (ParseBoolExpression(expressionNode))
                {
                    parent.Children.Add(expressionNode);
                    return true;
                }
            }
            return false;
        }

        bool ParseIntExpression(SyntaxNode parent, int minimumPrecedence)
		{
			SyntaxNode expressionNode = new SyntaxNode();
			expressionNode.Type = ASTType.IntExpression;
			
			if(ParsePrimitive(expressionNode))
			{
                while (GetPrecedence() >= minimumPrecedence)
                {
                    int nextPrecedence = GetPrecedence() + 1;
                    ParseBinaryOperator(expressionNode);

                    ParseIntExpression(expressionNode, nextPrecedence);
                }
			}
            if (expressionNode.Children.Count > 1)
            {
                parent.Children.Add(expressionNode);
            }
            else
            {
                parent.Children.Add(expressionNode.Children[0]);
            }
			return true;
		}

        int GetPrecedence()
        {
            switch (_tokenStream[TokenIndex].Type)
            {
                case TokenType.Plus:
                {
                    return 1;
                }
                case TokenType.Minus:
                {
                    return 1;
                }
                case TokenType.Multiply:
                {
                    return 2;
                }
                case TokenType.Divide:
                {
                    return 2;
                }
                default:
                {
                    return -1;
                }
            }
        }

		void AdvanceToken()
		{
            if (TokenIndex + 1 < _tokenStream.Count)
            {
                TokenIndex++;
            }
		}

		bool ParsePrimitive(SyntaxNode parent)
		{
			SyntaxNode primitiveNode = new SyntaxNode();
			primitiveNode.Type = ASTType.Primitive;

            if (_tokenStream[TokenIndex].Type == TokenType.Integer)
			{
				parent.Children.Add(primitiveNode);
                primitiveNode.Data = _tokenStream[TokenIndex].Data;
				AdvanceToken();
				return true;
			}
            else if (_tokenStream[TokenIndex].Type == TokenType.OpenPerenthesis)
			{
				AdvanceToken();
				if(ParseIntExpression(parent, 0))
				{
                    if (_tokenStream[TokenIndex].Type == TokenType.ClosePerenthesis)
					{
						AdvanceToken();
						return true;
					}
					else
					{
						Console.WriteLine("Error: missing closing parenthesis");
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else if(ParseUnaryOperator(primitiveNode))
			{
				parent.Children.Add(primitiveNode.Children[0]);
				return true;
			}
            else if (_tokenStream[TokenIndex].Type == TokenType.Label)
            {
                primitiveNode.Type = ASTType.VariableName;
                primitiveNode.Data = _tokenStream[TokenIndex].Data;
                parent.Children.Add(primitiveNode);
                AdvanceToken();
                return true;
            }
            else if (ParseTag(primitiveNode, false))
            {
                parent.Children.Add(primitiveNode.Children[0]);
                return true;
            }
            else
            {
                return false;
            }
		}

		bool ParseUnaryOperator(SyntaxNode parent)
		{
            if (_tokenStream[TokenIndex].Type == TokenType.Minus)
			{
				SyntaxNode minusNode = new SyntaxNode();
				minusNode.Type = ASTType.UnaryMinus;
				parent.Children.Add(minusNode);
				AdvanceToken();
				
				if(!ParsePrimitive(minusNode))
				{
					Console.WriteLine("Error: Unary minus with no argument");
                    return false;
				}
				
				return true;
			}
			return false;
		}

		bool ParseBinaryOperator(SyntaxNode parent)
		{
			SyntaxNode binaryNode = new SyntaxNode();

            switch (_tokenStream[TokenIndex].Type)
			{
				case TokenType.Plus:
				{
					parent.Children.Add(binaryNode);
					binaryNode.Type = ASTType.BinaryPlus;
					AdvanceToken();
					return true;
				} 
				case TokenType.Minus:
				{
					parent.Children.Add(binaryNode);
					binaryNode.Type = ASTType.BinaryMinus;
					AdvanceToken();
					return true;
				} 
				case TokenType.Multiply:
				{
					parent.Children.Add(binaryNode);
					binaryNode.Type = ASTType.BinaryMul;
					AdvanceToken();
					return true;
				} 
				case TokenType.Divide:
				{
					parent.Children.Add(binaryNode);
					binaryNode.Type = ASTType.BinaryDiv;
					AdvanceToken();
					return true;
				} 				
				default:
				{
					return false;
				}
			}
		}
	}
}
