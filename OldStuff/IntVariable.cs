
public class IntVariable : IVariable<int>
{
    public string Name { get; set; }
    public int Value { get; set; }
    public bool IsSet { get; set; }
    object IVariable.Value { get; set; }

    public IntVariable(string name)
    {
        Name = name;
        IsSet = false;
    }
}