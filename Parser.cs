using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace compiler
{
    public class Parser
    {
        private List<char> Input = new List<char>();
        private SymbolTable MyTable;

        private List<string> Lines = new List<string>();

        Regex ProgramName = new Regex(@"^program[a-zA-Z0-9]+");
        Regex DeclareNum = new Regex(@"^num[a-zA-Z0-9]+");
        Regex Assigning = new Regex(@"^[a-zA-Z0-9]+=[a-zA-Z0-9]+");
        Regex NumConst = new Regex(@"^[0-9]+$");
        Regex StringConst = new Regex("\".+");
        Regex Addition = new Regex(@"^[a-zA-Z0-9]+\+[a-zA-Z0-9]+");
        Regex Subtraction = new Regex(@"^[a-zA-Z0-9]+\-[a-zA-Z0-9]+");
        Regex Multiplication = new Regex(@"^[a-zA-Z0-9]+\*[a-zA-Z0-9]+");
        Regex Division = new Regex(@"^[a-zA-Z0-9]+\/[a-zA-Z0-9]+");
        Regex Exponent = new Regex(@"^[a-zA-Z0-9]+\^[a-zA-Z0-9]+");
        Regex Writing = new Regex(@"^write.");

        private bool InString = false;

        /// <summary>
        /// Reads the file into Input
        /// </summary>
        public Parser(string inputFile)
        {
            MyTable = new SymbolTable();

            using (StreamReader reader = new StreamReader(inputFile))
            {
                do
                {
                    Input.Add((char)reader.Read());
                }
                while (!reader.EndOfStream);
            }

            FilterComments();
        }

        /// <summary>
        /// Parses through instructions and converts into x86
        /// </summary>
        public void Parse()
        {
            int iter = 0; // pointer to current character
            StringBuilder builder = new StringBuilder();

            while (true)
            {
                // get 1 line without white space 
                while (Input[iter] != ';' && Input[iter] != '\n' && iter <= Input.Count - 1)
                {
                    
                    if (Input[iter] != ' ' && Input[iter] != '\t' && Input[iter] != '\n')
                    {
                        builder.Append(Input[iter]);
                    }
                    if (Input[iter] == '\"')
                    {
                        InString = !InString;
                    }

                    if (Input[iter] == ' ' && InString)
                    {
                        builder.Append(Input[iter]);
                    }

                    iter++;
                    if (iter == Input.Count) break;
                }
                iter++;
                if (builder.ToString() != "") Lines.Add(builder.ToString());
                if (builder.ToString() == "end.") break;
                builder.Clear();
            }


            foreach (var line in Lines)
            {
                if (ProgramName.IsMatch(line))
                {
                    Program.ProgramName = line.Substring(7);
                }
                else if (DeclareNum.IsMatch(line))
                {
                    string varName = line.Substring(3);

                    if (!Assigning.IsMatch(varName))    // num num1;
                    {
                        MyTable.DeclareVariable(varName, VarType.NumVar);
                        SymbolTable.BssSection.Add($"{varName}\tresq\t1\t; an int");
                    }
                    else
                    {
                        Assign(varName);
                    }
                }
                else if (Assigning.IsMatch(line)) // num2 = something
                {
                    Assign(line);
                }
                else if (Writing.IsMatch(line))
                {
                    string printValue = line.Substring(5);

                    if (NumConst.IsMatch(printValue))
                    {
                        string asm;
                        asm = $"\tmov\trax, {printValue}\n";
                        asm += $"\tcall\tprintInt\n";
                        SymbolTable.TextSection.Add(asm);
                    }
                    else if (line.Contains('\"'))
                    {
                        MyTable.AddConstString(printValue);

                        string asm;
                        asm = $"\tmov\trax, {MyTable.GetConstStringName(printValue)}\n";
                        asm += $"\tcall printString\n";
                        SymbolTable.TextSection.Add(asm);
                    }
                    else
                    {
                        if (MyTable.IsVariableSet(printValue))
                        {
                            string asm;
                            asm = $"\tmov\trax, [qword {printValue}]\n";
                            asm += $"\tcall printInt\n";
                            SymbolTable.TextSection.Add(asm);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Parses through file and removes all comments
        /// </summary>
        private void FilterComments()
        {
            for (int i = 0; i < Input.Count; i++)
            {
                if (Input[i] == '/')
                {
                    if (Input[i + 1] == '/')
                    {
                        int iter = i;
                        while (Input[iter] != '\n')
                        {
                            iter++;
                        }
                        Input.RemoveRange(i, iter - i);
                    }
                    else if (Input[i + 1] == '*')
                    {
                        int iter = i + 2; // jump past the *
                        while (true)
                        {
                            iter++;
                            if (Input[iter] == '*' && Input[iter + 1] == '/')
                            {
                                iter += 2; // jump past the '*/'
                                break;
                            }
                        }
                        Input.RemoveRange(i, iter - i);
                    }
                }
            }
        }

        /// <summary>
        /// Parses out a string into varaible assignment
        /// </summary>
        /// <param name="varValue">Value to be parsed through.</param>
        private void Assign(string varValue)
        {
            int equalsSignIndex = 0;
            for (int i = 0; i < varValue.Length; i++)
            {
                if (varValue[i] == '=')
                {
                    equalsSignIndex = i;
                    break;
                }
            }

            string realVarName = varValue.Substring(0, equalsSignIndex);
            string assignValue = varValue.Substring(equalsSignIndex + 1);

            if (NumConst.IsMatch(assignValue))  // num num2 = 3;
            {
                MyTable.DeclareVariable(realVarName, VarType.NumVar);
                MyTable.SetVariable(realVarName);
                SymbolTable.DataSection.Add($"{realVarName}\tdd\t{assignValue}");
            }
            else    // num num2 = some math
            {
                SymbolTable.BssSection.Add($"{realVarName}\tresq\t1\t; an int");
                if (Addition.IsMatch(assignValue))  // num2 = x + y
                {
                    int operatorIndex = 0;
                    for (int i = 0; i < assignValue.Length; i++)
                    {
                        if (assignValue[i] == '+')
                        {
                            operatorIndex = i;
                            break;
                        }
                    }

                    string value1 = assignValue.Substring(0, operatorIndex);
                    string value2 = assignValue.Substring(operatorIndex + 1);

                    VarType value1Type = MyTable.GetVarType(value1);
                    VarType value2Type = MyTable.GetVarType(value2);

                    string asm;

                    if (value1Type == VarType.NumVar && value2Type == VarType.NumVar)   // num1 + num2
                    {
                        asm = $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\tadd\trax, [qword {value2}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.NumVar && value2Type == VarType.ConstNum) // num1 + 10
                    {
                        asm = $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\tadd\trax, {value2}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.ConstNum && value2Type == VarType.NumVar)    // 10 + num1
                    {
                        asm = $"\tmov\trax, [qword {value2}]\n";
                        asm += $"\tadd\trax, {value1}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else    // 10 + 5
                    {
                        asm = $"\tmov\trax, {value1}\n";
                        asm += $"\tadd\trax, {value2}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }

                    SymbolTable.TextSection.Add(asm);
                }
                else if (Subtraction.IsMatch(assignValue))  // num2 = x - y
                {
                    int operatorIndex = 0;
                    for (int i = 0; i < assignValue.Length; i++)
                    {
                        if (assignValue[i] == '-')
                        {
                            operatorIndex = i;
                            break;
                        }
                    }

                    string value1 = assignValue.Substring(0, operatorIndex);
                    string value2 = assignValue.Substring(operatorIndex + 1);

                    VarType value1Type = MyTable.GetVarType(value1);
                    VarType value2Type = MyTable.GetVarType(value2);

                    string asm;

                    if (value1Type == VarType.NumVar && value2Type == VarType.NumVar)   // num1 - num2
                    {
                        asm = $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\tsub\trax, [qword {value2}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.NumVar && value2Type == VarType.ConstNum) // num1 - 10
                    {
                        asm = $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\tsub\trax, {value2}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.ConstNum && value2Type == VarType.NumVar)    // 10 - num1
                    {
                        asm = $"\tmov\trax, {value1}\n";
                        asm += $"\tsub\trax, [qword {value2}]\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else    // 10 - 5
                    {
                        asm = $"\tmov\trax, {value1}\n";
                        asm += $"\tsub\trax, {value2}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }

                    SymbolTable.TextSection.Add(asm);
                }
                else if (Multiplication.IsMatch(assignValue))   // num2 = x * y
                {
                    int operatorIndex = 0;
                    for (int i = 0; i < assignValue.Length; i++)
                    {
                        if (assignValue[i] == '*')
                        {
                            operatorIndex = i;
                            break;
                        }
                    }

                    string value1 = assignValue.Substring(0, operatorIndex);
                    string value2 = assignValue.Substring(operatorIndex + 1);

                    VarType value1Type = MyTable.GetVarType(value1);
                    VarType value2Type = MyTable.GetVarType(value2);

                    string asm;

                    if (value1Type == VarType.NumVar && value2Type == VarType.NumVar)   // num1 * num2
                    {
                        asm = $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\timul\trax, [qword {value2}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.NumVar && value2Type == VarType.ConstNum) // num1 * 10
                    {
                        asm = $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\timul\trax, {value2}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.ConstNum && value2Type == VarType.NumVar)    // 10 * num1
                    {
                        asm = $"\tmov\trax, [qword {value2}]\n";
                        asm += $"\timul\trax, {value1}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else    // 10 * 5
                    {
                        asm = $"\tmov\trax, {value1}\n";
                        asm += $"\timul\trax, {value2}\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }

                    SymbolTable.TextSection.Add(asm);
                }
                else if (Division.IsMatch(assignValue))     // num2 = x / y
                {
                    int operatorIndex = 0;
                    for (int i = 0; i < assignValue.Length; i++)
                    {
                        if (assignValue[i] == '/')
                        {
                            operatorIndex = i;
                            break;
                        }
                    }

                    string value1 = assignValue.Substring(0, operatorIndex);
                    string value2 = assignValue.Substring(operatorIndex + 1);

                    VarType value1Type = MyTable.GetVarType(value1);
                    VarType value2Type = MyTable.GetVarType(value2);

                    string asm;

                    if (value1Type == VarType.NumVar && value2Type == VarType.NumVar)   // num1 / num2
                    {
                        asm = $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\tmov\trbl, [qword {value2}\n";
                        asm += $"\tidiv\trbl\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.NumVar && value2Type == VarType.ConstNum) // num1 / 10
                    {
                        asm = $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\tmov\trbl, {value2}\n";
                        asm += $"\tidiv\trbl\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.ConstNum && value2Type == VarType.NumVar)    // 10 / num1
                    {
                        asm = $"\tmov\trax, {value1}\n";
                        asm += $"\tmov\trbl, [qword {value2}]\n";
                        asm += $"\tidiv\trbl\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else    // 10 / 5
                    {
                        asm = $"\tmov\trax, {value1}\n";
                        asm += $"\tmov\trbl, {value2}\n";
                        asm += $"\tidiv\trbl\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }

                    SymbolTable.TextSection.Add(asm);
                }
                else if (Exponent.IsMatch(assignValue)) // num2 = x ^ y
                {
                    int operatorIndex = 0;
                    for (int i = 0; i < assignValue.Length; i++)
                    {
                        if (assignValue[i] == '^')
                        {
                            operatorIndex = i;
                            break;
                        }
                    }

                    string value1 = assignValue.Substring(0, operatorIndex);
                    string value2 = assignValue.Substring(operatorIndex + 1);

                    VarType value1Type = MyTable.GetVarType(value1);
                    VarType value2Type = MyTable.GetVarType(value2);

                    string asm;

                    if (value1Type == VarType.NumVar && value2Type == VarType.NumVar)   // num1 ^ num2
                    {
                        asm =  "\tmov\trdi, 1\n";
                        asm += $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\tmov\trdx, [qword {value2}]\n";
                        asm += "exp_start:\n";
                        asm += "\tcmp\trdi, rdx\n";
                        asm += "\tjz exp_done\n";
                        asm += $"\timul\trax, [qword {value1}]\n";
                        asm += "\tinc\trdi\n";
                        asm += "\tjmp\texp_start\n";
                        asm += "exp_done:\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.NumVar && value2Type == VarType.ConstNum) // num1 ^ 10
                    {
                        asm =  "\tmov\trdi, 1\n";
                        asm += $"\tmov\trax, [qword {value1}]\n";
                        asm += $"\tmov\trdx, {value2}\n";
                        asm += "exp_start:\n";
                        asm += "\tcmp\trdi, rdx\n";
                        asm += "\tjz exp_done\n";
                        asm += $"\timul\trax, [qword {value1}]\n";
                        asm += "\tinc\trdi\n";
                        asm += "\tjmp\texp_start\n";
                        asm += "exp_done:\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else if (value1Type == VarType.ConstNum && value2Type == VarType.NumVar)    // 10 ^ num1
                    {
                        asm =  "\tmov\trdi, 1\n";
                        asm += $"\tmov\trax, {value1}\n";
                        asm += $"\tmov\trdx, [qword {value2}]\n";
                        asm += "exp_start:\n";
                        asm += "\tcmp\trdi, rdx\n";
                        asm += "\tjz exp_done\n";
                        asm += $"\timul\trax, {value1}\n";
                        asm += "\tinc\trdi\n";
                        asm += "\tjmp\texp_start\n";
                        asm += "exp_done:\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }
                    else    // 10 ^ 5
                    {
                        asm =  "\tmov\trdi, 1\n";
                        asm += $"\tmov\trax, {value1}\n";
                        asm += $"\tmov\trdx, {value2}\n";
                        asm += "exp_start:\n";
                        asm += "\tcmp\trdi, rdx\n";
                        asm += "\tjz exp_done\n";
                        asm += $"\timul\trax, {value1}\n";
                        asm += "\tinc\trdi\n";
                        asm += "\tjmp\texp_start\n";
                        asm += "exp_done:\n";
                        asm += $"\tmov\t[qword {realVarName}], rax\n";
                    }

                    SymbolTable.TextSection.Add(asm);
                }
                
                MyTable.DeclareVariable(realVarName, VarType.NumVar);
                MyTable.SetVariable(realVarName);
            }
        }
    }
}