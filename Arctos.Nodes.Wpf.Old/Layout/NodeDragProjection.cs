using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Nodes.Wpf.ViewModels.Components;

namespace Arctos.Nodes.Wpf.Layout;

/// <summary>
/// Represents the transient row/column structure used to evaluate drag preview placement.
/// </summary>
internal sealed class NodeDragProjection
{
    /// <summary>
    /// Gets the projected columns used during drag evaluation.
    /// </summary>
    public required IReadOnlyList<NodeDragProjectedColumn> Columns { get; init; }

    /// <summary>
    /// Gets the transferred components that are logically removed from the projected structure.
    /// </summary>
    public required NodeDragTransfer Transfer { get; init; }

    /// <summary>
    /// Resolves a projected column by zero-based visual index.
    /// </summary>
    /// <param name="columnIndex">The requested projected column index.</param>
    /// <returns>The resolved projected column.</returns>
    public NodeDragProjectedColumn GetColumn(int columnIndex)
    {
        if (Columns.Count == 0)
            throw new InvalidOperationException("A drag projection must contain at least one column.");

        var resolvedIndex = Math.Clamp(columnIndex, 0, Columns.Count - 1);
        return Columns[resolvedIndex];
    }
}

/// <summary>
/// Represents the payload being moved during drag evaluation.
/// </summary>
internal sealed class NodeDragTransfer
{
    /// <summary>
    /// Gets the components currently being transferred.
    /// </summary>
    public required IReadOnlyList<NodeComponentVM> Components { get; init; }
}

/// <summary>
/// Represents a transient projected column used during drag evaluation.
/// </summary>
internal sealed class NodeDragProjectedColumn
{
    /// <summary>
    /// Gets the zero-based visual column index.
    /// </summary>
    public required int Index { get; init; }

    /// <summary>
    /// Gets the stable column identifier from the persisted model.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the projected rows owned by this column.
    /// </summary>
    public required IReadOnlyList<NodeDragProjectedRow> Rows { get; init; }
}

/// <summary>
/// Represents a transient projected row used during drag evaluation.
/// </summary>
internal sealed class NodeDragProjectedRow
{
    /// <summary>
    /// Gets the stable row identifier from the persisted model.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the zero-based projected row order inside the owning column.
    /// </summary>
    public required int YOrder { get; init; }

    /// <summary>
    /// Gets the ordered components that remain in this row after transfer extraction.
    /// </summary>
    public required IReadOnlyList<NodeComponentVM> Components { get; init; }
}

/// <summary>
/// Builds transient drag projections from the persisted node structure.
/// </summary>
internal static class NodeDragProjectionBuilder
{
    /// <summary>
    /// Builds a projected drag structure for the supplied definition host.
    /// </summary>
    /// <param name="definitionHost">The node definition host that owns the persisted structure.</param>
    /// <param name="transferComponents">The components currently being moved.</param>
    /// <returns>The projected drag structure used for hover calculations.</returns>
    public static NodeDragProjection Build(NodeDefinitionVM definitionHost, IEnumerable<NodeComponentVM>? transferComponents = null)
    {
        ArgumentNullException.ThrowIfNull(definitionHost);

        var transferList = transferComponents?
            .Where(component => component != null)
            .Distinct()
            .ToList() ?? [];
        var transferredModels = transferList
            .Select(component => component.Model)
            .ToHashSet();
        var componentByModel = definitionHost.NodeComponents.ToDictionary(component => component.Model);
        var projectedColumns = new List<NodeDragProjectedColumn>();
        var orderedColumns = definitionHost.Model.Columns
            .OrderBy(column => column.XOrder)
            .ThenBy(column => column.Id, StringComparer.Ordinal)
            .ToList();

        for (var columnIndex = 0; columnIndex < orderedColumns.Count; columnIndex++)
        {
            var column = orderedColumns[columnIndex];
            var projectedRows = new List<NodeDragProjectedRow>();
            var orderedRows = column.Rows
                .OrderBy(row => row.YOrder)
                .ThenBy(row => row.Id, StringComparer.Ordinal)
                .ToList();

            foreach (var row in orderedRows)
            {
                var rowComponents = row.Components
                    .Where(component => !transferredModels.Contains(component))
                    .Select(component => componentByModel.TryGetValue(component, out var componentVm) ? componentVm : null)
                    .Where(componentVm => componentVm != null)
                    .Cast<NodeComponentVM>()
                    .OrderBy(componentVm => IsButtonLikeComponent(componentVm) ? componentVm.XOrder : 0)
                    .ThenBy(componentVm => componentVm.Model.Id, StringComparer.Ordinal)
                    .ToList();
                if (rowComponents.Count == 0)
                    continue;

                projectedRows.Add(new NodeDragProjectedRow
                {
                    Id = row.Id,
                    YOrder = projectedRows.Count,
                    Components = rowComponents,
                });
            }

            projectedColumns.Add(new NodeDragProjectedColumn
            {
                Index = columnIndex,
                Id = column.Id,
                Rows = projectedRows,
            });
        }

        return new NodeDragProjection
        {
            Columns = projectedColumns,
            Transfer = new NodeDragTransfer
            {
                Components = transferList,
            },
        };
    }

