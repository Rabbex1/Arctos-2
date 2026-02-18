using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Arctos.Wpf.Inputs
{
    public class StringSelection : ObservableObject
    {
        public List<string> Options { get; set; } = [];

        #region Value
        private string _value = string.Empty;

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
        #endregion


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
            return _value;
        }

    }
}
