using System;
using System.Collections.Generic;

namespace compiler
{
    public class Parser
    {
        private List<string> Words = new List<string>();

        public Parser(string inputFile)
        {
            // get text into Words
            string[] lines = System.IO.File.ReadAllLines(inputFile);

            foreach (var line in lines)
            {
                string[] words = line.Split(" ");
                foreach (var word in words)
                {
                    Words.Add(word);
                }
            }

        }

        private void FilterComments()
        {
            for (int i = 0; i < Words.Count; i++)
            {
                if (Words[i].Contains("//"))
                {
                    int iter = i;
                    while (Words[iter] != "\n" || Words[iter] == "\r") // this doesnt work
                    {
                        iter++;
                    }
                    Words.RemoveRange(i, iter - i);
                    i = iter;
                }

                if (Words[i].Contains("/*"))
                {
                    int iter = i;
                    while (Words[iter] != "*/")
                    {
                        iter++;
                    }
                    Words.RemoveRange(i, iter - i);
                    i = iter;
                }
            }
        }
    }
}
