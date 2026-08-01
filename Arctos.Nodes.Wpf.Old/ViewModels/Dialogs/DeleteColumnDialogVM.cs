using System.Windows.Input;
using Arctos.Nodes.Models;
using Arctos.Wpf;
using Arctos.Wpf.Utilities;

namespace Arctos.Nodes.Wpf.ViewModels.Dialogs;

/// <summary>
/// View model for the column delete options dialog.
/// </summary>
public sealed class DeleteColumnDialogVM : BaseVM
{
    private readonly bool _canDelete;
    private readonly bool _canMoveLeft;
    private readonly bool _canMoveRight;

    /// <summary>
    /// Initializes a new dialog state.
    /// </summary>
    /// <param name="canDelete">Whether deleting the selected column is allowed.</param>
    /// <param name="canMoveLeft">Whether moving the selected column content left is allowed.</param>
    /// <param name="canMoveRight">Whether moving the selected column content right is allowed.</param>
    public DeleteColumnDialogVM(bool canDelete, bool canMoveLeft, bool canMoveRight)
    {
        _canDelete = canDelete;
        _canMoveLeft = canMoveLeft;
        _canMoveRight = canMoveRight;
    }

    /// <summary>
    /// Gets the dialog result chosen by the user.
    /// </summary>
    public NodeColumnDeleteRule Result { get; private set; } = NodeColumnDeleteRule.Cancel;

    /// <summary>
    /// Gets whether the delete action is enabled.
    /// </summary>
    public bool CanDelete => _canDelete;

    /// <summary>
    /// Gets whether the move-left action is enabled.
    /// </summary>
    public bool CanMoveLeft => _canMoveLeft;

    /// <summary>
    /// Gets whether the move-right action is enabled.
    /// </summary>
    public bool CanMoveRight => _canMoveRight;

    /// <summary>
    /// Raised when the dialog should close.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Gets the command that confirms a direct column delete.
    /// </summary>
    public ICommand DeleteCommand => new RelayCommand(
        _ => Close(NodeColumnDeleteRule.Delete),
        _ => CanDelete);

    /// <summary>
    /// Gets the command that moves content into the column on the left.
    /// </summary>
    public ICommand MoveLeftCommand => new RelayCommand(
        _ => Close(NodeColumnDeleteRule.MoveLeft),
        _ => CanMoveLeft);

    /// <summary>
    /// Gets the command that moves content into the column on the right.
    /// </summary>
    public ICommand MoveRightCommand => new RelayCommand(
        _ => Close(NodeColumnDeleteRule.MoveRight),
        _ => CanMoveRight);

    /// <summary>
    /// Gets the command that cancels the dialog.
    /// </summary>
    public ICommand CancelCommand => new RelayCommand(_ => Close(NodeColumnDeleteRule.Cancel));

    private void Close(NodeColumnDeleteRule result)
    {
        Result = result;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
