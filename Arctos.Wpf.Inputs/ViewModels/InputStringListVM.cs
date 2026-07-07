using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Arctos.Wpf.Inputs;

public sealed class InputStringListVM : DataInputWrapperVM<List<string>>
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Nested Types

    public sealed class StringListItemVM(Action onChanged) : BaseVM
    {
        private string _value = string.Empty;
        private bool _suppressChange;

        public string Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    if (!_suppressChange)
                        onChanged();
                }
            }
        }

        public void SetValueSilently(string? value)
        {
            _suppressChange = true;
            Value = value ?? string.Empty;
            _suppressChange = false;
        }
    }

    #endregion Nested Types

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private StringListItemVM? _selectedItem;
    private readonly Action<string?>? _onSelectedItemChanged;
    private bool _suppressNotifications;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public ObservableCollection<StringListItemVM> Items { get; } = [];

    public StringListItemVM? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value))
                return;

            if (!_suppressNotifications)
                _onSelectedItemChanged?.Invoke(value?.Value);
            RaisePropertyChanged(nameof(CanRemove));
            RaisePropertyChanged(nameof(CanMoveUp));
            RaisePropertyChanged(nameof(CanMoveDown));
        }
    }

    public bool CanRemove => SelectedItem != null;

    public bool CanMoveUp => SelectedItem != null && Items.IndexOf(SelectedItem) > 0;

    public bool CanMoveDown => SelectedItem != null && Items.IndexOf(SelectedItem) >= 0 && Items.IndexOf(SelectedItem) < Items.Count - 1;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    public InputStringListVM(string description, IEnumerable<string>? value, string? selectedValue, Action<List<string>?>? onChange, Action<string?>? onSelectedItemChanged = null)
        : base(description, value?.ToList() ?? [], onChange)
    {
        _onSelectedItemChanged = onSelectedItemChanged;
        _suppressNotifications = true;

        foreach (var item in Value ?? [])
            Items.Add(CreateItem(item));

        SelectedItem = Items.FirstOrDefault(item => item.Value == (selectedValue ?? string.Empty)) ?? Items.FirstOrDefault();
        _suppressNotifications = false;
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    public void UpdateItems(IEnumerable<string>? items, string? selectedValue)
    {
        _suppressNotifications = true;

        Items.Clear();
        foreach (var item in items ?? [])
            Items.Add(CreateItem(item));

        var normalizedItems = Items.Select(item => item.Value).ToList();
        UpdateValue(normalizedItems);

        SelectedItem = Items.FirstOrDefault(item => item.Value == (selectedValue ?? string.Empty)) ?? Items.FirstOrDefault();

        _suppressNotifications = false;
        RaisePropertyChanged(nameof(CanRemove));
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Commands

    private RelayCommand? _addCommand;
    public RelayCommand AddCommand => _addCommand ??= new RelayCommand(AddItem);

    private RelayCommand? _removeCommand;
    public RelayCommand RemoveCommand => _removeCommand ??= new RelayCommand(RemoveSelectedItem);

    private RelayCommand? _moveUpCommand;
    public RelayCommand MoveUpCommand => _moveUpCommand ??= new RelayCommand(MoveSelectedItemUp);

    private RelayCommand? _moveDownCommand;
    public RelayCommand MoveDownCommand => _moveDownCommand ??= new RelayCommand(MoveSelectedItemDown);

    #endregion Commands

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private void AddItem()
    {
        CommitFocusedItemEdit();

        var item = CreateItem(string.Empty);
        Items.Add(item);
        _suppressNotifications = true;
        SelectedItem = item;
        _suppressNotifications = false;
        SyncValueFromItems();
    }

    private void RemoveSelectedItem()
    {
        CommitFocusedItemEdit();

        if (SelectedItem == null)
            return;

        var index = Items.IndexOf(SelectedItem);
        if (index < 0)
            return;

        _suppressNotifications = true;
        Items.RemoveAt(index);
        SelectedItem = Items.Count == 0
            ? null
            : index <= 0
                ? Items[0]
                : Items[index - 1];
        _suppressNotifications = false;
        SyncValueFromItems();
    }

    private void MoveSelectedItemUp()
    {
        CommitFocusedItemEdit();

        if (SelectedItem == null)
            return;

        var index = Items.IndexOf(SelectedItem);
        if (index <= 0)
            return;

        _suppressNotifications = true;
        Items.Move(index, index - 1);
        _suppressNotifications = false;
        SyncValueFromItems();
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    private void MoveSelectedItemDown()
    {
        CommitFocusedItemEdit();

        if (SelectedItem == null)
            return;

        var index = Items.IndexOf(SelectedItem);
        if (index < 0 || index >= Items.Count - 1)
            return;

        _suppressNotifications = true;
        Items.Move(index, index + 1);
        _suppressNotifications = false;
        SyncValueFromItems();
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    private StringListItemVM CreateItem(string? value)
    {
        var item = new StringListItemVM(SyncValueFromItems);
        item.SetValueSilently(value);
        return item;
    }

    private void SyncValueFromItems()
    {
        if (!_suppressNotifications)
            _onSelectedItemChanged?.Invoke(SelectedItem?.Value);

        Value = [.. Items.Select(item => item.Value)];

        RaisePropertyChanged(nameof(CanRemove));
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    private static void CommitFocusedItemEdit()
    {
        if (Keyboard.FocusedElement is not TextBox textBox)
            return;

        if (textBox.DataContext is not StringListItemVM)
            return;

        BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty)?.UpdateSource();
    }

    #endregion Private Methods
}
