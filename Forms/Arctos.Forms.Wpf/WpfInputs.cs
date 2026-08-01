using Arctos;
using Arctos.Wpf.Dialogs;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;

namespace Arctos.Forms.Wpf;

public sealed class InputFilePathVM : DataInputWrapperVM<string>
{
    private string _fileName = string.Empty;

    public InputFilePathVM(string description, string value, Action<string?> onChange)
        : base(description, value, onChange)
    {
        FileName = Path.GetFileName(value);
        OnButtonClicked = OpenDialog;
    }

    public string DialogTitle { get; set; } = "Select File";

    public string FileName
    {
        get => _fileName;
        set => SetProperty(ref _fileName, value);
    }

    public string FileGroup { get; set; } = string.Empty;

    public string[] FileExtensions { get; set; } = ["*"];

    public bool SaveAs { get; set; }

    public string InitialDirectory { get; set; } = string.Empty;

    public bool AllowSpaces { get; set; } = true;

    private void OpenDialog(DataInputWrapperVM<string> wrapper)
    {
        var directory = string.IsNullOrEmpty(InitialDirectory)
            ? string.IsNullOrEmpty(Path.GetDirectoryName(Value))
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : Path.GetDirectoryName(Value)
            : InitialDirectory;

        var filter = BuildFilter(FileGroup, FileExtensions);
        string? filePath = null;

        if (SaveAs)
        {
            var dialog = new SaveFileDialogVM
            {
                FileName = AllowSpaces ? FileName : FileName?.Replace(' ', '_'),
                InitialDirectory = directory,
                Filter = filter,
                Title = DialogTitle
            };

            new SaveFileDialogPresenter().Show(dialog);
            if (dialog.Result)
                filePath = dialog.FileName;

            if (!AllowSpaces && !string.IsNullOrWhiteSpace(filePath))
                filePath = filePath.Replace(' ', '_');
        }
        else
        {
            var dialog = new OpenFileDialogVM
            {
                FileName = FileName,
                InitialDirectory = directory,
                Filter = filter,
                Title = DialogTitle
            };

            new OpenFileDialogPresenter().Show(dialog);
            if (dialog.Result)
                filePath = dialog.FileName;
        }

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        Value = filePath;
        FileName = Path.GetFileName(filePath);
    }

    private static string BuildFilter(string fileGroup, IEnumerable<string> extensions)
    {
        var patterns = extensions
            .Select(NormalizePattern)
            .Where(pattern => !string.IsNullOrWhiteSpace(pattern))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (patterns.Length == 0)
            return "All Files (*.*)|*.*";

        var groupName = string.IsNullOrWhiteSpace(fileGroup) ? "Files" : fileGroup;
        return string.Join("|", patterns.Select(pattern => $"{groupName} ({pattern})|{pattern}"));
    }

    private static string NormalizePattern(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        var value = extension.Trim();
        if (value == "*")
            return "*.*";

        if (value.StartsWith('.'))
            return $"*{value}";

        if (value.Contains('*') || value.Contains('?'))
            return value;

        return $"*.{value}";
    }
}

public sealed class InputFolderVM : DataInputWrapperVM<DirectoryInfo>
{
    private string _folderName = string.Empty;

    public InputFolderVM(string description, DirectoryInfo value, Action<DirectoryInfo?> onChange)
        : base(description, value, onChange)
    {
        OnButtonClicked = OpenDialog;
        Icon = "ArctosWpf:MeatballMenu";
        FolderName = value.Name;
    }

    public string FolderName
    {
        get => _folderName;
        set => SetProperty(ref _folderName, value);
    }

    public string Title { get; set; } = "Select Folder";

    private void OpenDialog(DataInputWrapperVM<DirectoryInfo> wrapper)
    {
        var initialDirectory = string.IsNullOrEmpty(Value?.FullName)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : Value.FullName;

        var dialog = new SelectFolderDialogVM
        {
            Title = Title,
            InitialDirectory = initialDirectory,
            FolderName = initialDirectory
        };

        new SelectFolderDialogPresenter().Show(dialog);

        if (!dialog.Result || string.IsNullOrWhiteSpace(dialog.FolderName))
            return;

        Value = new DirectoryInfo(dialog.FolderName);
        FolderName = Value.Name;
    }
}

public sealed class InputColorVM : DataInputWrapperVM<Color>
{
    private SolidColorBrush? _brush;

    public InputColorVM(string description, Color color, Action<Color> onChange)
        : base(description, color, onChange)
    {
        Brush = new SolidColorBrush(color);
        OnButtonClicked = OpenDialog;
    }

    public SolidColorBrush? Brush
    {
        get => _brush;
        set => SetProperty(ref _brush, value);
    }

    public void UpdateColor(Color color)
    {
        UpdateValue(color);
        Brush = new SolidColorBrush(color);
    }

    private void OpenDialog(DataInputWrapperVM<Color> wrapper)
    {
        var currentColor = Brush?.Color ?? Value;
        var dialog = new ColorDialogVM
        {
            Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B),
            AllowFullOpen = true,
            AnyColor = true,
            SolidColorOnly = false,
            FullOpen = true,
            DisplayName = "Select Color"
        };

        new ColorDialogPresenter().Show(dialog);
        if (!dialog.Result)
            return;

        var newColor = dialog.Color;
        Value = Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
        Brush = new SolidColorBrush(Value);
    }
}

public sealed class NamedColorValue
{
    public string Name { get; set; } = string.Empty;

    public Color Color { get; set; } = Colors.White;
}

public sealed class InputNamedColorListVM : DataInputWrapperVM<List<NamedColorValue>>
{
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

    private NamedColorListItemVM? _selectedItem;
    private RelayCommand? _addCommand;
    private RelayCommand? _removeCommand;
    private RelayCommand? _moveUpCommand;
    private RelayCommand? _moveDownCommand;

    public InputNamedColorListVM(string description, IEnumerable<NamedColorValue>? value, Action<List<NamedColorValue>?>? onChange)
        : base(description, value?.Select(Clone).ToList() ?? [], onChange)
    {
        foreach (var item in Value ?? [])
            Items.Add(CreateItem(item));

        SelectedItem = Items.FirstOrDefault();
    }

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

    public RelayCommand AddCommand => _addCommand ??= new RelayCommand(AddItem);

    public RelayCommand RemoveCommand => _removeCommand ??= new RelayCommand(RemoveSelectedItem);

    public RelayCommand MoveUpCommand => _moveUpCommand ??= new RelayCommand(MoveSelectedItemUp);

    public RelayCommand MoveDownCommand => _moveDownCommand ??= new RelayCommand(MoveSelectedItemDown);

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
        SelectedItem = index < Items.Count ? Items[index] : Items.LastOrDefault();
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
            Color = value.Color
        };
    }

    private void SyncValueFromItems()
    {
        Value =
        [
            .. Items.Select(item => new NamedColorValue
            {
                Name = item.Name,
                Color = item.Color
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
            Color = value.Color
        };
    }
}
