using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace compiler
{
    public class OldParser
    {
        private List<char> Input = new List<char>();

        private List<IVariable> Variables = new List<IVariable>();

        private static string[] SymbolTable = { "num" };

        public OldParser(string inputFile)
        {
            // get text into Input
            StreamReader reader = new StreamReader(inputFile);
            do
            {
                Input.Add((char)reader.Read());
            } while (!reader.EndOfStream);
            reader.Close();
            reader.Dispose();

            FilterComments();
            Parse();
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
        /// Runs through Input until it finds end.
        /// Parses out different words
        /// </summary>
        private void Parse()
        {
            int index = 0;
            StringBuilder stringBuilder = new StringBuilder();

            while (stringBuilder.ToString() != "end.")
            {
                int iter = index;

                stringBuilder.Clear();
                SkipWhiteSpace(ref iter);

                // feed chars into stringBuilder until end of word (' ', ';', or new line is found)
                while (Input[iter] != ' ' && Input[iter] != ';' && Input[iter] != '\n')
                {
                    stringBuilder.Append(Input[iter]);
                    iter++;
                }

                // declaring program name
                if (stringBuilder.ToString() == "program")
                {
                    SkipWhiteSpace(ref iter);
                    stringBuilder.Clear();
                    while (Input[iter] != ';')
                    {
                        stringBuilder.Append(Input[iter]);
                        iter++;
                    }
                    iter++; // jump past ';'
                    Program.ProgramName = stringBuilder.ToString();
                }

                // declaring variables
                if (SymbolTable.Contains(stringBuilder.ToString()))
                {
                    while (Input[iter] != ';')
                    {
                        stringBuilder.Append(Input[iter]);
                        iter++;
                    }

                    // num num1 = 5
                    string[] words = stringBuilder.ToString().Split(' ');

                    DeclareVariable(words[1], words[0]);

                    if (words[2] == "=")
                    {
                        AssignVariable(words);
                    }
                }
                else if (IsVarNameUsed(stringBuilder.ToString()))
                {
                    while (Input[iter] != ';')
                    {
                        stringBuilder.Append(Input[iter]);
                        iter++;
                    }

                    string[] words = stringBuilder.ToString().Split(' ');

                    if (words[1] == "=")
                    {
                        AssignVariable(words);
                    }
                }

                // jump past white space
                SkipWhiteSpace(ref iter);
            }
        }

        /// <summary>
        /// Verifies validity of declared variable
        /// If valid, creates new variable and adds it to Variables list
        /// </summary>
        /// <param name="name">Name of variable to be set.</param>
        /// <param name="declareType">Type of variable to be set.</param>
        private void DeclareVariable(string declaredType, string name)
        {
            // throw error if they use a var type for a name
            if (SymbolTable.Contains(name))
            {
                Program.ThrowError("Variable cannot share a name with a variable type.");
                return;
            }

            // throw error if they already used that name
            if (IsVarNameUsed(name))
            {
                Program.ThrowError("A variable already exists with that name.");
                return;
            }

            switch (declaredType)
            {
                case "num":
                    Variables.Add(new IntVariable(name));
                    break;
                case null:
                    Program.ThrowError("Variable must have a name");
                    return;
            }
        }

        /// <summary>
        /// Takes a line of input and uses it to set the variable as indicated.
        /// </summary>
        /// <param name="line">Line of code to operate on.</param>
        private void AssignVariable(string[] line)
        {
            List<object> values = new List<object>();
            List<string> operators = new List<string>();
            int iter = 0;
            while (line[iter] != "=")
            {
                iter++;
            }
            iter++;

            while (line[iter] != ";")
            {
                int tempNum = 0;
                if (IsVarNameUsed(line[iter])) // value is a variable
                {
                    values.Add(GetValueFromName(line[iter]));
                }
                else if (int.TryParse(line[iter], out tempNum)) // value is a const num
                {
                    values.Add(tempNum);
                }
                else    // value is an operator
                {
                    operators.Add(line[iter]);
                }
                iter++;
            }

            int varIndex = Array.IndexOf(line, "=") - 1;
            IVariable varToAssign = GetVariable(line[varIndex]);

            if (varToAssign == null)
            {
                Program.ThrowError("Cannot assign a varaible before it is declared.");
            }

            if (operators.Count == 0) // if just assigning something
            {
                varToAssign.Value = values[0];
            }
            else // if doing some operation(s)_
            {
                int valToSet = (int)values[0];
                int subIter = 2;
                for (int i = 0; i < operators.Count; i++)
                {
                    if (operators[i] == "+")
                    {
                        valToSet += (int)values[subIter];
                    }
                    else if (operators[i] == "-")
                    {
                        valToSet -= (int)values[subIter];
                    }
                    else if (operators[i] == "*")
                    {
                        valToSet *= (int)values[subIter];
                    }
                    else if (operators[i] == "/")
                    {
                        valToSet /= (int)values[subIter];
                    }

                    subIter += 2;
                }

                varToAssign.Value = valToSet;
            }
        }

        /// <summary>
        /// Takes a name and returns any matching variable.
        /// <param name="name">Name of the variable to find.</param>
        /// <returns>Any variable with matching name. 
        /// If no variable exists with given name, returns NULL.</returns>
        private IVariable GetVariable(string name)
        {
            foreach (var variable in Variables)
            {
                if (variable.Name == name) return variable;
            }
            return null;
        }

        /// <summary>
        /// Takes the name of a variable, and returns its value.
        /// </summary>
        /// <param name="name">Name of the variable to find.</param>
        /// <returns>Value of the variable with given name. 
        /// If no variable exists with given name, returns NULL</returns>
        private object GetValueFromName(string name)
        {
            var variable = GetVariable(name);
            return variable.Value;
        }

        /// <summary>
        /// Checks to see if user is assigning at declaration time.
        /// </summary>
        /// <param name="currentIter">The current location in Input.</param>
        /// <returns>True if user is assigning at declaration, False otherwise.</returns>
        private bool IsAssigning(ref int currentIter)
        {

            // filter out white spaces in case = is father down
            SkipWhiteSpace(ref currentIter);
            bool isAssigning = Input[currentIter] == '=';
            ++currentIter;
            return isAssigning;
        }

        /// <summary>
        /// Checks to see if variable name has already been used
        /// </summary>
        /// <param name="name">Name of the variable to be used.</param>
        /// <returns>True if name used, False if not.</returns>
        private bool IsVarNameUsed(string name)
        {
            foreach (var variable in Variables)
            {
                if (variable.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Advances pointer iter to the next non-whitespace character.
        /// </summary>
        /// <param name="iter">Index to start at.</param>
        /// <returns>Location of next non-whitespace character.</returns>
        private void SkipWhiteSpace(ref int iter)
        {
            while (Input[iter] == ' ' || Input[iter] == '\n')
            {
                iter++;
            }
        }
    }
}
