namespace Arctos.ConfigXml
{
  public class ConfigManager
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly Dictionary<string, ConfigDocument> _documents = new();

    #endregion Fields

    //---------------------------------------------------------------------
    #region Public Methods

    public ConfigDocument GetDocument(string fileName)
    {
      if (_documents.TryGetValue(fileName, out var document))
        return document;

      var newDocument = new ConfigDocument(fileName);
      _documents.Add(fileName, newDocument);
      return newDocument;
    }

    public void SaveAll()
    {
      foreach (var document in _documents.Values)
        document.Save();
    }

    #endregion Public Methods
  }

}
