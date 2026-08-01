namespace Arctos.Forms;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ArctosInputAttribute : Attribute
{
    private int _position = -1;

    public int Position
    {
        get => _position;
        set
        {
            if (value < 0)
                throw new ArgumentException("Cannot be negative", nameof(Position));

            _position = value;
        }
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ArctosSettingAttribute : Attribute
{
    private int _order = -1;

    public int Order
    {
        get => _order;
        set
        {
            if (value < 0)
                throw new ArgumentException("Cannot be negative", nameof(Order));

            _order = value;
        }
    }
}
