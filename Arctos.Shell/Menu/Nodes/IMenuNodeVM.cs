namespace Arctos.Shell.Menu
{
  public interface IMenuNodeVM
  {
    string Id { get; set; }
    int Priority { get; set; }
    bool IsVisible { get; set; }
    bool IsEnabled { get; set; }
    string? DisplayName { get; set; }
  }
}
