using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public enum TokenType
    {
        Plus,
        Minus,
        Multiply,
        Divide,
        OpenPerenthesis,
        ClosePerenthesis,
        Integer,
        Label,
        IntDeclaration,
        Prime,
        SemiColon,
        Eqls,
        OpenASM,
        CloseASM,
        Tag,
        Comment,
        BooleanValue,
        NotImplemented,
        BoolDeclaration,
        BooleanComparison,
        BooleanConjunction,
        If,
        OpenBrace,
        CloseBrace
    }

    class Token
    {
        public TokenType Type { get; set; }

        public string Data { get; set; }

        public int SourceLocation { get; private set; }

        public string Source { get; set; }

        public Token()
        {
            SourceLocation = Lexer.PositionInCode;
        }
	}

	class Lexer
	{
        string _text;
        public static int PositionInCode { get; private set; }

        HashSet<char> _reservedCharacters;
        private List<string> _keywords;

        public Lexer()
        {
            _reservedCharacters = new HashSet<char> { ' ', '+', '-', '*', '/', '(', ')', ';', '\n', '\r', '=', '\'', '{', '}', '!', '>', '<', '|', '&', '\t'};
        }

        static List<string> SetupKeywords()
        {
            return new List<string>() { "+", "-", "*", "/", "(", ")", ";", "int", "=", "'", "bool", "==", "!=", ">", "<", ">=", "<=", "if", "{", "}", "&&", "||", "true", "false", "<asm>", "</asm>", "Tag"};
        }

		public List<Token> Lex(string inputFilename)
		{
            System.IO.StreamReader file = new System.IO.StreamReader(inputFilename);

            _text = file.ReadToEnd();
            PositionInCode = 0;
			List<Token> tokens = new List<Token>();

            _keywords = SetupKeywords();

            while (PositionInCode < _text.Length)
            {
                Token next = GetNextToken();
                if (next != null)
                {
                    tokens.Add(next);
                }
            }

            InsertCodeIntoTokens(tokens);

			return tokens;
		}

        private void InsertCodeIntoTokens(List<Token> tokens)
        {
            for(int tokenId = 0; tokenId < tokens.Count - 1; tokenId++)
            {
                int startPos = tokens[tokenId].SourceLocation;
                int endPos = tokens[tokenId + 1].SourceLocation;

                tokens[tokenId].Source = _text.Substring(startPos, endPos - startPos);
            }
        }

        private Token ReadComment()
        {
            
            int startPos = PositionInCode;
            while (_text[PositionInCode] != '\n')
            {
                PositionInCode++;
            }

            return new Token()
            {
                Data = _text.Substring(startPos, PositionInCode - startPos),
                Type = TokenType.Comment
            };
        }

        public Token GetNextToken()
        {
            char firstChar = _text[PositionInCode];

            int temp;

            if (firstChar == '/' && _text[PositionInCode + 1] == '/')
            {
                return ReadComment();
            }
            else if (int.TryParse(firstChar.ToString(), out temp))
            {
                return GetNumericToken();
            }
            else
            {
                Token token = new Token();

                string stringToken = GetStringToken();

                int keywordLength;
                bool keyword = TryGetKeyword(stringToken.Length, token, out keywordLength);

                if (keyword)
                {
                    PositionInCode += keywordLength;

                    if(token.Type == TokenType.OpenASM)
                    {
                        const string closingTag = "</asm>";

                        bool foundClosingTag = false;
                        while(!foundClosingTag)
                        {
                            for(int i = 0; i < 6; i++)
                            {
                                foundClosingTag = true;
                                if(_text[PositionInCode + i] != closingTag[i])
                                {
                                    foundClosingTag = false;
                                    break;
                                }
                            }
                            token.Data += _text[PositionInCode];
                            PositionInCode++;
                        }
                        PositionInCode += 6;
                    }
                }
                else
                {
                    if (_reservedCharacters.Contains(_text[PositionInCode]))
                    {
                        PositionInCode++;
                        return null;
                    }
                    else
                    {
                        token.Type = TokenType.Label;
                        token.Data = stringToken;
                        PositionInCode += stringToken.Length;
                    }
                }
                return token;
            }
        }

        public bool TryGetKeyword(int minimumLength, Token token, out int keywordLength)
        {
            keywordLength = -1;
            bool foundAKeyword = false;
            foreach (string keyword in _keywords)
            {
                if (keyword.Length < minimumLength)
                {
                    continue;
                }

                // Not enough chars left for it to be this word
                if (_text.Length < PositionInCode + keyword.Length)
                    continue;

                bool match = true;
                for (int i = 0; i < keyword.Length; i++)
                {
                    if (keyword[i] != _text[PositionInCode + i])
                    {
                        match = false;
                    }
                }
                if (match == true)
                {
                    GenerateToken(keyword, token);
                    keywordLength = keyword.Length;
                    minimumLength = keywordLength;
                    foundAKeyword = true;
                }
            }
            return foundAKeyword;
        }

        private void GenerateToken(string keyword, Token token)
        {
            switch (keyword)
            {
                case "+":
                    token.Type = TokenType.Plus;
                    break;
                case "-":
                    token.Type = TokenType.Minus;
                    break;
                case "*":
                    token.Type = TokenType.Multiply;
                    break;
                case "/":
                    token.Type = TokenType.Divide;
                    break;
                case "(":
                    token.Type = TokenType.OpenPerenthesis;
                    break;
                case ")":
                    token.Type = TokenType.ClosePerenthesis;
                    break;
                case "int":
                    token.Type = TokenType.IntDeclaration;
                    break;
                case "\'":
                    token.Type = TokenType.Prime;
                    break;
                case ";":
                    token.Type = TokenType.SemiColon;
                    break;
                case "=":
                    token.Type = TokenType.Eqls;
                    break;
                case "<asm>":
                    token.Type = TokenType.OpenASM;
                    break;
                case "Tag":
                    token.Type = TokenType.Tag;
                    break;
                case "bool":
                    token.Type = TokenType.BoolDeclaration;
                    break;
                case "true":
                case "false":
                    token.Type = TokenType.BooleanValue;
                    token.Data = keyword;
                    break;
                case "==":
                case "!=":
                case ">":
                case "<":
                case ">=":
                case "<=":
                    token.Type = TokenType.BooleanComparison;
                    token.Data = keyword;
                    break;
                case "&&":
                case "||":
                    token.Type = TokenType.BooleanConjunction;
                    token.Data = keyword;
                    break;
                case "if":
                    token.Type = TokenType.If;
                    break;
                case "{":
                    token.Type = TokenType.OpenBrace;
                    break;
                case "}":
                    token.Type = TokenType.CloseBrace;
                    break;
                default:
                    token.Type = TokenType.NotImplemented;
                    break;
            }
        }

        public string GetStringToken()
        {
            int length = 0;

            while (!_reservedCharacters.Contains(_text[PositionInCode + length]))
            {
                length++;
            }

            string stringToken = _text.Substring(PositionInCode, length);
            
            return stringToken;
        }

        public Token GetNumericToken()
        {
            Token token = new Token();

            int length = 0;
            int currentInt;

            while(PositionInCode + length < _text.Length && int.TryParse(_text[PositionInCode + length].ToString(), out currentInt))
            {
                length++;
            }

            token.Data = _text.Substring(PositionInCode, length);
            token.Type = TokenType.Integer;

            PositionInCode += length;

            return token;
        }
	}
}
