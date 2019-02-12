using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

	class SyntaxNode
	{
		public List<SyntaxNode> m_children = new List<SyntaxNode>();
		public ASTType m_type;
		public string m_data;
	}

	class Parser
	{
		List<Token> m_tokenStream;
		int m_tokenIndex;

		public bool Parse(List<Token> tokenStream, SyntaxNode programNode)
		{
			m_tokenStream = tokenStream;

            while (ParseStatement(programNode))
            {
                if (m_tokenIndex == tokenStream.Count)
                {
                    return true;
                }
            }

            return false;
		}

        bool ParseStatement(SyntaxNode parent)
        {
            int startIndex = m_tokenIndex;

            if(ParseDeclaration(parent) == true)
            {
                return true;
            }
            else
            {
                m_tokenIndex = startIndex;
                if(ParseAssignment(parent) == true)
                {
                    return true;
                }
                else
                {
                    m_tokenIndex = startIndex;
                    if(ParseASM(parent))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private bool ParseDeclaration(SyntaxNode parent)
        {
            bool initialise = true;
            string name = "";

            if (m_tokenStream[m_tokenIndex].type != TokenType.IntDeclaration)
            {
                return false;
            }
            m_tokenIndex++;
            
            if (m_tokenStream[m_tokenIndex].type == TokenType.Prime)
            {
                initialise = false;
                m_tokenIndex++;
            }

            if (m_tokenStream[m_tokenIndex].type != TokenType.Label)
            {
                return false;
            }
            else
            {
                name = m_tokenStream[m_tokenIndex].data;
            }
            m_tokenIndex++;

            if (m_tokenStream[m_tokenIndex].type != TokenType.SemiColon)
            {
                Console.WriteLine("Error, semicolon expected");
                return false;
            }
            m_tokenIndex++;

            SyntaxNode declerationNode = new SyntaxNode();
            if (initialise == true)
            {
                declerationNode.m_type = ASTType.Decleration;
            }
            else
            {
                declerationNode.m_type = ASTType.UninitialisedDeclaration;
            }
            declerationNode.m_data = name;
            parent.m_children.Add(declerationNode);

            return true;
        }

        private bool ParseAssignment(SyntaxNode parent)
        {
            string name = "";

            if (m_tokenStream[m_tokenIndex].type != TokenType.Label)
            {
                return false;
            }
            name = m_tokenStream[m_tokenIndex].data;
            m_tokenIndex++;

            if (m_tokenStream[m_tokenIndex].type != TokenType.Eqls)
            {
                return false;
            }
            m_tokenIndex++;

            SyntaxNode assignmentNode = new SyntaxNode();
            assignmentNode.m_type = ASTType.Assignment;
            assignmentNode.m_data = name;

            if (!ParseExpression(assignmentNode, 0))
            {
                return false;
            }

            if (m_tokenStream[m_tokenIndex].type != TokenType.SemiColon)
            {
                return false;
            }
            m_tokenIndex++;

            parent.m_children.Add(assignmentNode);
            return true;
        }

        private bool ParseASM(SyntaxNode parent)
        {
            if(m_tokenStream[m_tokenIndex].type == TokenType.OpenASM)
            {
                SyntaxNode asmNode = new SyntaxNode();
                asmNode.m_type = ASTType.ASM;
                asmNode.m_data = m_tokenStream[m_tokenIndex].data;

                parent.m_children.Add(asmNode);
                m_tokenIndex++;
                return true;
            }

            return false;
        }

		bool ParseExpression(SyntaxNode parent, int minimumPrecedence)
		{
			SyntaxNode expressionNode = new SyntaxNode();
			expressionNode.m_type = ASTType.Expression;
			
			if(ParsePrimitive(expressionNode))
			{
                while (GetPrecedence() >= minimumPrecedence)
                {
                    int nextPrecedence = GetPrecedence() + 1;
                    ParseBinaryOperator(expressionNode);

                    ParseExpression(expressionNode, nextPrecedence);
                }
			}
            if (expressionNode.m_children.Count > 1)
            {
                parent.m_children.Add(expressionNode);
            }
            else
            {
                parent.m_children.Add(expressionNode.m_children[0]);
            }
			return true;
		}

        int GetPrecedence()
        {
            switch (m_tokenStream[m_tokenIndex].type)
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
            if (m_tokenIndex + 1 < m_tokenStream.Count)
            {
                m_tokenIndex++;
            }
		}

		bool ParsePrimitive(SyntaxNode parent)
		{
			SyntaxNode primitiveNode = new SyntaxNode();
			primitiveNode.m_type = ASTType.Primitive;

            if (m_tokenStream[m_tokenIndex].type == TokenType.Integer)
			{
				parent.m_children.Add(primitiveNode);
                primitiveNode.m_data = m_tokenStream[m_tokenIndex].data;
				AdvanceToken();
				return true;
			}
            else if (m_tokenStream[m_tokenIndex].type == TokenType.OpenPerenthesis)
			{
				AdvanceToken();
				if(ParseExpression(parent, 0))
				{
                    if (m_tokenStream[m_tokenIndex].type == TokenType.ClosePerenthesis)
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
				parent.m_children.Add(primitiveNode.m_children[0]);
				return true;
			}
            else if (m_tokenStream[m_tokenIndex].type == TokenType.Label)
            {
                primitiveNode.m_type = ASTType.VariableName;
                primitiveNode.m_data = m_tokenStream[m_tokenIndex].data;
                parent.m_children.Add(primitiveNode);
                AdvanceToken();
                return true;
            }
            else
            {
                return false;
            }
		}

		bool ParseUnaryOperator(SyntaxNode parent)
		{
            if (m_tokenStream[m_tokenIndex].type == TokenType.Minus)
			{
				SyntaxNode minusNode = new SyntaxNode();
				minusNode.m_type = ASTType.UnaryMinus;
				parent.m_children.Add(minusNode);
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

            switch (m_tokenStream[m_tokenIndex].type)
			{
				case TokenType.Plus:
				{
					parent.m_children.Add(binaryNode);
					binaryNode.m_type = ASTType.BinaryPlus;
					AdvanceToken();
					return true;
				} 
				case TokenType.Minus:
				{
					parent.m_children.Add(binaryNode);
					binaryNode.m_type = ASTType.BinaryMinus;
					AdvanceToken();
					return true;
				} 
				case TokenType.Multiply:
				{
					parent.m_children.Add(binaryNode);
					binaryNode.m_type = ASTType.BinaryMul;
					AdvanceToken();
					return true;
				} 
				case TokenType.Divide:
				{
					parent.m_children.Add(binaryNode);
					binaryNode.m_type = ASTType.BinaryDiv;
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
