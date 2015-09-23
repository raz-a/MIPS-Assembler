/*
    OPCode
    By: Raz Aloni
    Class representing an OPCode
*/

namespace MIPSAssembler
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Enumeration of the Opcode types
    /// </summary>
    public enum OperationType
    {
        [XmlEnum]
        R_Type,

        [XmlEnum]
        I_Type,

        [XmlEnum]
        J_Type
    };

    /// <summary>
    /// Opcode
    /// </summary>
    [XmlType]
    public class OpCode
    {
        /// <summary>
        /// Name of the OPCode
        /// </summary>
        [XmlAttribute]
        public string Name
        {
            get;
            set;
        }
        
        /// <summary>
        /// Type of the OpCode
        /// </summary>
        [XmlElement("Type")]
        public OperationType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Value of the OPCode Field within the instruction
        /// </summary>
        [XmlElement]
        public string OP
        {
            get;
            set;
        }

        /// <summary>
        /// Value of the  Func Field within the instruction
        /// </summary>
        [XmlElement]
        public string Funct
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not the SHAMT is used
        /// </summary>
        [XmlElement]
        public bool Shift
        {
            get;
            set;
        }

        [XmlIgnore]
        public uint MachineCodeValue
        {
            get
            {
                uint opcode = Convert.ToUInt32(OP,2) << 26;
                opcode += (string.IsNullOrEmpty(Funct)) ? 0 : Convert.ToUInt32(Funct, 2);
                return opcode;
            }
        }

    }
}
