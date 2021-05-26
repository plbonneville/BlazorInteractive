# .NET Interactive Notebooks Blazor Extension

[![NuGet version (BlazorInteractive)](https://img.shields.io/nuget/v/BlazorInteractive.svg)](https://www.nuget.org/packages/BlazorInteractive/)

Compile and render Razor components (.razor) in .NET Interactive Notebooks.

## Get started

To get started with Blazor in .NET Interactive Notebooks, first install the `BlazorInteractive` NuGet package. In a new `C# (.NET Interactive)` cell enter and run the following:

```
#r "nuget: BlazorInteractive, 1.0.3-alpha.1"
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

## How to compile this project

Since this project requires a git submodule, you'll need to initialize and update the [Blazor REPL](https://github.com/BlazorRepl/BlazorRepl) submodule.

### On the first `git clone`:

```
git clone --recurse-submodules -j8 https://github.com/plbonneville/BlazorInteractive.git
```

### If you already have the repository cloned, run:

```
git submodule init
git submodule update
```