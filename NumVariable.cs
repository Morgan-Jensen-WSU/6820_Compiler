
public struct NumVariable
{
    public string Name { get; set; }
    public int Value { get; set; }
    public bool IsAssigned { get; set; }

    public NumVariable(string name)
    {
        Name = name;
        Value = 0;
        IsAssigned = false;
    }
}