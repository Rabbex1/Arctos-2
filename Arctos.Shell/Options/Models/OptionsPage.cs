using System.ComponentModel;
using System.Xml.Serialization;
using Arctos.Shell.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Shell.Options
{
    public abstract class OptionsPage
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        protected static Dictionary<Type, XmlSerializer> Serializers = new();
        private readonly Guid _guid = Guid.NewGuid();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Dictionary<string, OptionPageProperty> Properties { get; } = new();
        public OptionsPageKey Key { get; set; }
        public Type? ContentType { get; protected set; }
        public OptionsPage? TempCopy { get; private set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        protected OptionsPage(string categoryId, string pageId, Guid sourceId)
        {
            Key = new OptionsPageKey(categoryId, pageId, sourceId);
        }

        protected OptionsPage(OptionsPageKey key)
        {
            Argument.IsNotNull("key", key);

            Key = key;
        }

        protected OptionsPage()
        {
            Key = new OptionsPageKey();
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public OptionPageProperty this[string propName]
        {
            get => Properties[propName];
            set
            {
                if (Properties.ContainsKey(propName))
                    Properties[propName] = value;
                else
                    Properties.Add(propName, value);
            }

            //get => GetPropertyValue(propName);
            //set => SetPropertyValue(propName, value);
        }
        public void ClearChanges()
        {
            foreach (var kvp in Properties) 
                kvp.Value.Changed = false;
        }

        public abstract object GetModel();

        public void Update(OptionsPage source)
        {
            if (!Equals(Key, source.Key))
                throw new InvalidOperationException($"Cannot update options page with key '{Key}'. The source has a different key '{source.Key}'.");
            if (ContentType != source.ContentType)
                throw new InvalidOperationException($"Cannot update options page. The source and destination keys match '{source.Key}', but the content types are different.");
            if (Equals(source))
                return;

            foreach (var key in source.Properties.Keys.ToList())
            {
                this[key].Value = source[key].Value.Copy();
            }
        }

        public OptionsPage? Clone()
        {
            //Hack: This is inefficient because it serializes then deserializes the options page via a XML element to clone it. 
            //Hack: But it means objects don't need the [Serializable] attribute.
            return UserOptionsDoc.FromXElement(UserOptionsDoc.ToXElement(this));
            //return this.Copy();
        }

        public void MakeTempCopy()
        {
            if (TempCopy != null)
                return;
            TempCopy = Clone();
            if (TempCopy?.Properties != null)
                foreach (var property in TempCopy.Properties)
                {
                    property.Value.Changed = false;
                }
        }

        public void UpdateFromTempCopy()
        {
            bool propChanged = false;
            var newProperties = new List<OptionPageProperty>();
            var oldProperties = new List<OptionPageProperty>();

            if (TempCopy == null)
                return;
            foreach (var property in TempCopy.Properties)
            {
                if (property.Value.Changed)
                {
                    newProperties.Add(property.Value);
                    oldProperties.Add(Properties[property.Key]);
                    propChanged = true;
                }
            }
            if(propChanged)
            {
                Update(TempCopy);
                RaisePageChanged();
                WeakReferenceMessenger.Default.Send<OptionsPageMessage>(new OptionsPageMessage(this, newProperties, oldProperties));
            }
        }

        public void DeleteTempCopy()
        {
            TempCopy = null;
        }

        public object? GetPropertyValue(string propName)
        {
            //return Properties.ContainsKey(propName) ? Properties[propName]: null;
            return Properties.ContainsKey(propName) ? Properties[propName].Value : null;
        }

        public void SetPropertyValue(string propertyName, object? value)
        {
            if (!Properties.ContainsKey(propertyName))
                return;
            if (Equals(Properties[propertyName], value))
                return;


            var valueType = value?.GetType();
            var oldProperty = Properties[propertyName];
            var newProperty = new OptionPageProperty(oldProperty);

            try
            {
                if (valueType == null)
                    newProperty.Value = null;
                else if (newProperty.Type == valueType)
                    newProperty.Value = value;
                else if (valueType.IsSubclassOf(newProperty.Type))
                    newProperty.Value = value;
                else
                {
                    var converter = TypeDescriptor.GetConverter(newProperty.Type);
                    newProperty.Value = converter.CanConvertFrom(valueType) && value != null
                        ? converter.ConvertFrom(value)
                        : Activator.CreateInstance(newProperty.Type);
                }

                Properties[propertyName] = newProperty;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Cannot set property'{propertyName}' with value of type '{valueType}'.", ex);
            }

            if(newProperty.Changed)
            {
                RaisePageChanged();
                WeakReferenceMessenger.Default.Send(new OptionsPageMessage(this,
                    new List<OptionPageProperty> {newProperty}, new List<OptionPageProperty> {oldProperty}));
            }
        }

        protected bool Equals(OptionsPage other)
        {
            return Equals(Properties, other.Properties) &&
                   Equals(Key, other.Key) &&
                   ContentType == other.ContentType;
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        //#region Event Handlers

        //public void OnPageChanged(object? sender, PropertyChangedEventArgs e)
        //{
        //    //if (!(sender is OptionsPage page))
        //    //    return;

        //    //if (!_changedPages.Contains(page))
        //    //    _changedPages.Add(page);
        //}

        //#endregion Constructors


        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Events

        #region Property Changed
        public event EventHandler? PageChanged;
        protected virtual void RaisePageChanged()
        {
            PageChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #endregion Events


    }
}
