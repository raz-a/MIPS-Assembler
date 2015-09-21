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
        internal static string MifHeader = "WIDTH=32;\nDEPTH=256;\n\nADDRESS_RADIX=HEX;\nDATA_RADIX=HEX;\n\nCONTENT BEGIN";

        #endregion

        #region Globals

        /// <summary>
        /// Dictionary of useable Opcodes
        /// </summary>
        internal static Dictionary<string, OpCode> OpCodeLibrary;

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
                        if (cmdArgs.Disassemble)
                        {
                            DisassembleMIPS(sourceFile, destinationFile);
                        }
                        else
                        {
                            AssembleMIPS(sourceFile, destinationFile);
                            Console.WriteLine("Success!");
                        }
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

                OpCodeLibrary = new Dictionary<string, OpCode>();

                foreach (var oc in OpCodes)
                {
                    OpCodeLibrary.Add(oc.Name, oc);
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
            throw new NotImplementedException();
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
                        address += 4;
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


            OpCode OpCode;

            if (!OpCodeLibrary.TryGetValue(instructionComponents[0], out OpCode))
            {
                throw new UnsupportedOpCodeException(instructionComponents[0], address);
            }

            uint machineCode = OpCode.MachineCodeValue;

            switch (OpCode.Type)
            {
                case OperationType.R_Type:

                    List<uint> regs = new List<uint>(); 

                    for (int i = 0; i < instructionComponents.Count - 1; i++)
                    {
                        uint reg;

                        if (!uint.TryParse(instructionComponents[i+1].TrimStart('$'), out reg))
                        {
                            if (!registerDictionary.TryGetValue(instructionComponents[i+1].TrimStart('$'), out reg))
                            {
                                throw new InvalidRegisterException(instructionComponents[i+1], address);
                            }
                        }
                        else
                        {
                            if (reg < 0 || reg > 31)
                            {
                                throw new InvalidRegisterException(instructionComponents[i+1], address);
                            }
                        }

                        regs.Add(reg);
                    }

                    if (instructionComponents.Count == 4)
                    {
                        // Three Arguments
                        // rd
                        machineCode |= regs[0] << 11;

                        // Check if last value is an immediate
                        if (!instructionComponents[3].Contains("$"))
                        {
                            // rt
                            machineCode |= regs[1] << 16;

                            // Shamt
                            machineCode |= regs[2] << 6;
                        }
                        else
                        {
                            // rs
                            machineCode |= regs[1] << 21;

                            // rt
                            machineCode |= regs[2] << 16;
                        }
                    }
                    else if (instructionComponents.Count == 3)
                    {
                        // 2 Arguments
                        // rd
                        machineCode |= regs[0] << 11;

                        // Check if last value is an immediate
                        if (!instructionComponents[2].Contains("$"))
                        {
                            // rt
                            machineCode |= regs[0] << 16;

                            // Shamt
                            machineCode |= regs[1] << 6;
                        }
                        else
                        {
                            // rs
                            machineCode |= regs[0] << 21;

                            // rt
                            machineCode |= regs[1] << 16;
                        }
                    }
                    else if (instructionComponents.Count == 2)
                    {
                        // 1 Argument (jr)
                        // rs
                        machineCode |= regs[0];
                    }
                    else
                    {
                        // Invalid number of registers
                        throw new InstructionFormatException("Invalid number of operands", address);
                    }

                        break;

                case OperationType.I_Type:
                    break;

                case OperationType.J_Type:
                    break;
            }

            destination.WriteLine("\t{0:000}:{1:x8};", address >> 2, machineCode);
        }

        #endregion
    }
}
