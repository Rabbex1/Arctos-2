using System.Windows.Media;

namespace Arctos.Wpf.AvalonDock
{
    public abstract class DockingPaneVM : BaseVM, IDockingPaneVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        protected Dictionary<string, string> Content = new();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region IsActive
        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }
        #endregion

        #region IsSelected
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        #endregion

        #region ContentId
        public string ContentId
        {
            get
            {
                var isFirst = true;
                var contentId = "";
                foreach (var component in Content)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        contentId += ';';
                    contentId += component.Key + "=" + component.Value;
                }
                return contentId;
            }
        }

        #endregion

        #region Width
        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                if (Math.Abs(_width - value) < 10e-3) return;
                SetProperty(ref _width, value);
            }
        }
        #endregion

        #region Status
        private StatusVM _status = new();
        public StatusVM Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        #endregion

        public ImageSource? IconSource { get; protected set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        protected DockingPaneVM(string title)
        {
            DisplayName = title;
            var fullName = GetType().FullName;
            if (fullName != null) SetContentComponent("Class", fullName);
        }

        #endregion Constructor

        public void SetContentComponent(string key, string value)
        {
            if (!Content.TryAdd(key, value))
                Content[key] = value;
            OnPropertyChanged(nameof(ContentId));
        }

        public static Dictionary<string, string> GetContentComponents(string contentId)
        {
            var components = new Dictionary<string, string>();

            var componentStrings = contentId.Split([';'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var @string in componentStrings)
            {
                var componentParts = @string.Split(['='], StringSplitOptions.RemoveEmptyEntries);
                if (componentParts.Length != 2)
                    continue;
                components.Add(componentParts[0], componentParts[1]);
            }

            return components;
        }
    }
}
