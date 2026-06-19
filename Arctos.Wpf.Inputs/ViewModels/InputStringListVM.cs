using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace Arctos.Wpf.Inputs;

public sealed class InputStringListVM : DataInputWrapperVM<List<string>>
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Nested Types

    public sealed class StringListItemVM(Action onChanged) : BaseVM
    {
        private string _value = string.Empty;

        public string Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                    onChanged();
            }
        }
    }

    #endregion Nested Types

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private StringListItemVM? _selectedItem;
    private readonly Action<string?>? _onSelectedItemChanged;

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

        foreach (var item in Value ?? [])
            Items.Add(CreateItem(item));

        SelectedItem = Items.FirstOrDefault(item => item.Value == (selectedValue ?? string.Empty)) ?? Items.FirstOrDefault();
    }

    #endregion Constructors

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
        var item = CreateItem(string.Empty);
        Items.Add(item);
        SelectedItem = item;
        SyncValueFromItems();
    }

    private void RemoveSelectedItem()
    {
        if (SelectedItem == null)
            return;

        var index = Items.IndexOf(SelectedItem);
        if (index < 0)
            return;

        Items.RemoveAt(index);
        SelectedItem = index < Items.Count
            ? Items[index]
            : Items.LastOrDefault();
        SyncValueFromItems();
    }

    private void MoveSelectedItemUp()
    {
        if (SelectedItem == null)
            return;

        var index = Items.IndexOf(SelectedItem);
        if (index <= 0)
            return;

        Items.Move(index, index - 1);
        SyncValueFromItems();
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    private void MoveSelectedItemDown()
    {
        if (SelectedItem == null)
            return;

        var index = Items.IndexOf(SelectedItem);
        if (index < 0 || index >= Items.Count - 1)
            return;

        Items.Move(index, index + 1);
        SyncValueFromItems();
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    private StringListItemVM CreateItem(string? value)
    {
        return new StringListItemVM(SyncValueFromItems)
        {
            Value = value ?? string.Empty,
        };
    }

    private void SyncValueFromItems()
    {
        Value = [.. Items.Select(item => item.Value)];
        _onSelectedItemChanged?.Invoke(SelectedItem?.Value);
        RaisePropertyChanged(nameof(CanRemove));
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    #endregion Private Methods
}
