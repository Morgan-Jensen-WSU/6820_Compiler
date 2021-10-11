using System.Collections.Generic;
using System.Linq;
using System;

namespace compiler
{
    public class SymbolTable
    {
        public static List<Variable> Variables = new List<Variable>();
        public static List<string> DataSection = new List<string>();
        public static List<string> BssSection = new List<string>();
        public static List<string> TextSection = new List<string>();

        public int ConstStringCount;

        public SymbolTable()
        {
            ConstStringCount = 0;
        }

        public bool IsVariableNameUsed(string name)
        {
            return Variables.Exists(v => v.Name == name);
        }

        public void DeclareVariable(string name, VarType type)
        {
            if (IsVariableNameUsed(name))
            {
                Error.ThrowError("A variable already exists with that name");
            }
            Variables.Add(new Variable(name, type));
        }

        public void SetVariable(string name)
        {
            var index = Variables.FindIndex(r => r.Name == name);

            if (index != -1)
            {
                Variable newVar = new Variable(name, Variables[index].Type);
                newVar.IsAssigned = true;
                Variables[index] = newVar;      
            }
        }

        public Variable GetVariableByName(string name)
        {
            return Variables.FirstOrDefault(v => v.Name == name);
        }
        
        public bool IsVariableSet(string name)
        {
            return Variables.FirstOrDefault(v => v.Name == name).IsAssigned;
        }

        public VarType GetVarType(string name)
        {
            int i = 0;
            if (Int32.TryParse(name, out i)) return VarType.ConstNum; // if variable is just a number

            if (IsVariableSet(name)) return VarType.NumVar; // if variable is an assigned variable in table

            Error.ThrowError($"Variable {name} needs to be assigned before using it.");
            return VarType.ErrType;
        }

        public void AddConstString(string value)
        {
            Variables.Add(new Variable($"S{ConstStringCount}", VarType.ConstString));
            DataSection.Add($"S{ConstStringCount}\t\tdb\t\"{value}\", 0x0d, 0x0a, 0");
        }
    }
}