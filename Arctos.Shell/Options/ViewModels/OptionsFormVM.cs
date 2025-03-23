using Arctos.Wpf.Dialogs;
using Arctos.Wpf.ViewModels;

namespace Arctos.Shell.Options
{
  public class OptionsFormVM : BaseVM
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public string? ParentId { get; set; }
    public string? FormId { get; set; }
    public Guid OwnerId { get; set; }
    public OptionsPage? Data { get; set; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods
    public virtual void Initialize()
    {

    }


    public virtual void OnEnteringPage()
    {

    }

    public virtual void OnLeavingPage()
    {

    }

    #endregion Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Event Handlers

    public virtual void OnDialogOpening(object? sender, EventArgs e)
    {
      Data?.MakeTempCopy();
    }

    public virtual void OnDialogClosing(object? sender, CloseDialogArgs e)
    {
        if (e.Result == DialogResult.Ok)
          Data?.UpdateFromTempCopy();
        Data?.DeleteTempCopy();
    }

    #endregion Event Handlers

  }
}
