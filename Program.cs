using System;
using System.IO;

namespace compiler
{
    class Program
    {
        public static String ProgramName { get; set; }
        private static Parser MyParser { get; set; }
        private static String InputFile { get; set; }
        private static String OutputFile { get; set; }

        private static StreamWriter Writer;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No params given");
                // throw error
                return;
            }

            InputFile = args[0]; 

            MyParser = new Parser(InputFile);
            MyParser.Parse();
            CreateOutput();

            Writer = new StreamWriter(OutputFile);

            
            GetASMReady();
            PrintDataSection();
            PrintBssSection();


            Writer.Close();
            Writer.Dispose();
            foreach (var variable in SymbolTable.Variables)
            {
                Console.WriteLine($"{variable.Name}: {variable.Value}");
            }
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

            var asmFile = File.Create($"output/{ProgramName}.asm");
            var errFile = File.Create($"output/{ProgramName}.err");

            asmFile.Close();
            errFile.Close();
            OutputFile = $"output/{ProgramName}.asm";
        }

        private static void GetASMReady()
        {
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("; exports");
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("GLOBAL main");
            Writer.WriteLine("\n");
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("; imports");
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("extern printf");
            Writer.WriteLine("extern scanf");
            Writer.WriteLine("extern exit");
            Writer.WriteLine("\n");            
        }

        private static void PrintDataSection()
        {
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("; initialized data");
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("section\t.data");
            Writer.WriteLine();
            foreach (var line in SymbolTable.DataSection)
            {
                Writer.WriteLine(line);
            }
            Writer.WriteLine("\n");
        }

        private static void PrintBssSection()
        {
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("; uninitialized data");
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("seciton\t.bss");
            Writer.WriteLine();
            foreach (var line in SymbolTable.BssSection)
            {
                Writer.WriteLine(line);
            }
            Writer.WriteLine("\n");
        }
    }
}
