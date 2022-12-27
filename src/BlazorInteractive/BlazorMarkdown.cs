namespace BlazorInteractive;

internal sealed record BlazorMarkdown(string Value, string ComponentName)
{ 
    public override string ToString() => Value;
}