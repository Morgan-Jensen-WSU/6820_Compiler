using System.IO;

namespace compiler
{
    public static class Error
    {
        /// <summary>
        /// Writes error message to /ProgramName/.err
        /// Terminates the program
        /// </summary>
        /// <param name="errorMessage">Message to be displayed to user.</param> 
        public static void ThrowError(string errorMessage)
        {
            using (StreamWriter writer = new StreamWriter($"output/{Program.ProgramName}.err"))
            {
                writer.WriteLine($"Error: {errorMessage}");
            }
            System.Environment.Exit(1);
        }
    }
}
