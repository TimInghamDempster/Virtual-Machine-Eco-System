using System;
using System.Collections.Generic;
using System.Linq;

namespace Assembler
{
    public class Assembler
    {
        /// <summary>
        /// Converts a program in assembly format into a list of binary instructions.  Called by
        /// the compiler to turn the generated asm into a binary listing.
        /// </summary>
        public static List<int> ParseString(string asm)
        {
            // One instruction per line
            string[] lines = asm.Split('\n');

            // Used for error reporting
            UInt32 lineCount = 0;
            List<int> binaryStream = new List<int>();

            foreach (var line in lines)
            {
                lineCount++;

                // The components of an instruction can be split by spaces or tabs
                string[] parts = SplitLine(line);

                // Generate the binary for this line
                ParseExecutionUnit(parts, lineCount, binaryStream);
            }

            return binaryStream;
        }

        public static string[] SplitLine(string line)
        {
            List<string> strings = new List<string>();
            strings.Add("");
            int count = 0;

            bool inString = true;
            foreach (char c in line)
            {
                if (c != ' ' && c != '\t' && c != '\r')
                {
                    strings[count] += c;
                    inString = true;
                }
                else
                {
                    if (inString)
                    {
                        strings.Add("");
                        count++;
                    }

                    inString = false;
                }
            }

            return strings.ToArray();
        }

        public static int ParseExecutionUnit(string[] parts, UInt32 line, List<int> binaryStream)
        {
            if (parts.Count() == 0)
            {
                // Empty string
                return 0;
            }

            switch (parts[0])
            {
                case "ALU":
                    {
                        ParseALU(parts, line, binaryStream);
                    }
                    break;
                case "Branch":
                    {
                        ParseBranch(parts, line, binaryStream);
                    }
                    break;
                case "Interrupt":
                    {
                        ParseInterrupt(parts, line, binaryStream);
                    }
                    break;
                case "Load":
                    {
                        ParseLoad(parts, line, binaryStream);
                    }
                    break;
                case "Store":
                    {
                        ParseStore(parts, line, binaryStream);
                    }
                    break;
                case "Stack":
                    {
                        ParseStack(parts, line, binaryStream);
                    }
                    break;
                case "Data":
                    {
                        ParseData(parts, line, binaryStream);
                    }
                    break;
                case @"//": // Comment line
                case "": // Blank line
                case "<": // Closing tag from asm inlined into source
                    break;
                default:
                    {
                        Console.WriteLine("Error, didn't recognise instruction type " + parts[0] + " on line: " + line.ToString());
                    }
                    break;
            }
            return 0;
        }
        static void ParseData(string[] parts, UInt32 line, List<int> binaryStream)
        {
            try
            {
                int data = Convert.ToInt32(parts[1], 16);
                binaryStream.Add(data);
            }
            catch
            {
                Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
            }
            return;
        }

