using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace compiler
{
    public class Parser
    {
        private List<char> Input = new List<char>();
        private List<IVariable> Variables = new List<IVariable>();
        
        private static string[] SymbolTable = { "num" };

        public Parser(string inputFile)
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
                while(Input[iter] != ' ' || Input[iter] != ';' || Input[iter] == '\n')
                {
                    stringBuilder.Append(Input[iter]);
                    iter++;
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
                        AssignVariable(words[1], words[3]);
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
                    Variables.Add(new Variable<int>(name));
                    break;
                case null:
                    Program.ThrowError("Variable must have a name");
                    return;
            }
        }

        public void AssignVariable(string name, string value)
        {
            
        }

        /// <summary>
        /// Checks to see if user is assigning at declaration time.
        /// </summary>
        /// <param name="currentIter">The current location in Input.</param>
        /// <returns>True if user is assigning at declaration, False otherwise.</returns>
        public bool IsAssigning(ref int currentIter)
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
