using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Arctos.Forms.Wpf;

public static class FormsWpfModule
{
    private static bool _isRegistered;

#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Initialize()
    {
        EnsureRegistered();
    }

    public static void EnsureRegistered()
    {
        if (_isRegistered)
            return;

        _isRegistered = true;

        DataInputWrapperVM.RegisterExtension((description, value, action) =>
        {
            switch (value)
            {
                case Color color:
                {
                    InputColorVM? wrapper = null;
                    wrapper = new InputColorVM(description, color, changedValue => action?.Invoke(wrapper!, changedValue));
                    return wrapper;
                }
                case DirectoryInfo directoryInfo:
                {
                    InputFolderVM? wrapper = null;
                    wrapper = new InputFolderVM(description, directoryInfo, changedValue => action?.Invoke(wrapper!, changedValue));
                    return wrapper;
                }
                case IEnumerable<NamedColorValue> namedColors:
                {
                    InputNamedColorListVM? wrapper = null;
                    wrapper = new InputNamedColorListVM(description, namedColors, changedValue => action?.Invoke(wrapper!, changedValue));
                    return wrapper;
                }
                default:
                    return null;
            }
        });
    }
}
