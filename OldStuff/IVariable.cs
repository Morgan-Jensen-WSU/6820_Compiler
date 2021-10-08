
interface IVariable
{
    public string Name { get; set; }
    public bool IsSet { get; set; }
    public object Value { get; set; }
}

interface IVariable<T> : IVariable
{
    new public T Value { get; set; }
}