namespace Arctos.Wpf.Inputs
{
    public sealed class InputIntRangeExtraVM : DataInputWrapperVM<string>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Dictionary<char, string> _specialValues;
        private string _stringValue = string.Empty;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public int Min { get; }

        public int Max { get; }

        public IReadOnlyList<(char Key, string Text)> SpecialValues { get; }

        public string AllowedValuesDescription
        {
            get
            {
                var specials = _specialValues.Count == 0
                    ? string.Empty
                    : $" or {string.Join(", ", _specialValues.Select(kvp => $"'{kvp.Key}' = {kvp.Value}"))}";
                return $"Enter a value between {Min} and {Max}{specials}.";
            }
        }

        public string StringValue
        {
            get => _stringValue;
            set
            {
                var normalized = NormalizeInput(value);
                if (_stringValue == normalized)
                    return;

                if (TryGetCommittedValue(normalized, out var committedValue))
                    Value = committedValue;

                SetProperty(ref _stringValue, normalized);
            }
        }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public InputIntRangeExtraVM(string description, string? value, int min, int max, Action<string?>? onChange, params (char Key, string Text)[] specialValues)
            : base(description, null, onChange)
        {
            Min = Math.Min(min, max);
            Max = Math.Max(min, max);
            _specialValues = BuildSpecialValueMap(specialValues);
            SpecialValues = _specialValues.Select(kvp => (kvp.Key, kvp.Value)).ToList();

            var initialValue = NormalizeInitialValue(value);
            _value = initialValue;
            _stringValue = initialValue;
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Public Methods

        public bool IsValidText(string? text, out string error)
        {
            var normalized = NormalizeInput(text);
            if (TryGetCommittedValue(normalized, out _))
            {
                error = string.Empty;
                return true;
            }

            error = AllowedValuesDescription;
            return false;
        }

        #endregion Public Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Private Methods

        private static Dictionary<char, string> BuildSpecialValueMap(IEnumerable<(char Key, string Text)> specialValues)
        {
            var result = new Dictionary<char, string>();

            foreach (var (key, text) in specialValues)
            {
                if (char.IsWhiteSpace(key) || text == null)
                    continue;

                result[char.ToLowerInvariant(key)] = text;
            }

            return result;
        }

        private string NormalizeInitialValue(string? value)
        {
            var normalized = NormalizeInput(value);
            if (TryGetCommittedValue(normalized, out var committedValue))
                return committedValue;

            return Min.ToString();
        }

        private string NormalizeInput(string? value)
        {
            var normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(normalized))
                return string.Empty;

            var lastChar = char.ToLowerInvariant(normalized[^1]);
            if (_specialValues.ContainsKey(lastChar))
                return lastChar.ToString();

            if (normalized.Length > 1)
            {
                var firstChar = char.ToLowerInvariant(normalized[0]);
                if (_specialValues.ContainsKey(firstChar))
                    normalized = normalized[1..].TrimStart();
            }

            return normalized;
        }

        private bool TryGetCommittedValue(string text, out string committedValue)
        {
            if (text.Length == 1)
            {
                var specialKey = char.ToLowerInvariant(text[0]);
                if (_specialValues.ContainsKey(specialKey))
                {
                    committedValue = specialKey.ToString();
                    return true;
                }
            }

            if (int.TryParse(text, out var parsed) && parsed >= Min && parsed <= Max)
            {
                committedValue = parsed.ToString();
                return true;
            }

            committedValue = string.Empty;
            return false;
        }

        #endregion Private Methods
    }
}
