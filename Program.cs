using System;
using System.IO;

namespace compiler
{
    class Program
    {

        private static Parser MyParser;

        private static String InputFile { get; set; }
        private static String ProgramName { get; set; }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No params given");
                // throw error
                return;
            }

            InputFile = args[0];
            ProgramName = "temp"; // remove before taking input
            
            MyParser = new Parser(InputFile);

            CreateOutput();

        }

        /// <summary>
        /// Removes existing .asm and .err files if they exist
        /// Creates new .asm and .err files
        /// </summary>
        private static void CreateOutput()
        {
            if (File.Exists($"output/{ProgramName}.asm"))
            {
                File.Delete($"output/{ProgramName}.asm");
            }
            if (File.Exists($"output/{ProgramName}.err"))
            {
                File.Delete($"output/{ProgramName}.err");
            }

            File.Create($"output/{ProgramName}.asm");
            File.Create($"output/{ProgramName}.err");
        }
    }
}
