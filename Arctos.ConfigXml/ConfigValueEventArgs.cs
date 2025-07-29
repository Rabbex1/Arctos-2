namespace Arctos.ConfigXml
{
  public class ConfigValueEventArgs : EventArgs
  {
    public ConfigValueAction Action { get; set; }
    public string Name { get; set; }
    public Type Type { get; set; }
    public object NewValue { get; set; }
    public object OldValue { get; set; }
  }
}
