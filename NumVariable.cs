
public struct NumVariable
{
    public string Name { get; set; }
    public string Value { get; set; }
    public bool IsAssigned { get; set; }

    public NumVariable(string name)
    {
        Name = name;
        Value = "";
        IsAssigned = false;
    }
}