
namespace compiler
{
    public enum VarType
    {
        ErrType,
        ConstNum,
        ConstString, 
        NumVar,
    }
    public struct Variable
    {
        public string Name { get; set; }
        public bool IsAssigned { get; set; }
        public VarType Type { get; set; }
        public string Value { get; set; }

        /// <summary>
        /// Constructor for most variable types
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <param name="value">Type of the variable.</param>
        public Variable(string name, VarType type)
        {
            Name = name;
            IsAssigned = false;
            Type = type;
            Value = "";
        }

        /// <summary>
        /// Constructor for string literals.
        /// </summary>
        /// <param name="name">Name of the string literal.</param>
        /// <param name="value">Value of the string literal.</param>
        public Variable(string name, string value)
        {
            Name = name;
            IsAssigned = true;
            Type = VarType.ConstString;
            Value = value;
        }
    }
}
