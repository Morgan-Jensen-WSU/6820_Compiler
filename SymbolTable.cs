using System.Collections.Generic;
using System.Linq;
using System;

namespace compiler
{
    public class SymbolTable
    {
        public static string[] VarTypes = { "num" };
        public static List<NumVariable> Variables = new List<NumVariable>();
        public static List<string> DataSection = new List<string>();
        public static List<string> BssSection = new List<string>();

        public SymbolTable()
        {

        }

        public bool IsVarType(string text)
        {
            return Array.Exists(VarTypes, vT => vT == text);
        }

        public bool IsVariableNameUsed(string name)
        {
            return Variables.Exists(v => v.Name == name);
        }

        public void DeclareVariable(string name)
        {
            Variables.Add(new NumVariable(name));
        }

        public void SetVariable(string name, string value)
        {
            var query = Variables.FirstOrDefault(v => v.Name == name);
            query.Value = value;
        }

        public NumVariable GetVariableByName(string name)
        {
            return Variables.FirstOrDefault(v => v.Name == name);
        }
    }
}