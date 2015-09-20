/*
    AppArguments.cs
    By: Raz Aloni
    An object representation of the possible arguments that can be passed
    to the main console application.
*/

namespace MIPSAssembler
{
    using CommandLine;

    /// <summary>
    /// Class representation of the possible arguments that can be passed
    /// </summary>
    internal class AppArguments
    {
        /// <summary>
        /// Gets or set the location of the Source File
        /// </summary>
        [Option(shortName: 'i', longName: "in", HelpText = "The location of the Source File to Assembled/Disassembled", Required = true)]
        public string SourcePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location of the destination file
        /// </summary>
        [Option(shortName: 'o', longName: "out", HelpText = "The location for the proccesed file. If not specfied, it will be placed local to the executable", Required = false)]
        public string DestinationPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the file will be disassembled
        /// </summary>
        [Option(shortName: 'd', longName: "Disassemble", HelpText = "Disassemble the file", Required = false)]
        public bool Disassemble
        {
            get;
            set;
        }
    }
}
