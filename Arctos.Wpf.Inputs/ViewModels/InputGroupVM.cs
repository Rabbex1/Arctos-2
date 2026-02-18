using System;
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

        #region Collapsable
        private bool _collapsable;
        public bool Collapsable
        {
            get => _collapsable;
            set => SetProperty(ref _collapsable, value);
        }
        #endregion

        public InputFormVM PropertiesForm { get; set; } = new();

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructors

        public InputGroupVM(string description, (string header, List<DataInputWrapperVM> wrappers) value, bool collapsable = false, Action? extraAction = null)
            : base(description, value, null)
        {
            Header = value.header;
            Collapsable = collapsable;
            _extraAction = extraAction;

            PropertiesForm.Inputs = new ObservableCollection<DataInputWrapperVM>(value.wrappers);

            //GetProperties();
        }

        public InputGroupVM(string description, (string header, List<DataInputWrapperVM> wrappers) value, Action? extraAction)
            : this(description, value, false, extraAction)
        {
        }

        #endregion Constructors


        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void GetProperties()
        {
            SortedList<int, PropertyInfo> publicProperties = [];

            var propInfos = Value.GetType().GetProperties();
            foreach (var propInfo in propInfos)
            {
                if (propInfo.SetMethod == null || propInfo.GetMethod == null)
                    continue;

                var arctosInputAttr = (ArctosInputAttribute[])propInfo.GetCustomAttributes(typeof(ArctosInputAttribute), false);
                if (arctosInputAttr.Length != 0)
                {
                    publicProperties.Add(arctosInputAttr[0].Position, propInfo);
                    continue;
                }

                var arctosSettingAttr = (ArctosSettingAttribute[])propInfo.GetCustomAttributes(typeof(ArctosSettingAttribute), false);
                if (arctosSettingAttr.Length != 0)
                    publicProperties.Add(arctosSettingAttr[0].Order, propInfo);
            }

            foreach (var property in publicProperties.Values)
            {
                var wrapper = CreateTypeWrapper(property.Name.SplitCamelCase(), property.GetValue(Value), o =>
                {
                    property.SetValue(Value, o);
                    _extraAction?.Invoke();
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
