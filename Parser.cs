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
        Regex AssignNum = new Regex(@"^[a-zA-Z0-9]+=[a-zA-Z0-9]+");
        Regex NumConst = new Regex(@"^[0-9]+$");
        Regex Addition = new Regex(@"^[a-zA-Z0-9]+\+");


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
                    if (MyTable.IsVariableNameUsed(varName))
                    {
                        Error.ThrowError("A variable already has that name.");
                    }
                    else 
                    {
                        if (!AssignNum.IsMatch(varName))    // num num1;
                        {
                            MyTable.DeclareVariable(varName);
                            SymbolTable.BssSection.Add($"{varName}\tresq\t1\t; an int");
                        }
                        else
                        {
                            int equalsSign = 0;
                            for (int i = 0; i < varName.Length; i++)
                            {
                                if (varName[i] == '=')
                                {
                                    equalsSign = i;
                                }
                            }

                            string realVarName = varName.Substring(0, equalsSign);
                            string assignValue = varName.Substring(equalsSign + 1);
                            
                            if (NumConst.IsMatch(assignValue))  // num num2 = 3;
                            {
                                MyTable.DeclareVariable(realVarName);
                                MyTable.SetVariable(realVarName, assignValue);
                                SymbolTable.DataSection.Add($"{realVarName}\tdd\t{assignValue}");
                            }
                            else
                            {
                                SymbolTable.BssSection.Add($"{realVarName}\tresq\t1\t; an int");
                            }
                            
                        }
                    }
                    
                }

                Console.WriteLine(line);
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
    }
}