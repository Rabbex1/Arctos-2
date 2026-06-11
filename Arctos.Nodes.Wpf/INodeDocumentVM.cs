using Arctos.Shell.Documents;
using Arctos.Nodes.Wpf.ViewModels.Documents;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Wpf.Inputs;
using System.Collections.Generic;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Defines the shared surface exposed by node document view models.
/// </summary>
public interface INodeDocumentVM : IDocumentVM
{
    string FilePath { get; }

    Guid? DocumentId { get; }

    string DocumentName { get; }

    int DefinitionCount { get; }

    NavLinkVM? NavLinkRoot { get; }

    NavLinkVM? CurrentNavLink { get; }

    IEnumerable<NavLinkVM> NavLinks { get; }

    SkiaHostVM CurrentCanvas { get; }

    IModelHost? SelectedHost { get; }

    InputFormVM? PropertiesForm { get; }
}
