using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class VariableEntry
    {
        public int m_address;
        public bool m_initialised;
    }

    class SemanticAnalyser
    {
        Dictionary<string, VariableEntry> m_variableTable;
        int m_nextAddress;

        public Dictionary<string, VariableEntry> VariableTable { get { return m_variableTable; } }

        public bool DoSemanticAnalysis(SyntaxNode programTree)
        {
            m_variableTable = new Dictionary<string, VariableEntry>();
            m_nextAddress = 0;

            if (BuildTableForNode(programTree) == false)
            {
                return false;
            }
            return AnalyseNode(programTree);
        }

        bool BuildTableForNode(SyntaxNode node)
        {
            if (node.m_type == ASTType.Decleration)
            {
                if (m_variableTable.ContainsKey(node.m_data))
                {
                    Console.WriteLine("Error, variable redeclaration: " + node.m_data);
                    return false;
                }
                m_variableTable.Add(node.m_data, new VariableEntry());
                m_variableTable[node.m_data].m_address = m_nextAddress;
                m_nextAddress++;
                m_variableTable[node.m_data].m_initialised = true;
            }
            else if (node.m_type == ASTType.UninitialisedDeclaration)
            {
                if (m_variableTable.ContainsKey(node.m_data))
                {
                    Console.WriteLine("Error, variable redeclaration: " + node.m_data);
                    return false;
                }
                m_variableTable.Add(node.m_data, new VariableEntry());
                m_variableTable[node.m_data].m_address = m_nextAddress;
                m_nextAddress++;
                m_variableTable[node.m_data].m_initialised = false;
            }
            foreach (SyntaxNode child in node.m_children)
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
            if (node.m_type == ASTType.VariableName)
            {
                if (!m_variableTable.ContainsKey(node.m_data))
                {
                    Console.WriteLine("Error, undeclared variable: " + node.m_data);
                    return false;
                }
            }
            if (node.m_type == ASTType.Assignment)
            {
                if (!m_variableTable.ContainsKey(node.m_data))
                {
                    Console.WriteLine("Error, undeclared variable: " + node.m_data);
                    return false;
                }
            }
            foreach (SyntaxNode child in node.m_children)
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
