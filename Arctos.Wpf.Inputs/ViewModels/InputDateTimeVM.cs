using System.Globalization;

namespace Arctos.Wpf.Inputs
{
    public class InputDateTimeVM : DataInputWrapperVM<DateTime>
    {
        private readonly string[] _formats =
        [
            "d/M/yyyy h:mm tt",
            "dd/M/yyyy h:mm tt",
            "d/MM/yyyy h:mm tt",
            "dd/MM/yyyy h:mm tt",
            "d/M/yyyy h:mm:ss tt",
            "dd/M/yyyy h:mm:ss tt",
            "d/MM/yyyy h:mm:ss tt",
            "dd/MM/yyyy h:mm:ss tt"
        ];

        private readonly DateTimeKind _kind;
        private string _dateTimeString = string.Empty;

        public string DateTimeString
        {
            get => _dateTimeString;
            set
            {
                if (_dateTimeString == value)
                    return;

                var str = value.Replace(":00:00 ", ":00 ");
                SetProperty(ref _dateTimeString, str);

                if (!DateTime.TryParseExact(
                        str,
                        _formats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces,
                        out var dt))
                {
                    return;
                }

                Value = DateTime.SpecifyKind(dt, _kind);
            }
        }

        public override DateTime Value
        {
            get => _value;
            set
            {
                _value = value;
                var formatted = value.ToString("dd/MM/yyyy hh:mm tt");
                SetProperty(ref _dateTimeString, formatted, nameof(DateTimeString));
                OnPropertyChanged();
                OnChange?.Invoke(value);
            }
        }

        public InputDateTimeVM(string description, DateTime value, DateTimeKind kind, Action<DateTime> onChange)
            : base(description, value, onChange)
        {
            _kind = kind;
            _value = DateTime.SpecifyKind(value, _kind);
            _dateTimeString = _value.ToString("dd/MM/yyyy hh:mm tt");
        }
    }
}
