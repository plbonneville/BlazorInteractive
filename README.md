# .NET Interactive Nootbooks Blazor Extension

To get started with Blazor in .NET Interactive Notebooks, first install the `BlazorInteractive` NuGet package. In a new `C# (.NET Interactive)` cell enter and run the following:

```
#r "nuget: BlazorInteractive, 1.0.0"
```

Using the `#!Blazor` magic command your code cell will be parsed by a Blazor engine and the results displayed using the `"txt/html"` mime type.

```razor
#!blazor
<h1>Counter</h1>

<p>
    Current count: @currentCount
</p>

<button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

@code {
  int currentCount = 0;

  void IncrementCount()
  {
    currentCount++;
  }
}
```
