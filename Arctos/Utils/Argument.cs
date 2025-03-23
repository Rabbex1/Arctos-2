using System.Diagnostics;

namespace Arctos.Utils
{
  public class Argument
  {
    [DebuggerNonUserCode, DebuggerStepThrough]
    public static void IsNotNull(string paramName, object? paramValue)
    {
      if (paramValue == null)
      {
        throw new ArgumentNullException(paramName, $"Argument '{paramName}' cannot be null");
      }
    }

    [DebuggerNonUserCode, DebuggerStepThrough]
    public static void IsNotNullOrEmpty(string paramName, string paramValue)
    {
      if (string.IsNullOrEmpty(paramValue))
      {
        throw new ArgumentNullException(paramName, $"Argument '{paramName}' cannot be null or empty");
      }
    }

    [DebuggerNonUserCode, DebuggerStepThrough]
    public static void IsNotNullOrWhiteSpace(string paramName, string paramValue)
    {
      if (string.IsNullOrEmpty(paramValue) || string.IsNullOrWhiteSpace(paramValue))
      {
        throw new ArgumentNullException(paramName, $"Argument '{paramName}' cannot be null, empty or whitespace");
      }
    }

    [DebuggerNonUserCode, DebuggerStepThrough]
    public static void MustBeSubClassOf(string paramName, Type? paramValue, Type baseType)
    {
      if (paramValue == null || !paramValue.IsSubclassOf(baseType))
      {
        throw new ArgumentException(paramName, $"Argument '{paramName}' must be a subclass of '{baseType.Name}'");
      }
    }
    [DebuggerNonUserCode, DebuggerStepThrough]
    public static void MustBeSubClassOfOrNull(string paramName, Type? paramValue, Type baseType)
    {
      if (paramValue != null && !paramValue.IsSubclassOf(baseType))
      {
        throw new ArgumentException(paramName, $"Argument '{paramName}' must be a subclass of '{baseType.Name}'");
      }
    }
  }
}
