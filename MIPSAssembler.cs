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
    using CommandLine;
    using CommandLine.Text;
    
    /// <summary>
    /// A Basic MIPS Assembler and Disassembler
    /// </summary>
    public static class MIPSAssembler
    {
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

            // Open Source File Stream
            using (FileStream sourceFile = new FileStream(cmdArgs.SourcePath, FileMode.Open, FileAccess.Read))
            {
                // Check if destination was specified
                if (string.IsNullOrEmpty(cmdArgs.DestinationPath))
                {
                    cmdArgs.DestinationPath = "output.txt";
                }

                // Open Destination File Stream
                using (FileStream destinationFile = new FileStream(cmdArgs.DestinationPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    if (cmdArgs.Disassemble)
                    {
                        DisassembleMIPS(sourceFile, destinationFile);
                    }
                    else
                    {
                        AssembleMIPS(sourceFile, destinationFile);
                    }
                }
            }
        }

        /// <summary>
        /// Disassemble MIPS binary into Assembly
        /// </summary>
        /// <param name="source">Binary Stream</param>
        /// <param name="destination">Assembly Stream</param>
        private static void DisassembleMIPS(Stream source, Stream destination)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Assemble MIPS Assembly into Machine Code
        /// </summary>
        /// <param name="source">Assembly Stream</param>
        /// <param name="destination">Maching Code Stream</param>
        private static void AssembleMIPS(Stream source, Stream destination)
        {
            Dictionary<string, uint> labels = getLabels(source);

            int x = 7;
        }

        /// <summary>
        /// Parses the labels from a given Assembly stream
        /// </summary>
        /// <param name="assemblyCode"></param>
        /// <returns>Dictionary of labels with their corresponding addresses</returns>
        private static Dictionary<string, uint> getLabels(Stream assemblyCode)
        {
            Dictionary<string, uint> labelDictionary = new Dictionary<string, uint>();

            // Move to top of Stream (just in case)
            assemblyCode.Position = 0;

            using (StreamReader assemblyReader = new StreamReader(assemblyCode))
            {
                // Start at address 0
                uint address = 0;

                while (!assemblyReader.EndOfStream)
                {
                    // Split on '#' to ignore comments
                    string line = assemblyReader.ReadLine().Split('#')[0];

                    // Label can only be in first "column" and must end with a :
                    // Split by spaces to get first item in line
                    string[] instructionComponents = line.Split(' ');

                    // Check if it ends in a semicolon
                    if (instructionComponents[0].EndsWith(":"))
                    {
                        labelDictionary.Add(instructionComponents[0].TrimEnd(':'), address);

                        // Check if label is on same line as instructions
                        if (instructionComponents.Length > 1)
                        {
                            address += 4;
                        }
                    }
                    else
                    {
                        address += 4;
                    }
                }
            }

            return labelDictionary;

        }
    }
}
