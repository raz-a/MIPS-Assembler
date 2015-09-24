/*
    MIPS Assembler
    By: Raz Aloni
    A Basic MIPS Assembler and Disassembler
*/

namespace MIPSAssembler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// A Basic MIPS Assembler and Disassembler
    /// </summary>
    public static class MIPSAssembler
    {
        #region Constants

        /// <summary>
        /// Dictionary of Register Convention names
        /// </summary>
        internal static Dictionary<string, uint> registerDictionary = new Dictionary<string, uint>
        {
            {"zero", 0 }, {"at", 1 }, {"v0", 2 }, {"v1", 3 }, {"a0", 4 }, {"a1", 5 }, {"a2", 6 }, {"a3", 7 },
            {"t0", 8 }, {"t1", 9 }, {"t2", 10 }, {"t3", 11 }, {"t4", 12 }, {"t5", 13 }, {"t6", 14 }, {"t7", 15 },
            {"s0", 16 }, {"s1", 17 }, {"s2", 18 }, {"s3", 19 }, {"s4", 20 }, {"s5", 21 }, {"s6", 22 }, {"s7", 23 },
            {"t8", 24 }, {"t9", 25 }, {"k0", 26 }, {"k1", 27 }, {"gp", 28 }, {"sp", 29 }, {"fp", 30 }, {"ra", 31 }
        };

        /// <summary>
        /// Mif file header
        /// </summary>
        internal static string MifHeader = "\nWIDTH=32;\nDEPTH=256;\n\nADDRESS_RADIX=HEX;\nDATA_RADIX=HEX;\n\nCONTENT BEGIN";

        #endregion

        #region Globals

        /// <summary>
        /// List of useable Opcodes
        /// </summary>
        internal static List<OpCode> OpCodeLibrary;

        /// <summary>
        /// Labels obtained from first Parse of Assembly
        /// </summary>
        internal static Dictionary<string, uint> labels;

        #endregion

        /// <summary>
        /// Main Execution
        /// </summary>
        /// <param name="args">CommandLine Arguments</param>
        public static void Main(string[] args)
        {
            #region GetArgs

            AppArguments cmdArgs = new AppArguments();
            var helpScreen = HelpText.AutoBuild(cmdArgs);

            if (args.Length == 0 || args[0].Contains("help"))
            {
                Console.WriteLine(helpScreen);
                Environment.Exit(0);
            }

            bool argsReadSuccess = Parser.Default.ParseArguments(args, cmdArgs);

            if (!argsReadSuccess)
            {
                Console.Error.Write("Required argument \"in\" was not specified");
                Environment.Exit(1);
            }

            #endregion

            // Generate the OpCodeLibrary
            GetOpCodes();

            try
            {
                #region DoWork

                // Open Source File Stream
                using (StreamReader sourceFile = new StreamReader(cmdArgs.SourcePath))
                {
                    // Check if destination was specified
                    if (string.IsNullOrEmpty(cmdArgs.DestinationPath))
                    {
                        cmdArgs.DestinationPath = "output.txt";
                    }

                    // Open Destination File Stream
                    using (StreamWriter destinationFile = new StreamWriter(cmdArgs.DestinationPath))
                    {
                        destinationFile.AutoFlush = true;

                        if (cmdArgs.Disassemble)
                        {
                            DisassembleMIPS(sourceFile, destinationFile);
                        }
                        else
                        {
                            AssembleMIPS(sourceFile, destinationFile);
                        }

                        Console.WriteLine("Success!");
                    }
                }

                #endregion
            }
            catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Gets a dictionary of OpCodes from OpCodeLibrary.xml
        /// </summary>
        private static void GetOpCodes()
        {
            using (StreamReader xmlOCL = new StreamReader("OpCodeLibrary.xml"))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(List<OpCode>), new XmlRootAttribute("OpCodeLibrary"));
                List <OpCode> OpCodes = (List<OpCode>)deserializer.Deserialize(xmlOCL);

                OpCodeLibrary = new List<OpCode>();

                foreach (var oc in OpCodes)
                {
                    OpCodeLibrary.Add(oc);
                }
            }
        }

        #region Disassemble

        /// <summary>
        /// Disassemble MIPS binary into Assembly
        /// </summary>
        /// <param name="source">Binary Stream</param>
        /// <param name="destination">Assembly Stream</param>
        private static void DisassembleMIPS(StreamReader source, StreamWriter destination)
        {
            // Read up until CONTENT BEGIN
            while (source.ReadLine() != "CONTENT BEGIN");

            uint address = 0;
            string line;
            while (!(line = source.ReadLine()).Contains("END"))
            {
                // Get binary
                line = line.Split(new char[] { ':', ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries)[1];


                line = Convert.ToString(Convert.ToUInt32(line, 16), 2).PadLeft(32, '0');

                string op = line.Substring(0, 6);
                string funct = line.Substring(26);

                OpCode OpCode = (op.Equals("000000")) ? OpCodeLibrary.Find(oc => oc.Funct == funct) : OpCodeLibrary.Find(oc => oc.OP == op);

                if (OpCode == null)
                {
                    throw new UnsupportedOpCodeException(op, address);
                }

                string assembly = string.Empty;

                uint rs = Convert.ToUInt32(line.Substring(6, 5), 2);
                uint rt = Convert.ToUInt32(line.Substring(11, 5), 2);
                uint rd = Convert.ToUInt32(line.Substring(16, 5), 2);
                uint shamt = Convert.ToUInt32(line.Substring(21, 5), 2);
                short immediate = Convert.ToInt16(line.Substring(16, 16), 2);

                switch (OpCode.Type)
                {
                    case OperationType.R_Type:

                        // Check if it is a shift option
                        if (OpCode.Shift)
                        {
                            assembly = string.Format("{0} ${1}, ${2}, {3}", OpCode.Name, rd, rt, shamt);
                        }
                        else if(OpCode.Name.StartsWith("j", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Jump instruction
                            assembly = string.Format("{0} ${1}", OpCode.Name, rs);
                        }
                        else
                        {
                            assembly = string.Format("{0} ${1}, ${2}, ${3}", OpCode.Name, rd, rs, rt);
                        }
                        break;

                    case OperationType.I_Type:

                        if (OpCode.Name.StartsWith("b", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Branch
                            assembly = string.Format("{0} ${1}, ${2}, 0x{3:X}", OpCode.Name, rs, rt, immediate);
                        }
                        else if (OpCode.Name.Contains("lui"))
                        {
                            assembly = string.Format("{0} ${1}, 0x{2:X}", OpCode.Name, rt, immediate);
                        }
                        else if (OpCode.Name.EndsWith("i", StringComparison.InvariantCultureIgnoreCase) || OpCode.Name.EndsWith("iu", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Immediate operation
                            assembly = string.Format("{0} ${1}, ${2}, 0x{3:X}", OpCode.Name, rt, rs, immediate);
                        }
                        else
                        {
                            // Load/Store
                            assembly = string.Format("{0} ${1} {3}(${2})", OpCode.Name, rt, rs, immediate);
                        }

                        break;

                    case OperationType.J_Type:
                        assembly = string.Format("{0} 0x{1:X}", OpCode.Name, Convert.ToUInt32(line.Substring(6), 2));
                        break;
                }

                destination.WriteLine(assembly);

            }

        }

        #endregion

        #region Assemble

        /// <summary>
        /// Assemble MIPS Assembly into Machine Code
        /// </summary>
        /// <param name="source">Assembly Stream</param>
        /// <param name="destination">Maching Code Stream</param>
        private static void AssembleMIPS(StreamReader source, StreamWriter destination)
        {
            labels = new Dictionary<string, uint>();

            // First Parse. Get Labels
            parseAssembly(source, 
                (instructionComponents, address) =>
                {
                    // Label can only be in first "column" and must end with a :
                    if (instructionComponents[0].EndsWith(":"))
                    {
                        labels.Add(instructionComponents[0].TrimEnd(':'), address);
                    }
                });

            // Place Header
            destination.WriteLine(MifHeader);

            // Second Parse, Assemble Code
            parseAssembly(source, (instructionComponents, address) => AssembleLine(instructionComponents, address, destination));

            destination.Write("END;");
        }

        /// <summary>
        /// Parses the given Assembly stream
        /// </summary>
        /// <param name="assemblyReader"></param>
        /// <param name="processLine">Method that performs an action on a line of code</param>
        /// <returns>Dictionary of labels with their corresponding addresses</returns>
        private static void parseAssembly(StreamReader assemblyReader, Action<List<string>, uint> processLine)
        {
            // Save original position of Stream
            long originalStreamPosition = assemblyReader.BaseStream.Position;

            // Move to top of Stream
            assemblyReader.BaseStream.Position = 0;

            // Start at address 0
            uint address = 0;

            while (!assemblyReader.EndOfStream)
            {
                // Split on '#' to ignore comments
                string line = assemblyReader.ReadLine().Split('#')[0].Trim(' ', '\t');

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                
                // Split by spaces and columns to get items in the line
                List<string> instructionComponents = new List<string>(line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries));

                processLine(instructionComponents, address);

                if (instructionComponents.Count > 1)
                {
                        address ++;
                }
            }

            // Return to original position of Stream (just being polite, you know?)
            assemblyReader.BaseStream.Position = originalStreamPosition;
        }

        /// <summary>
        /// Assembles one line of MIPS code
        /// </summary>
        /// <param name="instructionComponents">Components of the line of code</param>
        /// <param name="address">Current Address</param>
        /// <param name="destination">Location to write maching code</param>
        private static void AssembleLine(List<string> instructionComponents, uint address, StreamWriter destination)
        {
            // Check if label
            if (instructionComponents[0].EndsWith(":"))
            {
                instructionComponents.RemoveAt(0);

                // Check if instruction is in line
                if (instructionComponents.Count == 0)
                {
                    return;
                }
            }

            OpCode OpCode = OpCodeLibrary.Find(oc => oc.Name == instructionComponents[0]);

            // Check if opcode exists
            if (OpCode == null)
            {
                throw new UnsupportedOpCodeException(instructionComponents[0], address);
            }

            uint machineCode = OpCode.MachineCodeValue;

            switch (OpCode.Type)
            {
                case OperationType.R_Type:

                    if (instructionComponents.Count == 4)
                    {
                        // 3 Arguments
                        // rd
                        machineCode |= GetRegCode(instructionComponents[1]) << 11;

                        // Check if last value is a shift ammount
                        if (OpCode.Shift)
                        {
                            // rt
                            machineCode |= GetRegCode(instructionComponents[2]) << 16;

                            // Shamt
                            machineCode |= GetImmediateCode(instructionComponents[3], 5) << 6;
                        }
                        else
                        {
                            // rs
                            machineCode |= GetRegCode(instructionComponents[2]) << 21;

                            // rt
                            machineCode |= GetRegCode(instructionComponents[3]) << 16;
                        }
                    }
                    else if (instructionComponents.Count == 2)
                    {
                        // 1 Argument (jr)
                        // rs
                        machineCode |= GetRegCode(instructionComponents[1]) << 21;
                    }
                    else
                    {
                        // Invalid number of registers
                        throw new InstructionFormatException("Invalid number of operands", address);
                    }

                    break;

                case OperationType.I_Type:

                    if (instructionComponents.Count == 4)
                    {
                        // 3 Arguments
                        // Check if a branch
                        if (OpCode.Name.StartsWith("b", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // rs
                            machineCode |= GetRegCode(instructionComponents[1]) << 21;

                            // rt
                            machineCode |= GetRegCode(instructionComponents[2]) << 16;

                            // Label 
                            uint label;

                            if (!labels.TryGetValue(instructionComponents[3], out label))
                            {
                                label = GetImmediateCode(instructionComponents[3], 16);
                            }
                            else
                            {
                                label -= (address + 1);
                            }

                            machineCode |= (label & 0x0000FFFF);
                        }
                        else
                        {
                            // rt 
                            machineCode |= GetRegCode(instructionComponents[1]) << 16;

                            // rs
                            machineCode |= GetRegCode(instructionComponents[2]) << 21;

                            // Immediate
                            machineCode |= GetImmediateCode(instructionComponents[3], 16);
                        }
                        
                    }
                    else if (instructionComponents.Count == 3)
                    {
                        // 2 Arguments

                        // rt
                        machineCode |= GetRegCode(instructionComponents[1]) << 16;

                        // Check if load/store
                        if (instructionComponents[2].Contains("("))
                        {
                            string[] regPlusOffset = instructionComponents[2].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

                            if (regPlusOffset.Length != 2)
                            {
                                throw new InstructionFormatException("Load/Store register+offset is syntactically incorrect");
                            }

                            // rs
                            machineCode |= GetRegCode(regPlusOffset[1]) << 21;

                            // Immediate
                            machineCode |= GetImmediateCode(regPlusOffset[0], 16);
                        }
                        else
                        {
                            machineCode |= GetImmediateCode(instructionComponents[2], 16);
                        }
                    }
                    else
                    {
                        // Invalid number of registers
                        throw new InstructionFormatException("Invalid number of operands", address);
                    }

                    break;

                case OperationType.J_Type:

                    if (instructionComponents.Count != 2)
                    {
                        // Invalid number of instructions
                        throw new InstructionFormatException("Invalid number of operands", address);
                    }

                    uint jumpValue;
                    
                    if (!labels.TryGetValue(instructionComponents[1], out jumpValue))
                    {
                        jumpValue = GetImmediateCode(instructionComponents[1], 26);
                    }

                    machineCode |= jumpValue;
                    break;
            }

            destination.WriteLine("\t{0:x3}  :   {1:X8};", address, machineCode);
        }

        /// <summary>
        /// Gets the Machine Code Value from a register string component
        /// </summary>
        /// <param name="regString">Register to parse</param>
        /// <returns>Machine code uint</returns>
        private static uint GetRegCode(string regString)
        {
            string regValue = regString.TrimStart('$');
            uint reg;

            if (!uint.TryParse(regValue, out reg))
            {
                if (!registerDictionary.TryGetValue(regValue, out reg))
                {
                    throw new InvalidRegisterException(regValue);
                }
            }
            else
            {
                if (reg < 0 || reg > 31)
                {
                    throw new InvalidRegisterException(regValue);
                }
            }

            return reg;
        }

        /// <summary>
        /// Gets the Machine Code Value from an immediate string component
        /// </summary>
        /// <param name="immString">Immediate to parse</param>
        /// <param name="numBits">number of bits allowed</param>
        /// <returns>Machine code uint</returns>
        private static uint GetImmediateCode(string immString, uint numBits)
        {
            int immediate;

            if (!(int.TryParse(immString, out immediate) || 
                  int.TryParse(immString, System.Globalization.NumberStyles.HexNumber, null, out immediate) ||
                  int.TryParse(immString.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out immediate)
                ))
            {
                throw new InvalidCastException("Invalid immediate value " + immString);
            }

            // Zero out top bits
            immediate &= (int)(Math.Pow(2, numBits) - 1);

            if (immediate > Math.Pow(2,numBits) - 1)
            {
                // Immediate is out of range
                throw new ArgumentOutOfRangeException("regString");
            }

            // Get rid of top bits
            return (uint)(immediate);
        }

        #endregion
    }
}
