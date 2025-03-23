using System.Collections;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Arctos.Shell.Options
{
    public class OptionsPage<TModel> : OptionsPage, IXmlSerializable
      where TModel : new()
    {

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public OptionsPage(string categoryId, string pageId, Guid sourceId)
          : base(categoryId, pageId, sourceId)
        {
            InitializeProperties();
        }

        public OptionsPage(OptionsPageKey key)
          : base(key)
        {
            InitializeProperties();
        }

        public OptionsPage()
        {
            InitializeProperties();
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void MapFrom(TModel? source)
        {
            if (source == null)
                return;
            foreach (var name in Properties.Keys.ToList())
                Properties[name].Value = ContentType?.GetProperty(name)?.GetValue(source).Copy();
            
        }

        public void MapTo(TModel dest)
        {
            foreach (var name in Properties.Keys.ToList())
                ContentType?.GetProperty(name)?.SetValue(dest, Properties[name].Value.Copy());
        }

        public void MapChanges(TModel dest, bool clearChanges = true)
        {
            foreach (var name in Properties.Keys.ToList())
            {
                var property = ContentType?.GetProperty(name);
                if (property == null || !Properties[name].Changed)
                    continue;
                property.SetValue(dest, Properties[name].Value.Copy());
                if (clearChanges)
                    Properties[name].Changed = false;
            }
        }

        public override object GetModel()
        {
            var dest = new TModel();
            foreach (var name in Properties.Keys.ToList())
                ContentType?.GetProperty(name)?.SetValue(dest, Properties[name].Value);
            return dest;
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region IXmlSerializable Methods

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool isEmpty = reader.IsEmptyElement;

            if (!reader.MoveToAttribute("categoryId"))
                throw new InvalidOperationException("OptionsPage must contain attribute 'categoryId'.");
            Key.CategoryId = reader.ReadContentAsString();
            if (!reader.MoveToAttribute("pageId"))
                throw new InvalidOperationException("OptionsPage must contain attribute 'nameId'.");
            Key.PageId = reader.ReadContentAsString();
            if (!reader.MoveToAttribute("contentType"))
                throw new InvalidOperationException("OptionsPage must contain attribute 'dataType'.");
            ContentType = OptionsService.GetPageContentType(reader.ReadContentAsString());
            if (!reader.MoveToAttribute("ownerId"))
                throw new InvalidOperationException("OptionsPage must contain attribute 'ownerId'.");
            Key.OwnerId = new Guid(reader.ReadContentAsString());

            if (string.IsNullOrWhiteSpace(Key.CategoryId) || string.IsNullOrWhiteSpace(Key.PageId) || ContentType == null)
                throw new InvalidOperationException("Invalid attributes on OptionPage XML element");
            if (isEmpty)
                return;


            reader.ReadStartElement();
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    continue;
                }
                if (reader.Name != "Option")
                    throw new InvalidOperationException("All first level child elements in 'OptionsPage' must be 'Option'.");
                if (!reader.MoveToAttribute("name"))
                    throw new InvalidOperationException("Option must contain attribute 'name'.");
                var propertyName = reader.ReadContentAsString();
                var propertyType = Properties[propertyName].Type;

                if (reader.MoveToAttribute("derivedType"))
                {
                    var derivedType = OptionsService.GetPageContentType(reader.ReadContentAsString());
                    if (derivedType != null && derivedType.IsSubclassOf(propertyType))
                        propertyType = derivedType;
                }

                reader.MoveToElement();

                if (propertyType.IsPrimitive || propertyType == typeof(string) || propertyType == typeof(decimal))
                {
                    Properties[propertyName].Value = Convert.ChangeType(reader.ReadElementContentAsString(), propertyType, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    reader.ReadStartElement();
                    try
                    {
                        if (!Serializers.TryGetValue(propertyType, out var propertySerializer))
                        {
                            propertySerializer = new XmlSerializer(propertyType);
                            Serializers.Add(propertyType, propertySerializer);
                        }
                        var value = propertySerializer.Deserialize(reader);
                        Properties[propertyName].Value = value;
                    }
                    catch (InvalidOperationException ex)
                    {
                        Debug.WriteLine($"Cannot Deserialize {propertyName}." + Environment.NewLine + ex.Message);
                    }

                    try
                    {
                        reader.ReadEndElement();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("categoryId", Key.CategoryId);
            writer.WriteAttributeString("pageId", Key.PageId);
            writer.WriteAttributeString("contentType", ContentType?.FullName);
            writer.WriteAttributeString("ownerId", "{" + Key.OwnerId + "}");
            foreach (var property in Properties)
            {
                var propertyName = property.Key;
                var propertyType = property.Value.Type;
                var propertyValue = property.Value.Value;
                writer.WriteStartElement("Option");
                writer.WriteAttributeString("name", propertyName);
                if (propertyValue != null)
                {
                    if (propertyType.IsPrimitive || propertyType == typeof(string) || propertyType == typeof(decimal))
                    {
                        writer.WriteString(propertyValue.ToString() ?? "");
                    }
                    else
                    {
                        var propertyValueType = propertyValue.GetType();
                        if (propertyType != propertyValueType && propertyValueType.IsSubclassOf(propertyType))
                        {
                            propertyType = propertyValueType;
                            writer.WriteAttributeString("derivedType", propertyType.FullName);
                        }

                        XmlRootAttribute? root; //Todo: used to get rid of 'ArrayOf' may have to rethink for deserialization
                        if (propertyType.IsGenericType && propertyValue is IEnumerable)
                        {
                            var serializableList = propertyValue as IEnumerable<object>;
                            propertyValue = serializableList;
                            root = new XmlRootAttribute(propertyName);
                        }
                        var ns = new XmlSerializerNamespaces();
                        ns.Add("", "");
                        if (propertyValue != null)
                        {
                            try
                            {
                                XmlSerializer valueSerializer;
                                if (Serializers.TryGetValue(propertyType, out var value))
                                    valueSerializer = value;
                                else
                                {
                                    valueSerializer = new XmlSerializer(propertyType);
                                    Serializers.Add(propertyType, valueSerializer);
                                }
                                valueSerializer.Serialize(writer, propertyValue, ns);
                            }
                            catch (InvalidOperationException ex)
                            {
                                Debug.WriteLine($"Cannot Serialize {propertyName}." + Environment.NewLine + ex.Message);
                            }
                        }
                    }
                }
                writer.WriteEndElement();
            }
        }

        #endregion IXmlSerializable Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private void InitializeProperties()
        {
            ContentType = typeof(TModel);
            IEnumerable<PropertyInfo> properties = ContentType.GetProperties();
            foreach (var property in properties)
            {
                if (!Attribute.IsDefined(property, typeof(UserOptionAttribute)))
                    continue;
                Properties.Add(property.Name, new OptionPageProperty(property.Name, property.PropertyType));
            }

            // generate an empty TModel and map it to this. This allows TModel to create default values
            var model = (TModel?)Activator.CreateInstance(typeof(TModel));
            MapFrom(model);
        }


        #endregion Private Methods

        #endregion Methods

    }
}
