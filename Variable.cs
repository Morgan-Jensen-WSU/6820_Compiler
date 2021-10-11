
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

        public Variable(string name, VarType type)
        {
            Name = name;
            IsAssigned = false;
            Type = type;
            Value = "";
        }

        public Variable(string name, string value)
        {
            Name = name;
            IsAssigned = true;
            Type = VarType.ConstString;
            Value = value;
        }
    }
}
