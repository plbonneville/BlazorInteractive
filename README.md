# .NET Interactive Notebooks Blazor Extension

[![NuGet version (BlazorInteractive)](https://img.shields.io/nuget/v/BlazorInteractive.svg)](https://www.nuget.org/packages/BlazorInteractive/)

Compile and render Razor components (.razor) in .NET Interactive Notebooks.

## 🎯 Table of content

- [Get started](#-get-started)
- [Usage](#-usage)
- [How to compile this project](#-how-to-compile-this-project)

## ❤️ Built With

- [Blazor REPL](https://github.com/BlazorRepl/BlazorRepl)
- [.NET Interactive ](https://github.com/dotnet/interactive)

## ⚙️ Get started

To get started with Blazor in .NET Interactive Notebooks, first install the `BlazorInteractive` NuGet package.

In a new `C# (.NET Interactive)` cell enter and run the following:

```
#r "nuget: BlazorInteractive, 1.0.9-alpha.1"
```

## ⚡️ Usage

Using the `#!blazor` magic command your code cell will be parsed by a Blazor engine and the results displayed using the `"txt/html"` mime type.

```razor
#!blazor
<h1>Hello @name</h1>

@code {
    string name = "Alice";
}
```

![hello world](img/example1.jpg)

## 🌱 How to compile this project

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
