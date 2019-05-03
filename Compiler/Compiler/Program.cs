using System.Collections.Generic;

namespace Compiler
{
    class Program
	{
		static Lexer m_lexer = new Lexer();
		static Parser m_parser = new Parser();
        static SemanticAnalyser m_semantics = new SemanticAnalyser();
        static CodeGenerator m_codeGenerator = new CodeGenerator();

		static void Main(string[] args)
		{
			string codeFile = "TestProgram.tim";

			List<Token> tokenStream = m_lexer.Lex(codeFile);

            if (tokenStream != null)
            {
                SyntaxNode abstractSyntaxTree = new SyntaxNode();
                bool parsed = m_parser.Parse(tokenStream, abstractSyntaxTree);
                bool semanticallySound = m_semantics.DoSemanticAnalysis(abstractSyntaxTree, (int)Virtual_Machine.VirtualMachine.RAMSize - 1);

                var stream = m_codeGenerator.GenerateCode(abstractSyntaxTree, m_semantics.VariableTable, m_semantics.TagTable);

                VMUtils.BlockDeviceWriter.WriteForBlockDevice("MainDrive", stream);

                //System.Diagnostics.Process proc = System.Diagnostics.Process.Start(@"..\..\..\..\Virutal Machine\bin\Release\Virutal Machine.exe");
            }
		}
	}
}
