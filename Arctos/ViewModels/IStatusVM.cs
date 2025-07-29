namespace Arctos
{
    public interface IStatusVM: IBaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public string WaitingText { get; set; }
        public bool IsWaiting { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasError { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void StartWaiting(string text);
        public void EndWaiting();
        public void SetError(string text);
        public void ClearError();

        #endregion Methods
    }
}
