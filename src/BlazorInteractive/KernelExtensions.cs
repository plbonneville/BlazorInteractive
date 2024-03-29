﻿using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;

namespace BlazorInteractive;

internal static class KernelExtensions
{
    private static readonly Assembly[] _references =
    {
        typeof(ComponentBase).Assembly,   // Microsoft.AspNetCore.Components
        typeof(WebAssemblyHost).Assembly, // Microsoft.AspNetCore.Components.WebAssembly
        typeof(DataType).Assembly,        // System.ComponentModel.DataAnnotations
        typeof(JsonContent).Assembly      // System.Net.Http.Json
    };

    internal static Task LoadRequiredAssemblies(this CompositeKernel kernel)
        => LoadRequiredAssemblies(kernel.FindKernelByName("csharp") as CSharpKernel);

    private static async Task LoadRequiredAssemblies(this CSharpKernel csharpKernel)
    {
        var rDirectives = string.Join(Environment.NewLine, _references.Select(a => $"#r \"{a.Location}\""));

        await csharpKernel.SendAsync(new SubmitCode($"{rDirectives}"), CancellationToken.None).ConfigureAwait(false);
        //await csharpKernel.SendAsync(new SubmitCode($"{rDirectives}{Environment.NewLine}{usings}"), CancellationToken.None).ConfigureAwait(false);
    }
}