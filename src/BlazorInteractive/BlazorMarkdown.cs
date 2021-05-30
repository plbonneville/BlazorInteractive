namespace BlazorInteractive
{
    public class BlazorMarkdown
    {
        public BlazorMarkdown(string value, string componentName)
        {
            Value = value;
            ComponentName = componentName;
        }

        public string Value { get; }
        public string ComponentName { get; }

        public override string ToString() => Value;
    }
}