    /// <summary>
    /// Returns a new projection with the transfer payload inserted at the requested target.
    /// </summary>
    /// <param name="projection">The source projection.</param>
    /// <param name="columnIndex">The zero-based target column index.</param>
    /// <param name="rowIndex">The zero-based target row index.</param>
    /// <param name="inlineWithExistingRow"><see langword="true"/> to insert into an existing row; otherwise, <see langword="false"/>.</param>
    /// <param name="xOrder">The zero-based inline insertion index used for button rows.</param>
    /// <returns>The inserted projection.</returns>
    public static NodeDragProjection InsertTransfer(NodeDragProjection projection, int columnIndex, int rowIndex, bool inlineWithExistingRow, int xOrder)
    {
        ArgumentNullException.ThrowIfNull(projection);

        var transferComponents = projection.Transfer.Components
            .Distinct()
            .ToList();
        if (transferComponents.Count == 0)
            return projection;

        var projectedColumns = projection.Columns
            .Select(column => new NodeDragProjectedColumn
            {
                Index = column.Index,
                Id = column.Id,
                Rows = column.Rows
                    .Select(row => new NodeDragProjectedRow
                    {
                        Id = row.Id,
                        YOrder = row.YOrder,
                        Components = row.Components.ToList(),
                    })
                    .ToList(),
            })
            .ToList();

        var resolvedColumnIndex = Math.Clamp(columnIndex, 0, projectedColumns.Count - 1);
        var targetColumn = projectedColumns[resolvedColumnIndex];
        var targetRows = targetColumn.Rows.ToList();

        if (inlineWithExistingRow && targetRows.Count > 0)
        {
            var resolvedRowIndex = Math.Clamp(rowIndex, 0, targetRows.Count - 1);
            var targetRow = targetRows[resolvedRowIndex];
            var rowComponents = targetRow.Components.ToList();
            var resolvedXOrder = Math.Clamp(xOrder, 0, rowComponents.Count);
            rowComponents.InsertRange(resolvedXOrder, transferComponents);
            targetRows[resolvedRowIndex] = new NodeDragProjectedRow
            {
                Id = targetRow.Id,
                YOrder = resolvedRowIndex,
                Components = NormalizeRowComponents(rowComponents),
            };
        }
        else
        {
            var resolvedRowIndex = Math.Clamp(rowIndex, 0, targetRows.Count);
            targetRows.Insert(resolvedRowIndex, new NodeDragProjectedRow
            {
                Id = Guid.NewGuid().ToString("N"),
                YOrder = resolvedRowIndex,
                Components = NormalizeRowComponents(transferComponents),
            });
        }

        projectedColumns[resolvedColumnIndex] = new NodeDragProjectedColumn
        {
            Index = targetColumn.Index,
            Id = targetColumn.Id,
            Rows = NormalizeRows(targetRows),
        };

        return new NodeDragProjection
        {
            Columns = projectedColumns,
            Transfer = projection.Transfer,
        };
    }

