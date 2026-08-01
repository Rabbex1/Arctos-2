namespace Arctos.Nodes.Wpf;

/// <summary>
/// Exposes a displayable host around a model that can participate in navigation and selection flows.
/// </summary>
public interface IModelHost
{
    /// <summary>
    /// Gets the hosted model instance.
    /// </summary>
    object Model { get; }

    /// <summary>
    /// Gets the display name for the hosted model.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets a value indicating whether the hosted model supports an overview mode.
    /// </summary>
    bool HasOverview { get; }
}

/// <summary>
/// Exposes a strongly typed model through a shared host surface.
/// </summary>
/// <typeparam name="T">The hosted model type.</typeparam>
public interface IModelHost<out T> : IModelHost
{
    /// <summary>
    /// Gets the hosted model instance.
    /// </summary>
    new T Model { get; }
}
