using UnityEngine;

public class EnumFlagsAttribute : PropertyAttribute
{
    public string name;

    public EnumFlagsAttribute() { }

    public EnumFlagsAttribute(string name)
    {
        this.name = name;
    }
}