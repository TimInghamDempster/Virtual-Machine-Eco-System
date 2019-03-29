using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class VariableEntry
    {
        public int Address { get; set; }
        public bool Initialised { get; set; }
    }

    public class Tag
    {
        public int Value { get; set; }
        public List<int> InstructionLocations { get; } = new List<int>();
    }

    class SemanticAnalyser
    {
        int _nextAddress;

        public Dictionary<string, VariableEntry> VariableTable { get; } = new Dictionary<string, VariableEntry>();
        public Dictionary<string, Tag> TagTable { get; } = new Dictionary<string, Tag>();

        public bool DoSemanticAnalysis(SyntaxNode programTree, int stackBase)
        {
            _nextAddress = stackBase;

            if (BuildTableForNode(programTree) == false)
            {
                return false;
            }
            return AnalyseNode(programTree);
        }

        bool BuildTableForNode(SyntaxNode node)
        {
            if (node.Type == ASTType.Decleration)
            {
                if (VariableTable.ContainsKey(node.Data))
                {
                    Console.WriteLine("Error, variable redeclaration: " + node.Data);
                    return false;
                }
                VariableTable.Add(node.Data, new VariableEntry());
                VariableTable[node.Data].Address = _nextAddress;
                _nextAddress--;
                VariableTable[node.Data].Initialised = true;
            }
            else if (node.Type == ASTType.UninitialisedDeclaration)
            {
                if (VariableTable.ContainsKey(node.Data))
                {
                    Console.WriteLine("Error, variable redeclaration: " + node.Data);
                    return false;
                }
                VariableTable.Add(node.Data, new VariableEntry());
                VariableTable[node.Data].Address = _nextAddress;
                _nextAddress--;
                VariableTable[node.Data].Initialised = false;
            }
            foreach (SyntaxNode child in node.Children)
            {
                if (BuildTableForNode(child) == false)
                {
                    return false;
                }
            }
            return true;
        }

        bool AnalyseNode(SyntaxNode node)
        {
            if (node.Type == ASTType.VariableName)
            {
                if (!VariableTable.ContainsKey(node.Data))
                {
                    Console.WriteLine("Error, undeclared variable: " + node.Data);
                    return false;
                }
            }
            if (node.Type == ASTType.Assignment)
            {
                if (!VariableTable.ContainsKey(node.Data))
                {
                    Console.WriteLine("Error, undeclared variable: " + node.Data);
                    return false;
                }
            }
            if (node.Type == ASTType.Tag)
            {
                if (!TagTable.ContainsKey(node.Data))
                {
                    TagTable.Add(node.Data, new Tag());
                }
            }

            foreach (SyntaxNode child in node.Children)
            {
                if (AnalyseNode(child) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
