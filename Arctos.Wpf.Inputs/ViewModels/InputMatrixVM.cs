using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Arctos.Wpf.Inputs
{
    public interface IInputMatrixVM
    {
        int Rows { get; }

        int Columns { get; }

        ObservableCollection<InputMatrixCellVM> Cells { get; }
    }

    public sealed class InputMatrixVM<T> : DataInputWrapperVM<T[]>, IInputMatrixVM
    {
        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Func<T?, string> _formatter;
        private readonly Func<string, (bool success, T? value)> _parser;
        private bool _isSynchronizingCells;

        #endregion Fields

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public int Rows { get; }

        public int Columns { get; }

        public ObservableCollection<InputMatrixCellVM> Cells { get; } = [];

        public override T[]? Value
        {
            get => base.Value;
            set
            {
                var normalized = Normalize(value, Rows, Columns);
                base.Value = normalized;

                if (_isSynchronizingCells)
                    return;

                SyncCellsFromValue(normalized);
            }
        }

        #endregion Properties

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public InputMatrixVM(string description, int rows, int columns, T[]? data, Action<T[]?>? onChange)
            : this(description, rows, columns, data, onChange, null, null)
        {
        }

        public InputMatrixVM(
            string description,
            int rows,
            int columns,
            T[]? data,
            Action<T[]?>? onChange,
            Func<T?, string>? formatter,
            Func<string, (bool success, T? value)>? parser)
            : base(description, Normalize(data, rows, columns), onChange)
        {
            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "Rows must be greater than zero.");

            if (columns <= 0)
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "Columns must be greater than zero.");

            Rows = rows;
            Columns = columns;
            _formatter = formatter ?? GetDefaultFormatter();
            _parser = parser ?? GetDefaultParser();

            BuildCells();
        }

        #endregion Constructors

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Private Methods

        private void BuildCells()
        {
            Cells.Clear();

            var values = Value ?? Normalize(null, Rows, Columns);

            for (var row = 0; row < Rows; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    var index = ToIndex(row, column);
                    Cells.Add(new InputMatrixCellVM(row, column, _formatter(values[index]), OnCellTextChanged));
                }
            }
        }

        private void OnCellTextChanged(InputMatrixCellVM cell, string text)
        {
            if (_isSynchronizingCells)
                return;

            var (success, value) = _parser(text);
            if (!success)
            {
                var existingValues = Value ?? Normalize(null, Rows, Columns);
                cell.SetTextSilently(_formatter(existingValues[ToIndex(cell.Row, cell.Column)]));
                return;
            }

            var values = Value is { Length: > 0 }
                ? [.. Value]
                : Normalize(null, Rows, Columns);

            values[ToIndex(cell.Row, cell.Column)] = value!;

            _isSynchronizingCells = true;
            base.Value = values;
            _isSynchronizingCells = false;
        }

        private void SyncCellsFromValue(T[] values)
        {
            _isSynchronizingCells = true;

            for (var index = 0; index < Math.Min(values.Length, Cells.Count); index++)
                Cells[index].SetTextSilently(_formatter(values[index]));

            _isSynchronizingCells = false;
        }

        private int ToIndex(int row, int column)
        {
            return (row * Columns) + column;
        }

        private static T[] Normalize(T[]? value, int rows, int columns)
        {
            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows), rows, "Rows must be greater than zero.");

            if (columns <= 0)
                throw new ArgumentOutOfRangeException(nameof(columns), columns, "Columns must be greater than zero.");

            var length = rows * columns;
            if (value == null || value.Length == 0)
                return new T[length];

            if (value.Length == length)
                return [.. value];

            return value
                .Concat(Enumerable.Repeat(default(T)!, Math.Max(0, length - value.Length)))
                .Take(length)
                .ToArray();
        }

        private static Func<T?, string> GetDefaultFormatter()
        {
            return typeof(T) switch
            {
                var type when type == typeof(double) => value => value is null ? string.Empty : ((double)(object)value).ToString("N4", CultureInfo.CurrentCulture),
                var type when type == typeof(float) => value => value is null ? string.Empty : ((float)(object)value).ToString("N4", CultureInfo.CurrentCulture),
                var type when type == typeof(int) => value => value?.ToString() ?? string.Empty,
                var type when type == typeof(uint) => value => value?.ToString() ?? string.Empty,
                var type when type == typeof(string) => value => value as string ?? string.Empty,
                _ => value => value?.ToString() ?? string.Empty,
            };
        }

        private static Func<string, (bool success, T? value)> GetDefaultParser()
        {
            return typeof(T) switch
            {
                var type when type == typeof(double) => text => double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var parsed)
                    ? (true, (T?)(object?)parsed)
                    : (false, default),
                var type when type == typeof(float) => text => float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var parsed)
                    ? (true, (T?)(object?)parsed)
                    : (false, default),
                var type when type == typeof(int) => text => int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsed)
                    ? (true, (T?)(object?)parsed)
                    : (false, default),
                var type when type == typeof(uint) => text => uint.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsed)
                    ? (true, (T?)(object?)parsed)
                    : (false, default),
                var type when type == typeof(string) => text => (true, (T?)(object?)text),
                _ => throw new NotSupportedException($"InputMatrixVM does not provide a default parser for '{typeof(T).Name}'. Supply custom formatter and parser delegates."),
            };
        }

        #endregion Private Methods
    }

    public sealed class InputMatrixCellVM : BaseVM
    {
        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Action<InputMatrixCellVM, string> _onChange;
        private bool _suppressChange;
        private string _text = string.Empty;

        #endregion Fields

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public int Row { get; }

        public int Column { get; }

        public string Text
        {
            get => _text;
            set
            {
                if (!SetProperty(ref _text, value ?? string.Empty))
                    return;

                if (_suppressChange)
                    return;

                _onChange(this, _text);
            }
        }

        #endregion Properties

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public InputMatrixCellVM(int row, int column, string text, Action<InputMatrixCellVM, string> onChange)
        {
            Row = row;
            Column = column;
            _text = text ?? string.Empty;
            _onChange = onChange ?? throw new ArgumentNullException(nameof(onChange));
        }

        #endregion Constructors

        //--------------------------------------------------------------------------------------------------------------------------------------
        #region Internal Methods

        internal void SetTextSilently(string text)
        {
            _suppressChange = true;
            Text = text ?? string.Empty;
            _suppressChange = false;
        }

        #endregion Internal Methods
    }
}
