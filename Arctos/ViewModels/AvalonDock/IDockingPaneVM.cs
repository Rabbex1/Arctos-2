namespace Arctos
{
    public interface IDockingPaneVM: IBaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public bool IsActive { get; set; }
        public bool IsSelected { get; set; }
        public string ContentId { get; }
        public double Width { get; set; }

        #endregion Properties

        public void SetContentComponent(string key, string value);
    }
}
