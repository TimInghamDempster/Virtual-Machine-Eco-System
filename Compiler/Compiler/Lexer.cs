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
        NotImplemented
    }

    class Token
    {
        public TokenType Type { get; set; }

        public string Data { get; set; }
	}

	class Lexer
	{
        string _text;
        int _positionInCode;

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
            _positionInCode = 0;
			List<Token> tokens = new List<Token>();

            _keywords = SetupKeywords();

            while (_positionInCode < _text.Length)
            {
                Token next = GetNextToken();
                if (next != null)
                {
                    tokens.Add(next);
                }
            }

			return tokens;
		}

        private Token ReadComment()
        {
            
            int startPos = _positionInCode;
            while (_text[_positionInCode] != '\n')
            {
                _positionInCode++;
            }

            return new Token()
            {
                Data = _text.Substring(startPos, _positionInCode - startPos),
                Type = TokenType.Comment
            };
        }

        public Token GetNextToken()
        {
            char firstChar = _text[_positionInCode];

            int temp;

            if (firstChar == '/' && _text[_positionInCode + 1] == '/')
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
                    _positionInCode += keywordLength;

                    if(token.Type == TokenType.OpenASM)
                    {
                        const string closingTag = "</asm>";

                        bool foundClosingTag = false;
                        while(!foundClosingTag)
                        {
                            for(int i = 0; i < 6; i++)
                            {
                                foundClosingTag = true;
                                if(_text[_positionInCode + i] != closingTag[i])
                                {
                                    foundClosingTag = false;
                                    break;
                                }
                            }
                            token.Data += _text[_positionInCode];
                            _positionInCode++;
                        }
                        _positionInCode += 6;
                    }
                }
                else
                {
                    if (_reservedCharacters.Contains(_text[_positionInCode]))
                    {
                        _positionInCode++;
                        return null;
                    }
                    else
                    {
                        token.Type = TokenType.Label;
                        token.Data = stringToken;
                        _positionInCode += stringToken.Length;
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
                bool match = true;
                for (int i = 0; i < keyword.Length; i++)
                {
                    if (keyword[i] != _text[_positionInCode + i])
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
                default:
                    token.Type = TokenType.NotImplemented;
                    break;
            }
        }

        public string GetStringToken()
        {
            int length = 0;

            while (!_reservedCharacters.Contains(_text[_positionInCode + length]))
            {
                length++;
            }

            string stringToken = _text.Substring(_positionInCode, length);
            
            return stringToken;
        }

        public Token GetNumericToken()
        {
            Token token = new Token();

            int length = 0;
            int currentInt;

            while(_positionInCode + length < _text.Length && int.TryParse(_text[_positionInCode + length].ToString(), out currentInt))
            {
                length++;
            }

            token.Data = _text.Substring(_positionInCode, length);
            token.Type = TokenType.Integer;

            _positionInCode += length;

            return token;
        }
	}
}
