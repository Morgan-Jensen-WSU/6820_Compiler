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
                Error.ThrowError("No parameters given.");
            }

            InputFile = args[0];

            MyParser = new Parser(InputFile);
            MyParser.Parse();
            CreateOutput();

            Writer = new StreamWriter(OutputFile);


            GetASMReady();
            PrintDataSection();
            PrintBssSection();
            StartTextSection();
            PrintTextSection();
            PrintTermination();


            Writer.Close();
            Writer.Dispose();
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
            Writer.WriteLine("stringPrinter\tdb\t\"%s\",0");
            Writer.WriteLine("numberPrinter\tdb\t\"%d\",0x0d,0x0a,0");
            Writer.WriteLine("int_format\tdb\t\"%i\", 0");
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
            Writer.WriteLine("section\t.bss");
            Writer.WriteLine();
            foreach (var line in SymbolTable.BssSection)
            {
                Writer.WriteLine(line);
            }
            Writer.WriteLine("\n");
        }

        private static void StartTextSection()
        {
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("; Code");
            Writer.WriteLine(";-----------------------------");
            Writer.WriteLine("section\t.text");
            Writer.WriteLine();
            Writer.WriteLine("printInt:");
            Writer.WriteLine("\tpush\trbp\t\t; Avoid stack alignment isses");
            Writer.WriteLine("\tpush\trax\t\t; save rax and rcx");
            Writer.WriteLine("\tpush\trcx");
            Writer.WriteLine();
            Writer.WriteLine("\tmov\trdi, numberPrinter\t\t; set printf format parameter");
            Writer.WriteLine("\tmov\trsi, rax\t\t; set printf value parameter");
            Writer.WriteLine("\txor\trax, rax\t\t; set rax to 0 (number of float/vector regs used is 0)");
            Writer.WriteLine();
            Writer.WriteLine("\tcall\t[rel printf wrt ..got]");
            Writer.WriteLine("\tpop\trcx\t\t; restore rcx");
            Writer.WriteLine("\tpop\trax\t\t; restore rax");
            Writer.WriteLine("\tpop\trbp\t\t; avoid stack alignment issues");
            Writer.WriteLine("\tret");
            Writer.WriteLine();
            Writer.WriteLine("printString:");
            Writer.WriteLine("\tpush\trbp\t\t; Avoid stack alignment isses");
            Writer.WriteLine("\tpush\trax\t\t; save rax and rcx");
            Writer.WriteLine("\tpush\trcx");
            Writer.WriteLine();
            Writer.WriteLine("\tmov\trdi, stringPrinter\t\t; set printf format parameter");
            Writer.WriteLine("\tmov\trsi, rax\t\t; set printf value parameter");
            Writer.WriteLine("\txor\trax, rax\t\t; set rax to 0 (number of float/vector regs used is 0)");
            Writer.WriteLine();
            Writer.WriteLine("\tcall\t[rel printf wrt ..got]");
            Writer.WriteLine("\tpop\trcx\t\t; restore rcx");
            Writer.WriteLine("\tpop\trax\t\t; restore rax");
            Writer.WriteLine("\tpop\trbp\t\t; avoid stack alignment issues");
            Writer.WriteLine("\tret");
            Writer.WriteLine();
            Writer.WriteLine("main:");
        }

        private static void PrintTextSection()
        {
            foreach (var line in SymbolTable.TextSection)
            {
                Writer.WriteLine(line);
            }
        }

        private static void PrintTermination()
        {
            Writer.WriteLine("exit:");
            Writer.WriteLine("\tmov\trax, 60");
            Writer.WriteLine("\txor\trdi, rdi");
            Writer.WriteLine("\tsyscall");
        }
    }
}
