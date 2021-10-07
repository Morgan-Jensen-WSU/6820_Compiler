
namespace compiler
{
    public class Variable<T> : IVariable
    {
        public string Name { get; set; }
        public T Value { get; set; }
        public bool IsAssigned { get; set; }

        public Variable(string name)
        {
            Name = name;
            IsAssigned = false;
        }
    }
}
