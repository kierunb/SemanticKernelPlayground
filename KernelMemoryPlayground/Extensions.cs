namespace SemanticKernelPlayground;
public static class Extensions
{
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? string.Concat(value.AsSpan(0, maxLength), truncationSuffix)
            : value;
    }
}
