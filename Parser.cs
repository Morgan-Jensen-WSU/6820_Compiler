using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Text.RegularExpressions;

{
    
}
public class Parser
{
    private List<char> Input = new List<char>();
    private SymbolTable MyTable;

    private Regex DeclareNum = new Regex(@"^num [a-zA-Z0-9]$", RegexOptions.IgnorePatternWhitespace);
    private Regex DeclareAndAssignNum = new Regex(@"^num [a-zA-Z0-9]+ \= [0-9]+$", RegexOptions.IgnorePatternWhitespace);
    private Regex DeclaredAndAssignAdd = new Regex(@"^num [a-zA-Z0-9]+ \= [a-zA-Z0-9}+ \+ [0-9]+", RegexOptions.IgnorePatternWhitespace);

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

        while (builder.ToString() != "end.")
        {
            // get 1 line
            while (Input[iter] != ';')
            {
                builder.Append(Input[iter]);
                iter++;
            }

            string line = builder.ToString();

            // num num1;
            if (DeclareNum.IsMatch(line))
            {
                string[] words = line.Split(' ');
                
                CheckAndDeclareVariable(words[0], words[1]);
            }
            // num num1 = 5;
            else if (DeclareAndAssignNum.IsMatch(line))
            {
                string[] words = line.Split(' ');
                
                CheckAndDeclareVariable(words[0], words[1]);
                CheckAndAssignVariable(words[1], words[3]);
            }
            // num num1 = num 2 + 5;
            else if (DeclaredAndAssignAdd.IsMatch(line))
            {
                string[] words = line.Split(' ');                
            }

        }
    }

    private void CheckAndDeclareVariable(string type, string name)
    {
        if (MyTable.IsVarType(type))
        {
            if (!MyTable.IsVariableNameUsed(name)) MyTable.DeclareVariable(name);
            else Error.ThrowError($"{name} is already declared.");
        }
        else
        {
            Error.ThrowError($"{type} is not a valid variable type.");
        }
    }

    private void CheckAndAssignVariable(string name, string value)
    {
        if (MyTable.IsVariableNameUsed(name))
        {
            MyTable.SetVariable(name, Int32.Parse(value));
        }
        else 
        {
            Error.ThrowError($"Need to declare {name} before assigning it.");
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