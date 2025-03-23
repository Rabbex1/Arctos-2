namespace Arctos.Configuration
{
  public class CategoryDocument : ConfigDocument
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    public CategoryDocument(string fileName, bool eraseExisting = false) : base(fileName, eraseExisting)
    {
    }

    public CategoryDocument(string fileName, string rootElementName, bool eraseExisting = false)
      : base(fileName, rootElementName, eraseExisting)
    {
    }

    #endregion Constructors

    public bool AddCategory(string categoryId)
    {
      GetOrAddAttributedElement(Document.Root, "Category", "id", categoryId, out var added);
      return added;
    }

    public bool RemoveCategory(string categoryId)
    {
      return RemoveAttributedElement(Document.Root, "Category", "id", categoryId);
    }

    public List<string?>? GetCategories()
    {
      return Document.Root?.Elements()
        .Where(e => e.Name.LocalName == "Category")
        .Select(c =>
        {
          var attr = c.Attributes().FirstOrDefault(a => a.Name == "id");
          return attr?.Value;
        }
        ).ToList();
    }

    public bool AddSubCategory(string categoryId, string subCategoryId)
    {
      var category = GetOrAddAttributedElement(Document.Root, "Category", "id", categoryId, out bool added);
      GetOrAddAttributedElement(category, "SubCategory", "id", subCategoryId, out added);
      return added;
    }

    public bool RemoveSubCategory(string categoryId, string subCategoryId)
    {
      var category = FindAttributedElement(Document.Root, "Category", "id", categoryId);
      return category != null && RemoveAttributedElement(category, "SubCategory", "id", subCategoryId);
    }

    public List<string?>? GetSubCategories(string categoryId)
    {
      return FindAttributedElement(Document.Root, "Category", "id", categoryId)?.Elements()
        .Where(e => e.Name.LocalName == "SubCategory")
        .Select(c =>
        {
          var attr = c.Attributes().FirstOrDefault(a => a.Name == "id");
          return attr?.Value;
        }
        ).ToList();
    }

    public bool SetCategoryValue(object value, string categoryId, string id)
    {
      var category = GetOrAddAttributedElement(Document.Root, "Category", "id", categoryId, out _);
      return SetValue(category, value, id);
    }

    public bool SetSubCategoryValue(object value, string categoryId, string subCategoryId, string id)
    {
      var category = GetOrAddAttributedElement(Document.Root, "Category", "id", categoryId, out _);
      var subCategory = GetOrAddAttributedElement(category, "SubCategory", "id", subCategoryId, out _);
      return SetValue(subCategory, value, id);
    }

    public T? GetCategoryValue<T>(string categoryId, string id)
    {
      var category = GetOrAddAttributedElement(Document.Root, "Category", "id", categoryId, out _);
      return GetValue<T>(category, id);
    }

    public T? GetSubCategoryValue<T>(string categoryId, string subCategoryId, string id)
    {
      var category = GetOrAddAttributedElement(Document.Root, "Category", "id", categoryId, out _);
      var subCategory = GetOrAddAttributedElement(category, "SubCategory", "id", subCategoryId, out _);
      return GetValue<T>(subCategory, id);
    }


  }
}
