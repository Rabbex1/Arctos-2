using System.Text.Json;

namespace Arctos.Nodes.Models;

/// <summary>
/// Validates and repairs persisted internal component UIDs in node artifact files.
/// </summary>
public static class NodeComponentUidValidator
{
    /// <summary>
    /// Validates the raw file JSON for missing or duplicate component UIDs.
    /// </summary>
    /// <param name="json">The raw file JSON.</param>
    /// <returns>The validation result.</returns>
    public static NodeComponentUidValidationResult ValidateJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using var document = JsonDocument.Parse(json);
        var seenUids = new HashSet<string>(StringComparer.Ordinal);
        var missingUidCount = 0;
        var duplicateUidCount = 0;
        ValidateElement(document.RootElement, seenUids, ref missingUidCount, ref duplicateUidCount);

        return new NodeComponentUidValidationResult
        {
            MissingUidCount = missingUidCount,
            DuplicateUidCount = duplicateUidCount,
        };
    }

    /// <summary>
    /// Repairs missing or duplicate component UIDs inside a map file.
    /// </summary>
    /// <param name="file">The file to repair.</param>
    /// <returns>The issues that were found and repaired.</returns>
    public static NodeComponentUidValidationResult AutoFix(NodeMapFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        return AutoFixDefinitions(file.LocalLibrary?.Definitions ?? []);
    }

    /// <summary>
    /// Repairs missing or duplicate component UIDs inside a library file.
    /// </summary>
    /// <param name="file">The file to repair.</param>
    /// <returns>The issues that were found and repaired.</returns>
    public static NodeComponentUidValidationResult AutoFix(NodeLibraryFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        return AutoFixDefinitions(file.Library.Definitions);
    }

    private static void ValidateElement(JsonElement element, ISet<string> seenUids, ref int missingUidCount, ref int duplicateUidCount)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                if (element.TryGetProperty("$type", out _))
                {
                    if (!element.TryGetProperty("Uid", out var uidProperty)
                        || uidProperty.ValueKind == JsonValueKind.Null
                        || string.IsNullOrWhiteSpace(uidProperty.GetString()))
                    {
                        missingUidCount++;
                    }
                    else if (!seenUids.Add(uidProperty.GetString()!))
                    {
                        duplicateUidCount++;
                    }
                }

                foreach (var property in element.EnumerateObject())
                    ValidateElement(property.Value, seenUids, ref missingUidCount, ref duplicateUidCount);

                break;
            }

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    ValidateElement(item, seenUids, ref missingUidCount, ref duplicateUidCount);
                break;
        }
    }

    private static NodeComponentUidValidationResult AutoFixDefinitions(IEnumerable<NodeDefinition> definitions)
    {
        var seenUids = new HashSet<string>(StringComparer.Ordinal);
        var missingUidCount = 0;
        var duplicateUidCount = 0;

        foreach (var component in definitions.SelectMany(definition => definition.Components))
        {
            if (string.IsNullOrWhiteSpace(component.Uid))
            {
                missingUidCount++;
                component.Uid = CreateUid(seenUids);
                continue;
            }

            if (seenUids.Add(component.Uid))
                continue;

            duplicateUidCount++;
            component.Uid = CreateUid(seenUids);
        }

        return new NodeComponentUidValidationResult
        {
            MissingUidCount = missingUidCount,
            DuplicateUidCount = duplicateUidCount,
        };
    }

    private static string CreateUid(ISet<string> seenUids)
    {
        string uid;
        do
        {
            uid = Guid.NewGuid().ToString("N");
        }
        while (!seenUids.Add(uid));

        return uid;
    }
}
