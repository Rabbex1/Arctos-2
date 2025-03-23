using System.Xml.Linq;
using System.Xml.Serialization;
using Arctos.Configuration;

namespace Arctos.Shell.Options
{

    public class UserOptionsDoc : ConfigDocument
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public UserOptionsDoc(string fileName, bool eraseExisting = false)
          : base(fileName, "UserOptions", eraseExisting)
        {
        }

        public UserOptionsDoc(string fileName, string rootElementName, bool eraseExisting = false)
          : base(fileName, rootElementName, eraseExisting)
        {
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public bool AddOrReplacePage(OptionsPage page)
        {
            var oldElement = GetPageElement(page.Key);
            var newElement = ToXElement(page);

            if (oldElement == null)
            {
                Document.Root?.Add(newElement);
                return true;
            }
            oldElement.ReplaceWith(newElement);
            return false;
        }

        public void RemovePage(OptionsPageKey? key)
        {
            var element = GetPageElement(key);
            element?.Remove();
        }

        public OptionsPage? GetPage(OptionsPageKey? key)
        {
            var element = GetPageElement(key);
            return element == null ? null : FromXElement(element);
        }

        public List<OptionsPage?>? GetPagesByAttribute(string? name, string? value)
        {
            var elements = GetPageElementsByAttribute(name, value);
            return elements?.Select(FromXElement).ToList();
        }

        public static OptionsPage? ClonePage(OptionsPage source)
        {
            return FromXElement(ToXElement(source));
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Static Methods

        public static XElement? ToXElement(OptionsPage page)
        {
            var doc = new XDocument();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var xmlSerializer = new XmlSerializer(page.GetType());
            using (var writer = doc.CreateWriter())
            {
                xmlSerializer.Serialize(writer, page, ns);
            }
            return doc.Root;
        }

        public static OptionsPage? FromXElement(XElement? element)
        {
            if (element == null) 
                return null;
            var modelType = OptionsService.GetPageContentType(element.Attribute("contentType")?.Value);
            if (modelType == null)
                return null;

            var pageType = typeof(OptionsPage<>);
            var genericType = pageType.MakeGenericType(modelType);
            var serializer = new XmlSerializer(genericType);

            using var reader = element.CreateReader();
            return (OptionsPage?)serializer.Deserialize(reader);
        }

        #endregion Static Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private XElement? GetPageElement(OptionsPageKey? key)
        {
            if (key == null)
                return null;
            //do inside try in case Guid. Parse throws an exception
            try
            {
                var e = Document.Root?.Elements();
                return Document.Root?.Elements()
                  .FirstOrDefault(e => e.Name.LocalName.Contains("OptionsPage") &&
                                       e.Attributes().Any(a => a.Name == "categoryId" && a.Value == key.CategoryId) &&
                                       e.Attributes().Any(a => a.Name == "pageId" && a.Value == key.PageId) &&
                                       e.Attributes().Any(a => a.Name == "ownerId" && Guid.Parse(a.Value) == key.OwnerId));
            }
            catch
            {
                return null;
            }
        }

        IEnumerable<XElement>? GetPageElementsByAttribute(string? name, string? value)
        {
            if (name == null || value == null)
                return null;
            return Document.Root?.Elements().Where(e => e.Name.LocalName == "OptionsPage" &&
                                                   e.Attributes().Any(a => a.Name == name && a.Value == value));
        }

        #endregion Private Methods

        #endregion Methods

    }
}
