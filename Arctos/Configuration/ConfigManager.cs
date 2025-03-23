using System.Collections.Generic;

namespace Arctos.Configuration
{
  public class ConfigManager
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly Dictionary<string, ConfigDocument> _documents = new Dictionary<string, ConfigDocument>();

    #endregion Fields

    //---------------------------------------------------------------------
    #region Public Methods

    public ConfigDocument GetDocument(string fileName)
    {
      if (_documents.ContainsKey(fileName))
        return _documents[fileName];

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
