
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Wpf.ViewModels
{
  public class StatusVM : ObservableObject
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    #region WaitingText
    private string _waitingText = string.Empty;
    public string WaitingText
    {
      get => _waitingText;
      set => SetProperty(ref _waitingText, value);
    }
        #endregion

        #region IsWaiting
        private bool _isWaiting;
    public bool IsWaiting
    {
      get => _isWaiting;
      set => SetProperty(ref _isWaiting, value);
    }
        #endregion

        #region ErrorMessage
    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
      get => _errorMessage;
      set => SetProperty(ref _errorMessage, value);
    }
        #endregion

        #region HasError
        private bool _hasError;
    public bool HasError
    {
      get => _hasError;
      set => SetProperty(ref _hasError, value);
    }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void StartWaiting(string text)
    {
      WaitingText = text;
      IsWaiting = true;
    }

    public void EndWaiting()
    {
      WaitingText = "";
      IsWaiting = false;
    }

    public void SetError(string text)
    {
      ErrorMessage = text;
      HasError = true;
      if (IsWaiting)
        EndWaiting();
    }

    public void ClearError()
    {
      ErrorMessage = "";
      HasError = false;
    }

    #endregion Methods

  }
}
