namespace Arctos.Nodes.Models;

/// <summary>
/// Defines the built-in input component type identifiers available to graph documents.
/// </summary>
public static class InputTypeIds
{
    #region Properties

    /// <summary>
    /// Gets the built-in boolean input type identifier.
    /// </summary>
    public static InputTypeId Boolean => new("Boolean");

    /// <summary>
    /// Gets the built-in signed integer input type identifier.
    /// </summary>
    public static InputTypeId Integer => new("Integer");

    /// <summary>
    /// Gets the built-in unsigned integer input type identifier.
    /// </summary>
    public static InputTypeId UnsignedInteger => new("UnsignedInteger");

    /// <summary>
    /// Gets the built-in floating-point input type identifier.
    /// </summary>
    public static InputTypeId Float => new("Float");

    /// <summary>
    /// Gets the built-in double-precision floating-point input type identifier.
    /// </summary>
    public static InputTypeId Double => new("Double");

    /// <summary>
    /// Gets the built-in string input type identifier.
    /// </summary>
    public static InputTypeId String => new("String");

    /// <summary>
    /// Gets the built-in date and time input type identifier.
    /// </summary>
    public static InputTypeId DateTime => new("DateTime");

    /// <summary>
    /// Gets the built-in time span input type identifier.
    /// </summary>
    public static InputTypeId TimeSpan => new("TimeSpan");

    /// <summary>
    /// Gets the built-in portable color input type identifier.
    /// </summary>
    public static InputTypeId Color => new("Color");

    /// <summary>
    /// Gets the built-in string selection input type identifier.
    /// </summary>
    public static InputTypeId StringSelection => new("StringSelection");

    /// <summary>
    /// Gets the built-in floating-point range input type identifier.
    /// </summary>
    public static InputTypeId FloatRange => new("FloatRange");

    /// <summary>
    /// Gets the built-in integer range input type identifier.
    /// </summary>
    public static InputTypeId IntRange => new("IntRange");

    #endregion Properties
}
