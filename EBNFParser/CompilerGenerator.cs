using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EBNFParser
{
    public class CompilerGenerator
    {
        private readonly GrammarParser _grammar;

        public CompilerGenerator(GrammarParser grammar)
        {
            _grammar = grammar;
        }

        // Generate the f# token types and regexes needed by
        // a lexer for this grammar and write them into a file
        public void GenerateLexerCode(string path)
        {
            var terminalStringBuilder = new StringBuilder("let patterns = [\n");
            var tokenEnumBuilder = new StringBuilder("type TokenTypes =\n");
            var accountedForTokens = new HashSet<string>();

            int index = 0;
            foreach (var elementTuple in _grammar.Terminals)
            {
                var tokenType = elementTuple.rule.Name;
                var tokenPattern = elementTuple.element.Name;
                terminalStringBuilder.Append($"    (TokenTypes.{tokenType}, Regex(\"^{tokenPattern}\"));\n");

                if (!accountedForTokens.Contains(tokenType))
                {
                    tokenEnumBuilder.Append($"    | {tokenType} = {index}\n");
                    accountedForTokens.Add(tokenType);
                    index++;
                }
            }

            terminalStringBuilder.Append("]");

            tokenEnumBuilder.Append($"    | Invalid = {index}\n");

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("module Tokens");
                writer.WriteLine();
                writer.WriteLine("open System.Text.RegularExpressions");
                writer.WriteLine();
                writer.WriteLine(tokenEnumBuilder.ToString());
                writer.WriteLine();
                writer.WriteLine(terminalStringBuilder.ToString());
            }
        }

        // Generate an f# recursive decent parser for the
        // grammar in the specified file
        public void GenerateParser(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("module Parser");
                writer.WriteLine();
                writer.WriteLine("type ASTNodeType =");
                writer.WriteLine("    | test = 0");
                writer.WriteLine("    | test2 = 1");
                writer.WriteLine();

                writer.WriteLine("type ASTNode =");
                writer.WriteLine("    {");
                writer.WriteLine("        _nodeType: ASTNodeType;");
                writer.WriteLine("        _content: string;");
                writer.WriteLine("        _children: ASTNode list");
                writer.WriteLine("    }");
                writer.WriteLine();

                writer.WriteLine("let testInnerNode =");
                writer.WriteLine("    {");
                writer.WriteLine("        _nodeType = ASTNodeType.test;");
                writer.WriteLine("        _content = \"inner content\";");
                writer.WriteLine("        _children = [];");
                writer.WriteLine("    }");
                writer.WriteLine();
                writer.WriteLine("let start =");
                writer.WriteLine("    {");
                writer.WriteLine("        _nodeType = ASTNodeType.test2;");
                writer.WriteLine("        _content = \"content\";");
                writer.WriteLine("        _children = [testInnerNode; testInnerNode];");
                writer.WriteLine("    }");
            }
        }
    }
}
