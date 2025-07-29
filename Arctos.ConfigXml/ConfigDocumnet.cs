using System.Collections;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Arctos.ConfigXml
{
    public class ConfigDocument
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        protected XDocument Document;
        protected string RootElementName;

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

        public bool SetRootValue(object value, string id = null)
        {
            BeforeSerialized(value);
            return SetValue(Document.Root, value, id);
        }

        public bool SetAsRoot(object value, string id = null)
        {
            BeforeSerialized(value);
            return SetValue(Document, value, id);
        }

        public T GetRootValue<T>(string id = null)
        {
            var root = GetValue<T>(Document.Root, id);
            AfterDeserialized(root);
            return root;
        }

        public T GetRoot<T>(string id = null)
        {
            var root = GetValue<T>(Document, id);
            AfterDeserialized(root);
            return root;
        }

        public void Save()
        {
            Document?.Save(FileName);
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region protected Methods

        protected void BeforeSerialized(object rootObject)
        {
            if (rootObject == null)
                return;
            var onDeserializedMethod = rootObject.GetType()
                .GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(XmlBeforeSerialized), false).Length > 0)
                .ToArray()
                .FirstOrDefault();
            if (onDeserializedMethod != null)
                onDeserializedMethod.Invoke(rootObject, null);
        }

        protected void AfterDeserialized(object rootObject)
        {
            if (rootObject == null)
                return;
            var onDeserializedMethod = rootObject.GetType()
                                           .GetMethods()
                                           .Where(m => m.GetCustomAttributes(typeof(XmlAfterDeserialized), false).Length > 0)
                                           .ToArray()
                                           .FirstOrDefault();
            if (onDeserializedMethod != null)
                onDeserializedMethod.Invoke(rootObject, null);
        }

        protected void Open()
        {
            Document = File.Exists(FileName) ? XDocument.Load(FileName) : new XDocument();
        }

        protected static XElement FindAttributedElement(XContainer container, string elementName, string attributeName, string attributeValue)
        {
            Argument.IsNotNull("container", container);
            Argument.IsNotNullOrWhiteSpace("elementName", elementName);
            Argument.IsNotNullOrWhiteSpace("attributeName", attributeName);
            Argument.IsNotNullOrWhiteSpace("attributeValue", attributeValue);

            return container.Elements().FirstOrDefault(e => e.Name.LocalName == elementName &&
                                                            e.Attributes().Any(a => a.Name == attributeName && a.Value == attributeValue));
        }

        protected static XElement GetOrAddAttributedElement(XContainer container, string elementName, string attributeName, string attributeValue, out bool added)
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
            container.Add(newElement);
            added = true;
            return newElement;
        }

        protected static bool RemoveAttributedElement(XContainer container, string elementName, string attributeName, string attributeValue)
        {
            var element = FindAttributedElement(container, elementName, attributeName, attributeValue);

            if (element == null)
                return false;
            element.Remove();
            return true;
        }

        protected static bool UseIdAsElementName(Type type)
        {
            return type == null || type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
        }

        protected bool SetValue(XContainer container, object value, string id)
        {
            Argument.IsNotNull("container", container);

            var valueType = value?.GetType();
            XElement newElement;
            XElement oldElement;

            if (UseIdAsElementName(valueType))
            {
                Argument.IsNotNullOrWhiteSpace("id", id);

                newElement = new XElement(id) { Value = value?.ToString() ?? "" };
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

        protected T GetValue<T>(XContainer container, string id = null)
        {
            Argument.IsNotNull("container", container);

            XElement xElement;
            var valueType = typeof(T);

            if (UseIdAsElementName(valueType))
            {
                Argument.IsNotNullOrWhiteSpace(nameof(id), id);

                xElement = container.Elements().FirstOrDefault(e => e.Name.LocalName == id);
                if (xElement == null)
                    return default;
                return (T)Convert.ChangeType(xElement.Value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            xElement = container is XDocument
              ? Document.Root
              : FindElement(container, id, typeof(T));
            return xElement == null ? default : FromXElement<T>(xElement);
        }

        protected static XElement FindElement(XContainer container, string id, Type type)
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
        protected XElement ToXElement(string name, object obj)
        {
            object serializableObj;

            if (obj.GetType().IsGenericType && obj is IEnumerable)
            {
                //TODO: may need to use external variable with[XmlArray]
                var serializableList = obj as IEnumerable<object>;
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
            if (name != null)
            {
                doc.Root?.Add(new XAttribute("id", name));
            }
            return doc.Root;
        }

        protected static T FromXElement<T>(XElement element)
        {
            var serializer = new XmlSerializer(typeof(T));

            using var reader = element.CreateReader();
            return (T)serializer.Deserialize(reader);
        }


        #endregion protected Methods

        #endregion Methods
    }
}
