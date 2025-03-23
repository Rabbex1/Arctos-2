using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Arctos.Shell.Options.Selectors
{
  public class OptionsFormDataTemplateSelector : DataTemplateSelector
  {
    private static readonly Dictionary<string, UserControl> Forms = new Dictionary<string, UserControl>();

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
      var formVM = item as OptionsFormVM;
      if (formVM == null || formVM.FormId == null)
        return null;

      var dialogFactory = new FrameworkElementFactory(Forms[formVM.FormId].GetType());
      Forms[formVM.FormId].DataContext = item;

      var template = new DataTemplate(formVM.GetType())
      {
        VisualTree = dialogFactory
      };
      return template;
    }

    public static void ClearViews()
    {
      Forms.Clear();
    }

    public static void AddOptionsFormView(string formId, UserControl view)
    {
      Forms.Add(formId, view);
    }

    public static void RemoveOptionsFormView(string? formId)
    {
        if (formId != null) 
            Forms.Remove(formId);
    }
  }
}