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

        /// Initializer
        public SymbolTable()
        {
            ConstStringCount = 0;
        }

        /// <summary>
        /// Checks to see if there exists a variable with the given name.
        /// </summary>
        /// <param name="name">Name of the variable to be checked.</param>
        /// <returns>True if variable exists with given name, false otherwise.</returns>
        public bool IsVariableNameUsed(string name)
        {
            return Variables.Exists(v => v.Name == name);
        }

        /// <summary>
        /// Instaniates a new variable with given name and type
        /// </summary>
        /// <param name="name">Name of the variable to create.</param>
        /// <param name="type">Data type of the variable to create.</param>
        public void DeclareVariable(string name, VarType type)
        {
            if (IsVariableNameUsed(name))
            {
                Error.ThrowError("A variable already exists with that name");
            }
            Variables.Add(new Variable(name, type));
        }

        /// <summary>
        /// Changes the "IsAssigned" field of the given variable to true.
        /// <summary>
        /// <param name="name">Name of the variable to set.</param>
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
        
        /// <summary>
        /// Checks to see if variable with given name is set.
        /// </summary>
        /// <param name="name">Name of the variable to check.</param>
        /// <returns>True if there is a set variable with given name, false otherwise.</returns>
        public bool IsVariableSet(string name)
        {
            return Variables.FirstOrDefault(v => v.Name == name).IsAssigned;
        }

        /// <summary>
        /// Gets the type of given variable.
        /// </summary>
        /// <param name="name">Name of the variable to get the type of.</param>
        /// <returns>Variable type of the variable with given name.</returns>
        public VarType GetVarType(string name)
        {
            int i = 0;
            if (Int32.TryParse(name, out i)) return VarType.ConstNum; // if variable is just a number

            if (IsVariableSet(name)) return VarType.NumVar; // if variable is an assigned variable in table

            Error.ThrowError($"Variable {name} needs to be assigned before using it.");
            return VarType.ErrType;
        }

        /// <summary>
        /// Creates a string variable when user uses a string literal.
        /// </summary>
        /// <param name="value">Value of the string literal</param>
        public void AddConstString(string value)
        {
            Variables.Add(new Variable($"S{ConstStringCount}", value));
            DataSection.Add($"S{ConstStringCount}\t\tdb\t{value}, 0x0d, 0x0a, 0");
        }

        /// <summary>
        /// Gets the name of a string literal variable
        /// </summary>
        /// <param name="value">Value of string literal</param>
        public string GetConstStringName(string value)
        {
            return Variables.FirstOrDefault(v => v.Value == value).Name;
        }
    }
}