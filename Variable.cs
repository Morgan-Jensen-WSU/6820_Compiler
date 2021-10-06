
namespace compiler
{
    public class Variable<T>
    {
        public T value { get; set; }
        public bool IsAssigned { get; set; }

        public Variable() { }
    }
}
