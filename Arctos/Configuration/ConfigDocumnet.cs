using System.Collections;
using System.Xml.Linq;
using System.Xml.Serialization;
using Arctos.Utils;

namespace Arctos.Configuration
{
    public class ConfigDocument
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        protected XDocument Document = null!;
        protected string? RootElementName;
        //[XmlArray]
        //private IEnumerable<object> _serializableList = new List<object>();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public string FileName { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public ConfigDocument(string fileName, bool eraseExisting = false)
        {
            Argument.IsNotNullOrWhiteSpace("fileName", fileName);

            if (eraseExisting && File.Exists(fileName))
                File.Delete(fileName);

            FileName = fileName;
            Open();
        }

        public ConfigDocument(string fileName, string rootElementName, bool eraseExisting = false)
          : this(fileName, eraseExisting)
        {
            Argument.IsNotNullOrWhiteSpace("rootElementName", rootElementName);

            RootElementName = rootElementName;
            if (Document.Root == null)
                Document.Add(new XElement(RootElementName));
        }


        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void ClearDocument()
        {
            Document.RemoveNodes();
            if (!string.IsNullOrWhiteSpace(RootElementName))
                Document.Add(new XElement(RootElementName));
        }

        public bool SetRootValue(object value, string id)
        {
            return SetValue(Document.Root, value, id);
        }

        public bool SetAsRoot(object value, string id)
        {
            return SetValue(Document, value, id);
        }

        public T? GetRootValue<T>(string id)
        {
            return Document.Root != null 
                ? GetValue<T>(Document.Root, id)
                : default;
        }

        public T? GetRoot<T>(string id)
        {
            return GetValue<T>(Document, id);
        }

        public void Save()
        {
            Document.Save(FileName);
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region protected Methods

        protected void Open()
        {
            Document = File.Exists(FileName) ? XDocument.Load(FileName) : new XDocument();
        }

        protected static XElement? FindAttributedElement(XContainer? container, string elementName, string attributeName, string atrributeValue)
        {
            Argument.IsNotNull("container", container);
            Argument.IsNotNullOrWhiteSpace("elementName", elementName);
            Argument.IsNotNullOrWhiteSpace("attributeName", attributeName);
            Argument.IsNotNullOrWhiteSpace("attributeValue", atrributeValue);

            return container?.Elements().FirstOrDefault(e => e.Name.LocalName == elementName &&
                                                        e.Attributes().Any(a => a.Name == attributeName && a.Value == atrributeValue));
        }

        protected static XElement GetOrAddAttributedElement(XContainer? container, string elementName, string attributeName, string attributeValue, out bool added)
        {
            Argument.IsNotNull("container", container);
            Argument.IsNotNullOrWhiteSpace("elementName", elementName);
            Argument.IsNotNullOrWhiteSpace("attributeName", attributeName);
            Argument.IsNotNullOrWhiteSpace("attributeValue", attributeValue);

            var element = FindAttributedElement(container, elementName, attributeName, attributeValue);
            if (element != null)
            {
                added = false;
                return element;
            }
            var newElement = new XElement(elementName, new XAttribute(attributeName, attributeValue));
            container?.Add(newElement);
            added = true;
            return newElement;
        }

        protected static bool RemoveAttributedElement(XContainer? container, string elementName, string attributeName, string attributeValue)
        {
            var element = FindAttributedElement(container, elementName, attributeName, attributeValue);

            if (element == null)
                return false;
            element.Remove();
            return true;
        }

        protected static bool UseIdAsElementName(Type? type)
        {
            return type == null || type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
        }

        protected bool SetValue(XContainer? container, object value, string id)
        {

            if (container == null)
                throw new ArgumentNullException($"Argument 'container' cannot be null");
            if (id == null)
                throw new ArgumentNullException($"Argument 'id' cannot be null");

            var valueType = value.GetType();
            XElement? newElement;
            XElement? oldElement;

            if (UseIdAsElementName(valueType))
            {
                Argument.IsNotNullOrWhiteSpace("id", id);

                newElement = new XElement(id) { Value = value.ToString() ?? "" };
                oldElement = container.Elements().FirstOrDefault(e => e.Name.LocalName == id);
            }
            else
            {
                newElement = ToXElement(id, value);
                oldElement = FindElement(container, id, valueType);
            }

            if (oldElement == null)
            {
                if (container is XDocument && Document.Root != null)
                    Document = new XDocument(newElement);
                else
                    container.Add(newElement);
                return true;
            }
            oldElement.ReplaceWith(newElement);
            return false;
        }

        protected T? GetValue<T>(XContainer container, string id)
        {
            if (container == null)
                throw new ArgumentNullException($"Argument 'container' cannot be null");


            XElement? xElement;
            var valueType = typeof(T);

            if (UseIdAsElementName(valueType))
            {
                Argument.IsNotNullOrWhiteSpace("id", id);

                xElement = container.Elements().FirstOrDefault(e => e.Name.LocalName == id);
                if (xElement == null)
                    return default(T);
                return (T)Convert.ChangeType(xElement.Value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            xElement = container is XDocument
              ? Document.Root
              : FindElement(container, id, typeof(T));
            return xElement == null ? default(T) : FromXElement<T>(xElement);
        }

        protected static XElement? FindElement(XContainer container, string id, Type type)
        {
            string typeName;
            if (type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                typeName = "ArrayOf" + type.GetGenericArguments()[0].Name;
            else
                typeName = type.Name;
            return string.IsNullOrEmpty(id)
              ? container.Elements().FirstOrDefault(e => e.Name.LocalName == typeName)
              : container.Elements().FirstOrDefault(e => e.Name.LocalName == typeName && e.Attributes()
                                    .Any(a => a.Name == "id" && a.Value == id));
        }

        //TODO: Maybe rewrite this without using XDocument
        protected XElement? ToXElement(string name, object obj)
        {
            object? serializableObj;

            if ( obj is IEnumerable e && e.GetType().IsGenericType)
            {
                //TODO: may need to use external variable with[XmlArray]
                var serializableList = e as IEnumerable<object>;
                serializableObj = serializableList;
            }
            else
                serializableObj = obj;

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var doc = new XDocument();
            if (serializableObj != null)
            {
                var xmlSerializer = new XmlSerializer(serializableObj.GetType());
                using var writer = doc.CreateWriter();
                xmlSerializer.Serialize(writer, serializableObj, ns);
            }
            doc.Root?.Add(new XAttribute("id", name));
            return doc.Root;
        }

        protected static T? FromXElement<T>(XElement element)
        {
            var serializer = new XmlSerializer(typeof(T));

            using var reader = element.CreateReader();
            var obj = serializer.Deserialize(reader);
            return obj != null
                ? (T) obj
                : default;
        }


        #endregion protected Methods

        #endregion Methods
    }
}
