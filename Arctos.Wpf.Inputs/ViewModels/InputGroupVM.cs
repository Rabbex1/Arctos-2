using System.Collections.ObjectModel;
using System.Reflection;

namespace Arctos.Wpf.Inputs
{
    public class InputGroupVM : DataInputWrapperVM<(string header, List<DataInputWrapperVM> wrappers)>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Action? _extraAction;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Header
        private string _header = string.Empty;
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }
        #endregion

        public InputFormVM PropertiesForm { get; set; } = new();

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructors
        public InputGroupVM(string description, (string header, List<DataInputWrapperVM> wrappers) value, Action? extraAction)
            : base(description, value, null)
        {
            Header = value.header;
            _extraAction = extraAction;

            PropertiesForm.Inputs = new ObservableCollection<DataInputWrapperVM>(value.wrappers);

            //GetProperties();
        }

        #endregion Constructors


        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void GetProperties()
        {
            SortedList<int, PropertyInfo> publicProperties = new();

            var propInfos = Value.GetType().GetProperties();
            foreach (var propInfo in propInfos)
            {
                var attr = (ArctosInputAttribute[])propInfo.GetCustomAttributes(typeof(ArctosInputAttribute), false);

                if (propInfo.SetMethod != null && propInfo.GetMethod != null && attr.Length != 0)
                    publicProperties.Add(attr[0].Position, propInfo);
            }

            foreach (var property in publicProperties.Values)
            {
                var wrapper = CreateTypeWrapper(property.Name.SplitCamelCase(), property.GetValue(Value), o =>
                {
                    property.SetValue(Value, o);
                    _extraAction.Invoke();
                });
                if (wrapper == null)
                    continue;
                wrapper.Description = property.Name.SplitCamelCase() + ":";
                if (wrapper.Description == "Name:")
                    wrapper.IsReadOnly = true;
                PropertiesForm.Inputs.Add(wrapper);
            }

        }

    }

    #endregion Methods

}
