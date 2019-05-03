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
        Expression,
        Primitive,
        Decleration,
        UninitialisedDeclaration,
        Assignment,
        VariableName,
        ASM,
        Tag,
        Comment,
    }

	class SyntaxNode
	{
		public List<SyntaxNode> Children { get; } = new List<SyntaxNode>();
		public ASTType Type { get; set; }
		public string Data { get; set; }
	}

	class Parser
	{
		List<Token> _tokenStream;
		int _tokenIndex;

		public bool Parse(List<Token> tokenStream, SyntaxNode programNode)
		{
			_tokenStream = tokenStream;

            while (ParseStatement(programNode) || ParseComment(programNode))
            {
                if (_tokenIndex == tokenStream.Count)
                {
                    return true;
                }
            }

            return false;
		}

        private bool ParseComment(SyntaxNode programNode)
        {
            var token = _tokenStream[_tokenIndex];
            if (token.Type == TokenType.Comment)
            {
                var commentNode = new SyntaxNode()
                {
                    Type =  ASTType.Comment,
                    Data = token.Data
                };

                programNode.Children.Add(commentNode);
                _tokenIndex++;
                return true;
            }
            else
            {
                return false;
            }
        }

        bool ParseStatement(SyntaxNode parent)
        {
            int startIndex = _tokenIndex;

            if(ParseDeclaration(parent) == true)
            {
                return true;
            }
            else
            {
                _tokenIndex = startIndex;
                if(ParseAssignment(parent) == true)
                {
                    return true;
                }
                else
                {
                    _tokenIndex = startIndex;
                    if (ParseASM(parent))
                    {
                        return true;
                    }
                    else
                    {
                        _tokenIndex = startIndex;
                        if (ParseTag(parent, true))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        private bool ParseTag(SyntaxNode parent, bool isStatement)
        {
            if (_tokenStream[_tokenIndex].Type == TokenType.Tag &&
                _tokenStream[_tokenIndex + 1].Type == TokenType.OpenPerenthesis &&
                _tokenStream[_tokenIndex + 2].Type == TokenType.Label &&
                _tokenStream[_tokenIndex + 3].Type == TokenType.ClosePerenthesis)
            {
                var tagNode = new SyntaxNode()
                {
                    Type = ASTType.Tag,
                    Data = _tokenStream[_tokenIndex + 2].Data
                };

                parent.Children.Add(tagNode);
                _tokenIndex += 4;

                // A tag statement ends in a semicolon but a tag reference might not,
                // for a reference the semicolon will be handled by the statement it
                // appears in but for a tag statement we ARE the statement and need
                // to deal with it
                if (isStatement)
                {
                    if (_tokenStream[_tokenIndex].Type == TokenType.SemiColon)
                    {
                        _tokenIndex++;
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

        private bool ParseDeclaration(SyntaxNode parent)
        {
            bool initialise = true;
            string name = "";

            if (_tokenStream[_tokenIndex].Type != TokenType.IntDeclaration)
            {
                return false;
            }
            _tokenIndex++;
            
            if (_tokenStream[_tokenIndex].Type == TokenType.Prime)
            {
                initialise = false;
                _tokenIndex++;
            }

            if (_tokenStream[_tokenIndex].Type != TokenType.Label)
            {
                return false;
            }
            else
            {
                name = _tokenStream[_tokenIndex].Data;
            }
            _tokenIndex++;

            if (_tokenStream[_tokenIndex].Type != TokenType.SemiColon)
            {
                Console.WriteLine("Error, semicolon expected");
                return false;
            }
            _tokenIndex++;

            SyntaxNode declerationNode = new SyntaxNode();
            if (initialise == true)
            {
                declerationNode.Type = ASTType.Decleration;
            }
            else
            {
                declerationNode.Type = ASTType.UninitialisedDeclaration;
            }
            declerationNode.Data = name;
            parent.Children.Add(declerationNode);

            return true;
        }

        private bool ParseAssignment(SyntaxNode parent)
        {
            string name = "";

            if (_tokenStream[_tokenIndex].Type != TokenType.Label)
            {
                return false;
            }
            name = _tokenStream[_tokenIndex].Data;
            _tokenIndex++;

            if (_tokenStream[_tokenIndex].Type != TokenType.Eqls)
            {
                return false;
            }
            _tokenIndex++;

            SyntaxNode assignmentNode = new SyntaxNode();
            assignmentNode.Type = ASTType.Assignment;
            assignmentNode.Data = name;

            if (!ParseExpression(assignmentNode, 0))
            {
                return false;
            }

            if (_tokenStream[_tokenIndex].Type != TokenType.SemiColon)
            {
                return false;
            }
            _tokenIndex++;

            parent.Children.Add(assignmentNode);
            return true;
        }

        private bool ParseASM(SyntaxNode parent)
        {
            if(_tokenStream[_tokenIndex].Type == TokenType.OpenASM)
            {
                SyntaxNode asmNode = new SyntaxNode();
                asmNode.Type = ASTType.ASM;
                asmNode.Data = _tokenStream[_tokenIndex].Data;

                parent.Children.Add(asmNode);
                _tokenIndex++;
                return true;
            }

            return false;
        }

		bool ParseExpression(SyntaxNode parent, int minimumPrecedence)
		{
			SyntaxNode expressionNode = new SyntaxNode();
			expressionNode.Type = ASTType.Expression;
			
			if(ParsePrimitive(expressionNode))
			{
                while (GetPrecedence() >= minimumPrecedence)
                {
                    int nextPrecedence = GetPrecedence() + 1;
                    ParseBinaryOperator(expressionNode);

                    ParseExpression(expressionNode, nextPrecedence);
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
            switch (_tokenStream[_tokenIndex].Type)
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
            if (_tokenIndex + 1 < _tokenStream.Count)
            {
                _tokenIndex++;
            }
		}

		bool ParsePrimitive(SyntaxNode parent)
		{
			SyntaxNode primitiveNode = new SyntaxNode();
			primitiveNode.Type = ASTType.Primitive;

            if (_tokenStream[_tokenIndex].Type == TokenType.Integer)
			{
				parent.Children.Add(primitiveNode);
                primitiveNode.Data = _tokenStream[_tokenIndex].Data;
				AdvanceToken();
				return true;
			}
            else if (_tokenStream[_tokenIndex].Type == TokenType.OpenPerenthesis)
			{
				AdvanceToken();
				if(ParseExpression(parent, 0))
				{
                    if (_tokenStream[_tokenIndex].Type == TokenType.ClosePerenthesis)
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
            else if (_tokenStream[_tokenIndex].Type == TokenType.Label)
            {
                primitiveNode.Type = ASTType.VariableName;
                primitiveNode.Data = _tokenStream[_tokenIndex].Data;
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
            if (_tokenStream[_tokenIndex].Type == TokenType.Minus)
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

            switch (_tokenStream[_tokenIndex].Type)
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
