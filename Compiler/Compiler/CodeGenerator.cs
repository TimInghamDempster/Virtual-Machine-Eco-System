using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
class CodeGenerator
	{
		private List<int> _codeStream;
        private Dictionary<string, VariableEntry> _variableTable;
        private Dictionary<string, Tag> _tagTable;

		public List<int> GenerateCode(SyntaxNode abstractSyntaxTree, Dictionary<string, VariableEntry> variableTable, Dictionary<string, Tag> tagTable)
		{
			_codeStream = new List<int>();
            _variableTable = variableTable;
            _tagTable = tagTable;

            // Make sure r0 = 0
            var initRegInstruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.SetLiteral;
            _codeStream.Add(initRegInstruction);
            _codeStream.Add(0);

            var pushAndStoreInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.PushAndStore;
            var pushInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.Push;

            foreach (VariableEntry variable in _variableTable.Values)
            {
                if (variable.Initialised)
                {
                    // Initialise the variable to whatever is in reg0 (which we just set to 0)
                    _codeStream.Add(pushAndStoreInstruction);
                    _codeStream.Add(0);
                }
                else
                {
                    // Reserve space for variable on stack without storing anything, much faster
                    // but causes uninitalised variables
                    _codeStream.Add(pushInstruction);
                    _codeStream.Add(0);
                }
            }

            foreach (SyntaxNode node in abstractSyntaxTree.Children)
            {
                if (node.Type == ASTType.Assignment)
                {
                    GenerateAssignment(node);
                }
                else if(node.Type == ASTType.ASM)
                {
                    GenerateASM(node);
                }
                else if (node.Type == ASTType.Tag)
                {
                    SetTagLocation(node);
                }
            }

            WriteTagValuesIntoCodestream();

			AddPostScript();
            return _codeStream;
		}

        private void SetTagLocation(SyntaxNode node)
        {
            _tagTable[node.Data].Value = _codeStream.Count;
        }

        private void GenerateASM(SyntaxNode node)
        {
            _codeStream.AddRange(Assembler.Assembler.ParseString(node.Data));
        }

        private void GenerateAssignment(SyntaxNode node)
        {
            GenerateExpression(node.Children[0]);

            var storeAddress = _variableTable[node.Data].Address;
            var storeInstruction = (int)Virtual_Machine.UnitCodes.Store | (int)Virtual_Machine.StoreOperations.StoreToLiteralLocation;
            _codeStream.Add(storeInstruction);
            _codeStream.Add(storeAddress);
        }

		void GenerateExpression(SyntaxNode  expressionNode)
		{
            if (expressionNode.Type == ASTType.Primitive || expressionNode.Type == ASTType.Tag)
            {
                GeneratePrimitive(expressionNode, 0, true);
            }
            else
            {
                GeneratePrimitive(expressionNode.Children[0], 0, true);

                for (int i = 1; i < expressionNode.Children.Count; i += 2)
                {
                    GeneratePrimitive(expressionNode.Children[i + 1], 1, false);
                    GenerateBinaryOperator(expressionNode.Children[i], 0, 1);
                }
            }
        }

		void GenerateBinaryOperator(SyntaxNode operatorNode, int targetRegister, int sourceRegister)
		{
			int instruction = targetRegister << 8 | targetRegister;
			int argument = sourceRegister;

			switch(operatorNode.Type)
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

			_codeStream.Add(instruction);
			_codeStream.Add(argument);
		}

		void GenerateUnaryOperator(SyntaxNode unaryNode, int targetRegister)
		{
			GeneratePrimitive(unaryNode.Children[0], targetRegister, false);
		}

		void GeneratePrimitive(SyntaxNode primitiveNode, int targetRegister, bool firstPrimitiveInExpression)
		{
			
			if(primitiveNode.Type == ASTType.Primitive)
			{
				int val;
				int.TryParse(primitiveNode.Data, out val);

				int instruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.SetLiteral | targetRegister << 8;

				_codeStream.Add(instruction);
				_codeStream.Add(val);
			}
			else if(primitiveNode.Type == ASTType.UnaryMinus)
			{
				if (primitiveNode.Children[0].Type == ASTType.Primitive)
				{
					GenerateUnaryOperator(primitiveNode, targetRegister);

					int negativeInstruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.MultiplyLiteral | targetRegister << 8 | targetRegister;
					_codeStream.Add(negativeInstruction);
					_codeStream.Add(-1);
				}
            }
            else if (primitiveNode.Type == ASTType.VariableName)
            {
                int address = _variableTable[primitiveNode.Data].Address;
                int loadInstruction = (int)Virtual_Machine.UnitCodes.Load | (int)Virtual_Machine.LoadOperations.LoadFromLiteralLocation | targetRegister << 8;
                _codeStream.Add(loadInstruction);
                _codeStream.Add(address);
            }
            else if(primitiveNode.Type == ASTType.Expression)
            {
                if (!firstPrimitiveInExpression)
                {
                    int pushInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.PushAndStore;
                    _codeStream.Add(pushInstruction);
                    _codeStream.Add(0);
                }

                GenerateExpression(primitiveNode);

                if (!firstPrimitiveInExpression)
                {
                    int copyInstruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Copy | 1 << 8 | 0;
                    _codeStream.Add(copyInstruction);
                    _codeStream.Add(0);

                    int popInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.PopAndLoad;
                    _codeStream.Add(popInstruction);
                    _codeStream.Add(0);
                }
            }
            else if(primitiveNode.Type == ASTType.Tag)
            {
                int instruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.SetLiteral | targetRegister << 8;

                _codeStream.Add(instruction);
                _codeStream.Add(0);
                _tagTable[primitiveNode.Data].InstructionLocations.Add(_codeStream.Count - 1);
            }
            else
            {
                throw new NotImplementedException("Primitive type not recognised");
            }
		}

        void WriteTagValuesIntoCodestream()
        {
            foreach (var tag in _tagTable.Values)
            {
                foreach (var tagInstructionLocation in tag.InstructionLocations)
                {
                    if (tag.Value == 0)
                    {
                        Console.WriteLine("Error: tag not found");
                    }
                    _codeStream[tagInstructionLocation] = tag.Value + (int)Virtual_Machine.VirtualMachine.RAMStartAddress;
                }
            }
        }

		void AddPostScript()
		{
			List<int> postScript = new List<int>()
			{
				(int)Virtual_Machine.UnitCodes.Branch	|	(int)Virtual_Machine.BranchOperations.Break
			};

			_codeStream = _codeStream.Concat(postScript).ToList();
		}
	}
}
