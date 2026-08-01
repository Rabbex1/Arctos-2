using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Arctos.Forms;

public interface IInput
{
}

public class Input : ObservableObject, IInput
{
}

public sealed class IntRangeInput : Input
{
    private int _min;
    private int _max;
    private int _value;

    public int Min
    {
        get => _min;
        set
        {
            if (!SetProperty(ref _min, value))
                return;

            if (_max < _min)
                _max = _min;

            if (_value < _min)
                _value = _min;

            OnPropertyChanged(nameof(Max));
            OnPropertyChanged(nameof(Value));
            Wrapper?.OnChange?.Invoke(this);
        }
    }

    public int Max
    {
        get => _max;
        set
        {
            var max = Math.Max(value, Min);
            if (!SetProperty(ref _max, max))
                return;

            if (_value > _max)
            {
                _value = _max;
                OnPropertyChanged(nameof(Value));
            }

            Wrapper?.OnChange?.Invoke(this);
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, Min, Max);
            if (!SetProperty(ref _value, clamped))
                return;

            Wrapper?.OnChange?.Invoke(this);
        }
    }

    [XmlIgnore]
    public InputIntRangeVM? Wrapper { get; set; }
}

public sealed class FloatRangeInput : Input
{
    private float _min;
    private float _max;
    private float _value;

    public float Min
    {
        get => _min;
        set => SetProperty(ref _min, value);
    }

    public float Max
    {
        get => _max;
        set => SetProperty(ref _max, value);
    }

    public float Value
    {
        get => _value;
        set
        {
            if (!SetProperty(ref _value, value))
                return;

            Wrapper?.OnChange?.Invoke(this);
        }
    }

    [XmlIgnore]
    public InputFloatRangeVM? Wrapper { get; set; }
}

public sealed class StringSelection : ObservableObject
{
    private string _value = string.Empty;

    public List<string> Options { get; set; } = [];

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public List<string> List { get; set; } = [];

    [XmlIgnore]
    [JsonIgnore]
    public Action<string> Action { get; set; } = _ => { };

    [JsonIgnore]
    public int Index => Options.IndexOf(Value);

    public StringSelection Copy()
    {
        return new StringSelection
        {
            Options = [.. Options],
            Value = Value,
            Action = Action
        };
    }

    public override string ToString()
    {
        return Value;
    }
}