    /// <summary>
    /// Returns a new projection with the specified column moved to a new visual index.
    /// </summary>
    /// <param name="projection">The source projection.</param>
    /// <param name="sourceIndex">The zero-based source column index.</param>
    /// <param name="targetIndex">The zero-based destination column index.</param>
    /// <returns>The reordered projection.</returns>
    public static NodeDragProjection MoveColumn(NodeDragProjection projection, int sourceIndex, int targetIndex)
    {
        ArgumentNullException.ThrowIfNull(projection);

        var projectedColumns = projection.Columns.ToList();
        if (projectedColumns.Count <= 1)
            return projection;

        var resolvedSourceIndex = Math.Clamp(sourceIndex, 0, projectedColumns.Count - 1);
        var resolvedTargetIndex = Math.Clamp(targetIndex, 0, projectedColumns.Count - 1);
        if (resolvedSourceIndex == resolvedTargetIndex)
            return projection;

        var movedColumn = projectedColumns[resolvedSourceIndex];
        projectedColumns.RemoveAt(resolvedSourceIndex);
        projectedColumns.Insert(resolvedTargetIndex, movedColumn);

        return new NodeDragProjection
        {
            Columns = projectedColumns
                .Select((column, index) => new NodeDragProjectedColumn
                {
                    Index = index,
                    Id = column.Id,
                    Rows = NormalizeRows(column.Rows),
                })
                .ToList(),
            Transfer = projection.Transfer,
        };
    }

    /// <summary>
    /// Returns a new projection with an empty column inserted at the requested visual index.
    /// </summary>
    /// <param name="projection">The source projection.</param>
    /// <param name="columnIndex">The zero-based insertion index.</param>
    /// <returns>The projection containing the inserted column.</returns>
    public static NodeDragProjection InsertColumn(NodeDragProjection projection, int columnIndex)
    {
        ArgumentNullException.ThrowIfNull(projection);

        var projectedColumns = projection.Columns.ToList();
        var insertIndex = Math.Clamp(columnIndex, 0, projectedColumns.Count);
        projectedColumns.Insert(insertIndex, new NodeDragProjectedColumn
        {
            Index = insertIndex,
            Id = Guid.NewGuid().ToString("N"),
            Rows = [],
        });

        return new NodeDragProjection
        {
            Columns = projectedColumns
                .Select((column, index) => new NodeDragProjectedColumn
                {
                    Index = index,
                    Id = column.Id,
                    Rows = NormalizeRows(column.Rows),
                })
                .ToList(),
            Transfer = projection.Transfer,
        };
    }

    private static bool IsButtonLikeComponent(NodeComponentVM component)
    {
        return component is NodeButtonComponentVM or NodeIconButtonComponentVM;
    }

    private static IReadOnlyList<NodeDragProjectedRow> NormalizeRows(IReadOnlyList<NodeDragProjectedRow> rows)
    {
        return rows
            .Select((row, index) => new NodeDragProjectedRow
            {
                Id = row.Id,
                YOrder = index,
                Components = NormalizeRowComponents(row.Components),
            })
            .ToList();
    }

    private static IReadOnlyList<NodeComponentVM> NormalizeRowComponents(IEnumerable<NodeComponentVM> components)
    {
        var orderedComponents = components.ToList();

        for (var index = 0; index < orderedComponents.Count; index++)
            orderedComponents[index].SetXOrder(IsButtonLikeComponent(orderedComponents[index]) ? index : 0, notifyComponentChanged: false);

        return orderedComponents;
    }
}
