using Arctos.Nodes.Wpf.ViewModels;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Defines the node-document surface for file-backed or hosted node maps.
/// </summary>
public interface INodeMapDocumentVM : INodeDocumentVM
{
    NodeMapVM NodeMap { get; }
}