        static void ParseBranch(string[] parts, UInt32 line, List<int> binaryStream)
        {
            int instruction0 = (int)Virtual_Machine.UnitCodes.Branch;
            int instruction1 = 0;

            if (parts.Count() < 5)
            {
                Console.WriteLine("Error, not enough arguments on line " + line.ToString() + ", should be 5");
                return;
            }

            switch (parts[1])
            {
                case "Nop":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.Nop;
                    }
                    break;
                case "Jump":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.Jump;
                        var success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "JumpRegister":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.JumpRegister;
                        int reg1;
                        var success = int.TryParse(parts[3], out reg1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= reg1;

                        success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "JumpNotEqual":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.JumpNotEqual;

                        int reg1;
                        var success = int.TryParse(parts[2], out reg1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= reg1;

                        int reg2;
                        success = int.TryParse(parts[3], out reg2);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= (reg2 << 8);

                        success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "JumpEqual":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.JumpEqual;

                        int reg1;
                        var success = int.TryParse(parts[2], out reg1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= reg1;

                        int reg2;
                        success = int.TryParse(parts[3], out reg2);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= (reg2 << 8);

                        success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "JumpLess":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.JumpLess;

                        int reg1;
                        var success = int.TryParse(parts[2], out reg1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= reg1;

                        int reg2;
                        success = int.TryParse(parts[3], out reg2);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= (reg2 << 8);

                        success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "JumpLessEqual":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.JumpLessEqual;

                        int reg1;
                        var success = int.TryParse(parts[2], out reg1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= reg1;

                        int reg2;
                        success = int.TryParse(parts[3], out reg2);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= (reg2 << 8);

                        success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "JumpNotEqualRegister":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.JumpNotEqualRegister;

                        int reg1;
                        var success = int.TryParse(parts[2], out reg1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= reg1;

                        int reg2;
                        success = int.TryParse(parts[3], out reg2);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= (reg2 << 8);

                        success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "JumpEqualRegister":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.JumpEqualRegister;

                        int reg1;
                        var success = int.TryParse(parts[2], out reg1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= reg1;

                        int reg2;
                        success = int.TryParse(parts[3], out reg2);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= (reg2 << 8);

                        success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "JumpLessRegister":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.JumpLessRegister;

                        int reg1;
                        var success = int.TryParse(parts[2], out reg1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= reg1;

                        int reg2;
                        success = int.TryParse(parts[3], out reg2);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= (reg2 << 8);

                        success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "JumpLessEqualRegister":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.JumpLessEqualRegister;

                        int reg1;
                        var success = int.TryParse(parts[2], out reg1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= reg1;

                        int reg2;
                        success = int.TryParse(parts[3], out reg2);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                        instruction0 |= (reg2 << 8);

                        success = int.TryParse(parts[4], out instruction1);
                        if (!success)
                        {
                            Console.WriteLine("Error, failed to convert argument to int on line: " + line.ToString());
                            return;
                        }
                    }
                    break;
                case "Break":
                    {
                        instruction0 |= (int)Virtual_Machine.BranchOperations.Break;
                    }
                    break;
                default:
                    {
                        Console.WriteLine("Error, did not recognise branch instruction on line: " + line.ToString());
                    }
                    break;
            }
            binaryStream.Add(instruction0);
            binaryStream.Add(instruction1);
        }
        static void ParseInterrupt(string[] parts, UInt32 line, List<int> binaryStream)
        {
            int instruction0 = (int)Virtual_Machine.UnitCodes.Interrupt;
            int instruction1 = 0;

            switch(parts[1])
            {
                case "InterruptReturn":
                    instruction0 |= (int)Virtual_Machine.InterruptInstructions.InterruptReturn;
                    break;
            }

            binaryStream.Add(instruction0);
            binaryStream.Add(instruction1);
        }
        static void ParseLoad(string[] parts, UInt32 line, List<int> binaryStream)
        {
            int instruction0 = (int)Virtual_Machine.UnitCodes.Load;
            int instruction1 = 0;

            switch (parts[1])
            {
                case "LoadFromRegisterLocation":
                    {
                        if (parts.Count() < 5)
                        {
                            Console.WriteLine("Error, not enough arguments on line " + line.ToString() + ", should be 5");
                            return;
                        }

                        instruction0 |= (int)Virtual_Machine.LoadOperations.LoadFromRegisterLocation;
                        int reg;
                        bool parsed = int.TryParse(parts[2], out reg);

                        if (parsed == false || reg > 15)
                        {
                            Console.WriteLine("Error, third argument on line " + line + " must be a uint less than sixteen");
                            return;
                        }
                        instruction0 |= reg << 8;

                        parsed = int.TryParse(parts[3], out reg);

                        if (parsed == false || reg > 15)
                        {
                            Console.WriteLine("Error, fourth argument on line " + line + " must be a uint less than sixteen");
                            return;
                        }
                        instruction0 |= reg;

                        int offset;
                        parsed = int.TryParse(parts[4], out offset);

                        if (parsed == false)
                        {
                            Console.WriteLine("Error, fourth argument on line " + line + " must be a uint");
                            return;
                        }
                        instruction1 = offset;

                    }
                    break;
                case "LoadFromLiteralLocation":
                    {
                        if (parts.Count() < 5)
                        {
                            Console.WriteLine("Error, not enough arguments on line " + line.ToString() + ", should be 4");
                            return;
                        }

                        instruction0 |= (int)Virtual_Machine.LoadOperations.LoadFromLiteralLocation;

                        int reg;
                        bool parsed = int.TryParse(parts[2], out reg);

                        if (parsed == false || reg > 15)
                        {
                            Console.WriteLine("Error, third argument on line " + line + " must be a uint less than sixteen");
                            return;
                        }
                        instruction0 |= reg << 8;

                        parsed = int.TryParse(parts[3], out reg);

                        if (parsed == false || reg > 15)
                        {
                            Console.WriteLine("Error, fourth argument on line " + line + " must be a uint less than sixteen");
                            return;
                        }
                        instruction0 |= reg;
                        int offset;

                        parsed = int.TryParse(parts[4], out offset);

                        if (parsed == false)
                        {
                            Console.WriteLine("Error, third argument on line " + line + " must be a uint");
                            return;
                        }
                        instruction1 = offset;
                    }
                    break;
                default:
                    {
                        Console.WriteLine("Error, Load instruction \"" + parts[1] + "\" on line " + line.ToString() + " not recognised");
                        return;
                    }
            }
            binaryStream.Add(instruction0);
            binaryStream.Add(instruction1);
        }
        static void ParseStore(string[] parts, UInt32 line, List<int> binaryStream)
        {

            int instruction0 = (int)Virtual_Machine.UnitCodes.Store;
            int instruction1 = 0;

            switch (parts[1])
            {

                case "StoreToRegisterLocation":
                    {
                        if (parts.Count() < 5)
                        {
                            Console.WriteLine("Error, not enough arguments on line " + line.ToString() + ", should be 5");
                            return;
                        }

                        instruction0 |= (int)Virtual_Machine.StoreOperations.StoreToRegisterLocation;
                        int reg;
                        bool parsed = int.TryParse(parts[2], out reg);

                        if (parsed == false || reg > 15)
                        {
                            Console.WriteLine("Error, third argument on line " + line + " must be a uint less than sixteen");
                            return;
                        }
                        instruction0 |= reg << 8;

                        parsed = int.TryParse(parts[3], out reg);

                        if (parsed == false || reg > 15)
                        {
                            Console.WriteLine("Error, fourth argument on line " + line + " must be a uint less than sixteen");
                            return;
                        }
                        instruction0 |= reg;

                        int offset;
                        parsed = int.TryParse(parts[4], out offset);

                        if (parsed == false)
                        {
                            Console.WriteLine("Error, fourth argument on line " + line + " must be a uint");
                            return;
                        }
                        instruction1 = offset;

                    }
                    break;
                case "StoreToLiteralLocation":
                    {
                        if (parts.Count() < 5)
                        {
                            Console.WriteLine("Error, not enough arguments on line " + line.ToString() + ", should be 4");
                            return;
                        }

                        instruction0 |= (int)Virtual_Machine.StoreOperations.StoreToLiteralLocation;

                        int reg;
                        bool parsed = int.TryParse(parts[2], out reg);

                        if (parsed == false || reg > 15)
                        {
                            Console.WriteLine("Error, third argument on line " + line + " must be a uint less than sixteen");
                            return;
                        }
                        instruction0 |= reg << 8;

                        parsed = int.TryParse(parts[3], out reg);

                        if (parsed == false || reg > 15)
                        {
                            Console.WriteLine("Error, fourth argument on line " + line + " must be a uint less than sixteen");
                            return;
                        }
                        instruction0 |= reg;
                        int offset;

                        parsed = int.TryParse(parts[4], out offset);

                        if (parsed == false)
                        {
                            Console.WriteLine("Error, third argument on line " + line + " must be a uint");
                            return;
                        }
                        instruction1 = offset;
                    }
                    break;
                default:
                    {
                        Console.WriteLine("Error, Store instruction \"" + parts[1] + "\" on line " + line.ToString() + " not recognised");
                        return;
                    }
            }
            binaryStream.Add(instruction0);
            binaryStream.Add(instruction1);
        }
        static void ParseStack(string[] parts, UInt32 line, List<int> binaryStream)
        {

            int instruction0 = (int)Virtual_Machine.UnitCodes.Stack;
            int instruction1 = 0;

            if (parts.Count() < 3)
            {
                Console.WriteLine("Error, not enough arguments on line " + line.ToString() + ", should be 3");
                return;
            }

            switch (parts[1])
            {
                case "Push":
                {
                    instruction0 |= (int)Virtual_Machine.StackOperations.Push;
                }
                break;
                case "Pop":
                {
                    instruction0 |= (int)Virtual_Machine.StackOperations.Pop;
                }
                break;
                case "PushAndStore":
                {
                    instruction0 |= (int)Virtual_Machine.StackOperations.PushAndStore;
                }
                break;
                case "PopAndLoad":
                {
                    instruction0 |= (int)Virtual_Machine.StackOperations.PopAndLoad;
                }
                break;
                default:
                {
                    Console.WriteLine("Error, Stack instruction \"" + parts[1] + "\" on line " + line.ToString() + " not recognised");
                    return;
                }
            }

            int reg;
            bool parsed = int.TryParse(parts[2], out reg);

            if (parsed == false || reg > 15)
            {
                Console.WriteLine("Error, third argument on line " + line + " must be a uint less than sixteen");
                return;
            }
            instruction1 = reg;

            binaryStream.Add(instruction0);
            binaryStream.Add(instruction1);
        }

        static void ParseALU(string[] parts, UInt32 line, List<int> binaryStream)
        {
            int instruction0 = (int)Virtual_Machine.UnitCodes.ALU;
            int instruction1 = 0;

            if (parts.Count() < 5)
            {
                Console.WriteLine("Error, not enough arguments on line " + line.ToString() + ", should be 5");
                return;
            }

            switch (parts[1])
            {
                case "Add":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.Add;
                    }
                    break;
                case "AddLiteral":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.AddLiteral;
                    }
                    break;
                case "Sub":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.Subtract;
                    }
                    break;
                case "SubtractLiteral":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.SubtractLiteral;
                    }
                    break;
                case "Mul":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.Multiply;
                    }
                    break;
                case "MulLiteral":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.MultiplyLiteral;
                    }
                    break;
                case "Div":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.Divide;
                    }
                    break;
                case "DivLiteral":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.DivideLiteral;
                    }
                    break;
                case "Copy":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.Copy;
                    }
                    break;
                case "SetLiteral":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.SetLiteral;
                    }
                    break;
                case "Nop":
                    {
                        instruction0 |= (int)Virtual_Machine.ALUOperations.Nop;
                    }
                    break;
                default:
                    {
                        Console.WriteLine("Error, ALU instruction \"" + parts[1] + "\" on line " + line.ToString() + " not recognised");
                        return;
                    }
            }

            int register0 = 0;
            int register1 = 0;
            bool test = int.TryParse(parts[2], out register0);
            if (!test || register0 > 255)
            {
                Console.WriteLine("Error, third argument on line " + line.ToString() + " must be UInt8.");
                return;
            }

            test = int.TryParse(parts[3], out register1);
            if (!test || register1 > 255)
            {
                Console.WriteLine("Error, fourth argument on line " + line.ToString() + " must be UInt8.");
                return;
            }

            test = int.TryParse(parts[4], out instruction1);
            if (!test)
            {
                Console.WriteLine("Error, fifth argument on line " + line.ToString() + " must be UInt32.");
                return;
            }

            instruction0 |= register0 << 8;
            instruction0 |= register1;

            binaryStream.Add(instruction0);
            binaryStream.Add(instruction1);
        }
    }
}
