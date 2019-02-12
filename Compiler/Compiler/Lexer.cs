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
        CloseASM
    }

	class Token
	{
		public TokenType type;
		public string data;
	}

	class Lexer
	{
        string m_text;
        int m_positionInCode;

        HashSet<char> m_reservedCharacters;
        private List<string> m_keywords;

        public Lexer()
        {
            m_reservedCharacters = new HashSet<char> { ' ', '+', '-', '*', '/', '(', ')', ';', '\n', '\r', '=', '\'', '{', '}', '!', '>', '<', '|', '&'};
        }

        static List<string> SetupKeywords()
        {
            return new List<string>() { "+", "-", "*", "/", "(", ")", ";", "int", "=", "'", "bool", "==", "!=", ">", "<", ">=", "<=", "if", "{", "}", "&&", "||", "true", "false", "<asm>", "</asm>"};
        }

		public List<Token> Lex(string inputFilename)
		{
            System.IO.StreamReader file = new System.IO.StreamReader(inputFilename);

            m_text = file.ReadToEnd();
            m_positionInCode = 0;
			List<Token> tokens = new List<Token>();

            m_keywords = SetupKeywords();

            while (m_positionInCode < m_text.Length)
            {
                Token next = GetNextToken();
                if (next != null)
                {
                    tokens.Add(next);
                }
            }

			return tokens;
		}

        public Token GetNextToken()
        {
            char firstChar = m_text[m_positionInCode];

            int temp;
            if (int.TryParse(firstChar.ToString(), out temp))
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
                    m_positionInCode += keywordLength;

                    if(token.type == TokenType.OpenASM)
                    {
                        const string closingTag = "</asm>";

                        bool foundClosingTag = false;
                        while(!foundClosingTag)
                        {
                            for(int i = 0; i < 6; i++)
                            {
                                foundClosingTag = true;
                                if(m_text[m_positionInCode + i] != closingTag[i])
                                {
                                    foundClosingTag = false;
                                    break;
                                }
                            }
                            token.data += m_text[m_positionInCode];
                            m_positionInCode++;
                        }
                        m_positionInCode += 6;
                    }
                }
                else
                {
                    if (m_reservedCharacters.Contains(m_text[m_positionInCode]))
                    {
                        m_positionInCode++;
                        return null;
                    }
                    else
                    {
                        token.type = TokenType.Label;
                        token.data = stringToken;
                        m_positionInCode += stringToken.Length;
                    }
                }
                return token;
            }
        }

        public bool TryGetKeyword(int minimumLength, Token token, out int keywordLength)
        {
            keywordLength = -1;
            bool foundAKeyword = false;
            foreach (string keyword in m_keywords)
            {
                if (keyword.Length < minimumLength)
                {
                    continue;
                }
                bool match = true;
                for (int i = 0; i < keyword.Length; i++)
                {
                    if (keyword[i] != m_text[m_positionInCode + i])
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
                    token.type = TokenType.Plus;
                    break;
                case "-":
                    token.type = TokenType.Minus;
                    break;
                case "*":
                    token.type = TokenType.Multiply;
                    break;
                case "/":
                    token.type = TokenType.Divide;
                    break;
                case "(":
                    token.type = TokenType.OpenPerenthesis;
                    break;
                case ")":
                    token.type = TokenType.ClosePerenthesis;
                    break;
                case "int":
                    token.type = TokenType.IntDeclaration;
                    break;
                case "\'":
                    token.type = TokenType.Prime;
                    break;
                case ";":
                    token.type = TokenType.SemiColon;
                    break;
                case "=":
                    token.type = TokenType.Eqls;
                    break;
                case "<asm>":
                    token.type = TokenType.OpenASM;
                    break;
            }
        }

        public string GetStringToken()
        {
            int length = 0;

            while (!m_reservedCharacters.Contains(m_text[m_positionInCode + length]))
            {
                length++;
            }

            string stringToken = m_text.Substring(m_positionInCode, length);
            
            return stringToken;
        }

        public Token GetNumericToken()
        {
            Token token = new Token();

            int length = 0;
            int currentInt;

            while(m_positionInCode + length < m_text.Length && int.TryParse(m_text[m_positionInCode + length].ToString(), out currentInt))
            {
                length++;
            }

            token.data = m_text.Substring(m_positionInCode, length);
            token.type = TokenType.Integer;

            m_positionInCode += length;

            return token;
        }
	}
}
