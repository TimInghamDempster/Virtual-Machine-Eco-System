using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                writer.WriteLine("module Parser_Generated");
                writer.WriteLine();

                WriteNodeTypes(writer);

                WriteTokenToASTMAppings(writer);

                WriteRulePatterns(writer);
            }
        }

        private void WriteNodeTypes(StreamWriter writer)
        {
            writer.WriteLine("type ASTNodeType =");

            for(int count = 0; count < _grammar.ProductionRules.Count; count++)
            {
                var productionRule = _grammar.ProductionRules[count];
                writer.WriteLine($"    | {productionRule.Name} = {count}");
            }
            writer.WriteLine($"    | Repeat = {_grammar.ProductionRules.Count}");
            writer.WriteLine($"    | Error = {_grammar.ProductionRules.Count + 1}");
            writer.WriteLine();
        }

        private void WriteTokenToASTMAppings(StreamWriter writer)
        {
            writer.WriteLine("let terminalMappings =");
            writer.WriteLine("     Map.empty.");

            var mappings = new StringBuilder();
            foreach (var terminal in _grammar.Terminals)
            {
                mappings.Append($"        Add(Tokens.TokenTypes.{terminal.rule.Name}, ASTNodeType.{terminal.rule.Name}).{Environment.NewLine}");
            }

            // Remove the trailing '.'
            var charsToRemove = Environment.NewLine.Length + 1;
            mappings.Remove(mappings.Length - charsToRemove, 1);
            writer.WriteLine(mappings.ToString());
        }

        private void WriteRulePatterns(StreamWriter writer)
        {
            writer.WriteLine("let patterns =");
            writer.WriteLine("     Map.empty.");

            var rules = new StringBuilder();
            foreach (var productionRule in _grammar.ProductionRules)
            {
                var patternSet = new StringBuilder($"        Add(ASTNodeType.{productionRule.Name},[");
                foreach (var pattern in productionRule.Patterns)
                {
                    patternSet.Append("[");
                    foreach (var element in pattern.Elements)
                    {
                        switch(element.Type)
                        {
                            case ElementType.Repeat:
                                patternSet.Append("ASTNodeType.Repeat; ");
                                
                                if (element.SubPattern == null) throw new InvalidDataException("Repeats must have a sub-pattern");

                                foreach(var subElement in element.SubPattern.Elements)
                                {
                                    switch (subElement.Type)
                                    {
                                        case ElementType.Repeat:
                                            throw new InvalidDataException("Repeats cannot be nested");
                                        case ElementType.Rule:
                                            patternSet.Append($"ASTNodeType.{subElement.Name}; ");
                                            break;
                                        case ElementType.Terminal:
                                            patternSet.Append($"ASTNodeType.{_grammar.Terminals.Where(tpl => tpl.element == subElement).First().rule.Name}; ");
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }
                                }
                                patternSet.Append("ASTNodeType.Repeat; ");
                                break;
                            case ElementType.Rule:
                                patternSet.Append($"ASTNodeType.{element.Name}; ");
                                break;
                            case ElementType.Terminal:
                                patternSet.Append($"ASTNodeType.{_grammar.Terminals.Where(tpl => tpl.element == element).First().rule.Name}; ");
                                break;
                            default:
                                throw new NotImplementedException();
                        }

                    }
                    patternSet.Remove(patternSet.Length - 1, 1);
                    patternSet.Append("];");
                }
                patternSet.Remove(patternSet.Length - 1, 1); 

                patternSet.Append($"]).{Environment.NewLine}");
                rules.Append(patternSet.ToString());
            }
            // Remove the new line and the trailing '.'
            var charsToRemove = Environment.NewLine.Length + 1;
            rules.Remove(rules.Length - charsToRemove, charsToRemove);
            writer.WriteLine(rules.ToString());
        }
    }
}
