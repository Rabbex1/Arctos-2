namespace Arctos.Forms;

public readonly record struct InputValidationResult(IReadOnlyList<string> Errors)
{
    public static InputValidationResult Success { get; } = new(Array.Empty<string>());

    public bool IsValid => Errors.Count == 0;

    public string? FirstError => Errors.FirstOrDefault();

    public static InputValidationResult Invalid(string error)
    {
        return new InputValidationResult([error]);
    }

    public static InputValidationResult Invalid(IEnumerable<string> errors)
    {
        var materializedErrors = errors
            .Where(error => !string.IsNullOrWhiteSpace(error))
            .ToArray();

        return materializedErrors.Length == 0
            ? Success
            : new InputValidationResult(materializedErrors);
    }
}
