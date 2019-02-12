using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	class CodeGenerator
	{
		List<int> m_codeStream = new List<int>();
        Dictionary<string, VariableEntry> m_variableTable;

		public List<int> GenerateCode(SyntaxNode abstractSyntaxTree, Dictionary<string, VariableEntry> variableTable)
		{
			m_codeStream = new List<int>();
            m_variableTable = variableTable;

            // Make sure r0 = 0
            int initRegInstruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.SetLiteral;
            m_codeStream.Add(initRegInstruction);
            m_codeStream.Add(0);

            int storeInstruction = (int)Virtual_Machine.UnitCodes.Store | (int)Virtual_Machine.StoreOperations.StoreToLiteralLocation;
            int pushInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.Push;
 
            foreach (VariableEntry variable in m_variableTable.Values)
            {
                if (variable.m_initialised)
                {
                    // Write a zero if variable is initialised
                    m_codeStream.Add(storeInstruction);
                    m_codeStream.Add(variable.m_address);
                }
                // Reserve space for variable on stack
                m_codeStream.Add(pushInstruction);
                m_codeStream.Add(0);
            }

            foreach (SyntaxNode node in abstractSyntaxTree.m_children)
            {
                if (node.m_type == ASTType.Assignment)
                {
                    GenerateAssignment(node);
                }
                else if(node.m_type == ASTType.ASM)
                {
                    GenerateASM(node);
                }
            }

			AddPostScript();
            return m_codeStream;
		}

        private void GenerateASM(SyntaxNode node)
        {
            m_codeStream.AddRange(Assembler.Assembler.ParseString(node.m_data));
        }

        private void GenerateAssignment(SyntaxNode node)
        {
            GenerateExpression(node.m_children[0]);

            int storeAddress = m_variableTable[node.m_data].m_address;
            int storeInstruction = (int)Virtual_Machine.UnitCodes.Store | (int)Virtual_Machine.StoreOperations.StoreToLiteralLocation;
            m_codeStream.Add(storeInstruction);
            m_codeStream.Add(storeAddress);
        }

		void GenerateExpression(SyntaxNode  expressionNode)
		{
			GeneratePrimitive(expressionNode.m_children[0], 0, true);

			for(int i = 1; i < expressionNode.m_children.Count; i += 2)
			{
				GeneratePrimitive(expressionNode.m_children[i + 1], 1, false);
				GenerateBinaryOperator(expressionNode.m_children[i], 0, 1);
			}
		}

		void GenerateBinaryOperator(SyntaxNode operatorNode, int targetRegister, int sourceRegister)
		{
			int instruction = targetRegister << 8 | targetRegister;
			int argument = sourceRegister;

			switch(operatorNode.m_type)
			{
			case ASTType.BinaryPlus:
				instruction |= (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Add;
				break;
			case ASTType.BinaryMinus:
				instruction |= (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Subtract;
				break;
			case ASTType.BinaryMul:
				instruction |= (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Multiply;
				break;
			case ASTType.BinaryDiv:
				instruction |= (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Divide;
				break;
			}

			m_codeStream.Add(instruction);
			m_codeStream.Add(argument);
		}

		void GenerateUnaryOperator(SyntaxNode unaryNode, int targetRegister)
		{
			GeneratePrimitive(unaryNode.m_children[0], targetRegister, false);
		}

		void GeneratePrimitive(SyntaxNode primitiveNode, int targetRegister, bool firstPrimitiveInExpression)
		{
			
			if(primitiveNode.m_type == ASTType.Primitive)
			{
				int val;
				int.TryParse(primitiveNode.m_data, out val);

				int instruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.SetLiteral | targetRegister << 8;

				m_codeStream.Add(instruction);
				m_codeStream.Add(val);
			}
			else if(primitiveNode.m_type == ASTType.UnaryMinus)
			{
				if (primitiveNode.m_children[0].m_type == ASTType.Primitive)
				{
					GenerateUnaryOperator(primitiveNode, targetRegister);

					int negativeInstruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.MultiplyLiteral | targetRegister << 8 | targetRegister;
					m_codeStream.Add(negativeInstruction);
					m_codeStream.Add(-1);
				}
            }
            else if (primitiveNode.m_type == ASTType.VariableName)
            {
                int address = m_variableTable[primitiveNode.m_data].m_address;
                int loadInstruction = (int)Virtual_Machine.UnitCodes.Load | (int)Virtual_Machine.LoadOperations.LoadFromLiteralLocation | targetRegister << 8;
                m_codeStream.Add(loadInstruction);
                m_codeStream.Add(address);
            }
            else
            {
                if (!firstPrimitiveInExpression)
                {
                    int pushInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.Push;
                    m_codeStream.Add(pushInstruction);
                    m_codeStream.Add(0);
                }

                GenerateExpression(primitiveNode);

                if (!firstPrimitiveInExpression)
                {
                    int copyInstruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Copy | 1 << 8 | 0;
                    m_codeStream.Add(copyInstruction);
                    m_codeStream.Add(0);

                    int popInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.Pop;
                    m_codeStream.Add(popInstruction);
                    m_codeStream.Add(0);
                }
            }
		}

		void AddPostScript()
		{
			List<int> postScript = new List<int>()
			{
				(int)Virtual_Machine.UnitCodes.Branch	|	(int)Virtual_Machine.BranchOperations.Break
			};

			m_codeStream = m_codeStream.Concat(postScript).ToList();
		}
	}
}
