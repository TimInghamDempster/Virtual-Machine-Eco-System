using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Compiler
{
    class DebugInfo
    {
        public string Source { get; set; }

        public int Address { get; set; }
    }
    class CodeGenerator
	{
		private List<int> _codeStream = new List<int>();
        private Dictionary<string, VariableEntry> _variableTable;
        private Dictionary<string, Tag> _tagTable;
        private int _jumpTagCount = 0;

        // If we are generate a code block embedded in a larger
        // structure then our tag locations shouldn't start at
        // 0, this variable records the actual start location
        private int _codeOffset = 0;

        private List<DebugInfo> _sourceLines = new List<DebugInfo>();
        private List<Token> _tokens;

        // Some types of nodes in the AST only provides meta-data
        // and so doesn't need any processing, rather than list all
        // as exceptions in the top level generate function, just have
        // a set here
        private readonly HashSet<ASTType> _metaNodeTypes =
            new HashSet<ASTType>()
            {
                ASTType.Comment,
                ASTType.UninitialisedBoolDeclaration,
                ASTType.UninitialisedIntDeclaration,
                ASTType.BoolDecleration,
                ASTType.IntDecleration
            };

        public List<int> GenerateCode(
            SyntaxNode abstractSyntaxTree,
            Dictionary<string, VariableEntry> variableTable,
            Dictionary<string, Tag> tagTable,
            List<Token> tokens)
        {

            // Make sure r0 = 0
            var initRegInstruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.SetLiteral;
            _codeStream.Add(initRegInstruction);
            _codeStream.Add(0);

            var pushAndStoreInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.PushAndStore;
            var pushInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.Push;

            foreach (VariableEntry variable in variableTable.Values)
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

            GenerateCodeBlock(abstractSyntaxTree, variableTable, tagTable, tokens, 0);

            WriteTagValuesIntoCodestream();

            AddPostScript();

            WriteDebugInfo();

            return _codeStream;
        }

        private void WriteDebugInfo()
        {
           using (var debugfile = new StreamWriter("Debug.dbg"))
           {
                foreach(var debugInfo in _sourceLines)
                {
                    debugfile.WriteLine(debugInfo.Address);
                    debugfile.WriteLine(debugInfo.Source);
                }

                debugfile.Close();
           }

        }

        public List<int> GenerateCodeBlock(
            SyntaxNode abstractSyntaxTree,
            Dictionary<string, VariableEntry>
            variableTable, Dictionary<string, Tag> tagTable,
            List<Token> tokens,
            int offset)
		{
            _variableTable = variableTable;
            _tagTable = tagTable;
            _tokens = tokens;
            _codeOffset = offset;

            int previousStartToken = 0;
            foreach (SyntaxNode node in abstractSyntaxTree.Children)
            {
                var endToken = node.StartTokenId;
                var address = _codeStream.Count;

                if (node.Type == ASTType.Assignment)
                {
                    GenerateAssignment(node);
                }
                else if (node.Type == ASTType.ASM)
                {
                    GenerateASM(node);
                }
                else if (node.Type == ASTType.Tag)
                {
                    SetTagLocation(node.Data);
                }
                else if (node.Type == ASTType.Conditional)
                {
                    GenerateConditional(node);
                }
                else if (!_metaNodeTypes.Contains(node.Type))
                {
                    throw new NotImplementedException();
                }
                
                string sourceString = "";
                for (int tokenIndex = previousStartToken; tokenIndex < endToken; tokenIndex++)
                {
                    sourceString += _tokens[tokenIndex].Source;
                }

                var splitStrings = sourceString.Split('\r');

                foreach (var debugString in splitStrings)
                {
                    _sourceLines.Add(
                        new DebugInfo()
                        {
                            Source = debugString,
                            Address = address
                        });
                }

                previousStartToken = endToken;
            }

            return _codeStream;
		}

        private void GenerateConditional(SyntaxNode node)
        {
            string jumpTagName = GenerateNewTag();

            // Generate the expression in the if statement, its value will
            // be written into r3
            GenerateBooleanExpression(node.Children.First());

            // Set r0 to the end of the code block
            SetRegisterToTagValue(jumpTagName, 0);

            // Set r2 to 0 for comparison purposes
            int storeInstructions = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.SetLiteral | 2 << 8 | 0;
            _codeStream.Add(storeInstructions);
            _codeStream.Add(0);

            // If the result of the expression is 0, jump to the tag (if not execution continues)
            int jumpInstruction = (int)Virtual_Machine.UnitCodes.Branch | (int)Virtual_Machine.BranchOperations.JumpEqualRegister | 3 << 8 | 2;
            _codeStream.Add(jumpInstruction);
            _codeStream.Add(0);

            // Now generate the code block and write it into the stream
            const int codeBlockIndex = 1;
            var subGenerator = new CodeGenerator();
            var codeBlock = 
                subGenerator.
                GenerateCodeBlock(
                    node.Children[codeBlockIndex],
                    _variableTable, 
                    _tagTable,
                    _tokens,
                    _codeStream.Count + _codeOffset);

            _codeStream = _codeStream.Concat(codeBlock).ToList();

            // And now create the tag to jump to if the condition is false
            SetTagLocation(jumpTagName);
        }

        private string GenerateNewTag()
        {
            // We want to jump past the code block if the condition is false,
            // but we don't know where the end of the block is yet, so create
            // a tag which will be filled in by the tag pass later
            var jumpTagName = "GeneratedJumpTag" + _jumpTagCount.ToString();
            _jumpTagCount++;
            _tagTable.Add(jumpTagName, new Tag());
            return jumpTagName;
        }

        private void GenerateBooleanExpression(SyntaxNode node)
        {
            GenerateBooleanSubExpression(node.Children.First());

            // An expression can have many sub-expressions
            if (node.Children.Count > 1)
            {
                var indexOfCurrentChild = 1;
                while (indexOfCurrentChild < node.Children.Count)
                {
                    // Save the value of the previous sub-expression for later!
                    GenerateStackPushAndStore(0);

                    // Now evaluate the next sub-expression
                    GenerateBooleanSubExpression(node.Children[indexOfCurrentChild + 2]);

                    // Move the result of the latest sub-expression so that we can
                    // restore the previous result
                    GenerateRegisterSwap(0, 1);
                    // Restore the result of the previous expression
                    GenerateStackPopAndLoad(0);

                    // Now combine the two results based on the specified conjunction
                    GenerateBooleanConjunction(node.Children[indexOfCurrentChild + 1]);
                    indexOfCurrentChild += 2;
                }
            }
        }

        private void GenerateStackPopAndLoad(int targetRegister)
        {
            int popInstruction = 
                (int)Virtual_Machine.UnitCodes.Stack | 
                (int)Virtual_Machine.StackOperations.PopAndLoad;

            _codeStream.Add(popInstruction);
            _codeStream.Add(targetRegister);
        }

        private void GenerateRegisterSwap(int sourceRegister, int targetRegister)
        {
            int swapInstruction =
                (int)Virtual_Machine.UnitCodes.ALU |
                (int)Virtual_Machine.ALUOperations.Copy |
                targetRegister << 8 |
                sourceRegister;

            _codeStream.Add(swapInstruction);
            _codeStream.Add(0);
        }

        private void GenerateStackPushAndStore(int sourceRegister)
        {
            int popInstruction = 
                (int)Virtual_Machine.UnitCodes.Stack | 
                (int)Virtual_Machine.StackOperations.PushAndStore;

            _codeStream.Add(popInstruction);
            _codeStream.Add(sourceRegister);
        }

        private void GenerateBooleanConjunction(SyntaxNode syntaxNode)
        {
            throw new NotImplementedException();
        }

        private void GenerateBooleanSubExpression(SyntaxNode syntaxNode)
        {
            var nodeType = syntaxNode.Type;

            // Literal bool, trivial
            if (nodeType == ASTType.BoolValue)
            {
                if (syntaxNode.Data == "true")
                {
                    SetRegister(0, 1);
                }
                else if(syntaxNode.Data == "false")
                {
                    SetRegister(0, 0);
                }
            }
            else if (nodeType == ASTType.BoolSubExpression)
            {
                GeneratePrimitive(syntaxNode.Children[0], 0, true);
                GeneratePrimitive(syntaxNode.Children[1], 1, true);

                GenerateComparison(syntaxNode.Data);
            }
            else if(nodeType == ASTType.BoolExpression)
            {
                GenerateBooleanExpression(syntaxNode);
            }
        }

        private void GenerateComparison(string condition)
        {
            const int intermediateResultRegister = 3;
            const int firstOperandRegister = 0;
            const int secondOperandRegister = 1;
            const int jumpTargetRegister = 2;
            var jumpTag = GenerateNewTag();

            // Tag will contain a location at the end of the
            // code block generated in this function
            SetRegisterToTagValue(jumpTag, jumpTargetRegister);

            // Set the result to true
            SetRegister(intermediateResultRegister, 1);

            // Jump out of this generated code if the result is true
            GenerateConditionalJump(condition, firstOperandRegister, secondOperandRegister, jumpTargetRegister);
            
            // Else set the result to false
            SetRegister(intermediateResultRegister, 0);

            SetTagLocation(jumpTag);
        }

        private void GenerateConditionalJump(
            string condition, 
            int firstRegister,
            int secondRegister,
            int targetRegister)
        {
            int instruction = (int)Virtual_Machine.UnitCodes.Branch;

            switch(condition)
            {
                case "==":
                    instruction |= (int)Virtual_Machine.BranchOperations.JumpEqualRegister;
                    break;
                case "<":
                    instruction |= (int)Virtual_Machine.BranchOperations.JumpLessRegister;
                    break;
                case "<=":
                    instruction |= (int)Virtual_Machine.BranchOperations.JumpLessEqualRegister;
                    break;
                case "!=":
                    instruction |= (int)Virtual_Machine.BranchOperations.JumpNotEqualRegister;
                    break;
            }

            instruction |= firstRegister << 8;
            instruction |= secondRegister;

            _codeStream.Add(instruction);
            _codeStream.Add(targetRegister);
        }

        private void SetRegister(int target, int value)
        {
            int instruction =
                (int)Virtual_Machine.UnitCodes.ALU |
                (int)Virtual_Machine.ALUOperations.SetLiteral |
                target << 8;

            _codeStream.Add(instruction);
            _codeStream.Add(value);
        }

        private void SetTagLocation(string tagName)
        {
            _tagTable[tagName].Value = _codeStream.Count;
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
            // Some ASTNodes can only ever have one child, and so get
            // added directly to their parent's child collection to
            // reduce redundant layers and nodes in the AST
            if (expressionNode.Type == ASTType.Primitive 
                || expressionNode.Type == ASTType.Tag
                || expressionNode.Type == ASTType.VariableName)
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
            else if(primitiveNode.Type == ASTType.IntExpression)
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
                SetRegisterToTagValue(primitiveNode.Data, targetRegister);
            }
            else
            {
                throw new NotImplementedException("Primitive type not recognised");
            }
		}

        private void SetRegisterToTagValue(string tagName, int targetRegister)
        {
            int instruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.SetLiteral | targetRegister << 8;

            _codeStream.Add(instruction);
            _codeStream.Add(0);
            _tagTable[tagName].InstructionLocations.Add(_codeStream.Count + _codeOffset - 1);
        }

        void WriteTagValuesIntoCodestream()
        {
            foreach (var tag in _tagTable.Values)
            {
                foreach (var tagInstructionLocation in tag.InstructionLocations)
                {
                    if (tag.Value == 0)
                    {
                        Console.WriteLine("Error: tag " + tag.Name + " not found");
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
