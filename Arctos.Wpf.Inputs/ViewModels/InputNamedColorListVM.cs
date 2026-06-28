using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace Arctos.Wpf.Inputs;

/// <summary>
/// Provides a reusable editable list of named colors.
/// </summary>
public sealed class InputNamedColorListVM : DataInputWrapperVM<List<NamedColorValue>>
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Nested Types

    public sealed class NamedColorListItemVM(Action onChanged) : BaseVM
    {
        private string _name = string.Empty;
        private Color _color = Colors.White;

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    onChanged();
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                if (SetProperty(ref _color, value))
                    onChanged();
            }
        }
    }

    #endregion Nested Types

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private NamedColorListItemVM? _selectedItem;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public ObservableCollection<NamedColorListItemVM> Items { get; } = [];

    public NamedColorListItemVM? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value))
                return;

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

    public InputNamedColorListVM(string description, IEnumerable<NamedColorValue>? value, Action<List<NamedColorValue>?>? onChange)
        : base(description, value?.Select(Clone).ToList() ?? [], onChange)
    {
        foreach (var item in Value ?? [])
            Items.Add(CreateItem(item));

        SelectedItem = Items.FirstOrDefault();
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
        var item = CreateItem(new NamedColorValue());
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

    private NamedColorListItemVM CreateItem(NamedColorValue value)
    {
        return new NamedColorListItemVM(SyncValueFromItems)
        {
            Name = value.Name ?? string.Empty,
            Color = value.Color,
        };
    }

    private void SyncValueFromItems()
    {
        Value =
        [
            .. Items.Select(item => new NamedColorValue
            {
                Name = item.Name,
                Color = item.Color,
            })
        ];

        RaisePropertyChanged(nameof(CanRemove));
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    private static NamedColorValue Clone(NamedColorValue value)
    {
        return new NamedColorValue
        {
            Name = value.Name,
            Color = value.Color,
        };
    }

    #endregion Private Methods
}
