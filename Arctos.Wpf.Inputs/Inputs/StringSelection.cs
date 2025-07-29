using System.Xml.Serialization;

namespace Arctos.Wpf.Inputs
{
    public class StringSelection
    {
        public string? Value { get; set; }
        public List<string>? List { get; set; }
        [XmlIgnore] public Action<string>? Action { get; set; }
    }
}
