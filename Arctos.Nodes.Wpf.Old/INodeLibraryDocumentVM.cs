using Arctos.Nodes.Wpf.ViewModels;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Defines the node-document surface for file-backed or hosted shared node libraries.
/// </summary>
public interface INodeLibraryDocumentVM : INodeDocumentVM
{
    NodeLibraryVM NodeLibrary { get; }
}
