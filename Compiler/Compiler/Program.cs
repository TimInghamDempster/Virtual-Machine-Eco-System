using System;
using System.Collections.Generic;

namespace Compiler
{
    class Program
	{
        static ILogger _logger = new ConsoleLogger();
		static Lexer _lexer = new Lexer();
		static Parser _parser = new Parser(
            _logger);
        static SemanticAnalyser m_semantics = new SemanticAnalyser();
        static CodeGenerator m_codeGenerator = new CodeGenerator();
        

		static void Main(string[] args)
		{
			string codeFile = "TestProgram.tim";

			List<Token> tokenStream = _lexer.Lex(codeFile);

            if (tokenStream != null)
            {
                SyntaxNode abstractSyntaxTree = new SyntaxNode();
                bool parsed = _parser.Parse(tokenStream, abstractSyntaxTree);
                bool semanticallySound = m_semantics.DoSemanticAnalysis(abstractSyntaxTree, (int)Virtual_Machine.VirtualMachine.RAMSize - 1);

                var stream = 
                    m_codeGenerator.GenerateCode(
                        abstractSyntaxTree, 
                        m_semantics.VariableTable,
                        m_semantics.TagTable,
                        tokenStream);

                VMUtils.BlockDeviceWriter.WriteForBlockDevice("MainDrive", stream);

                //System.Diagnostics.Process proc = System.Diagnostics.Process.Start(@"..\..\..\..\Virutal Machine\bin\Release\Virutal Machine.exe");
                if (_logger.HasLogged)
                {
                    Console.WriteLine("Press enter to continue");
                    Console.ReadLine();
                }
            }
		}
	}
}
