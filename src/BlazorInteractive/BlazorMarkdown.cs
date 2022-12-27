namespace BlazorInteractive;

/// <summary>
/// Type used to register a markdown formatter for Blazor.
/// </summary>
internal sealed record BlazorMarkdown(string Value, string ComponentName)
{
    public override string ToString() => Value;
}