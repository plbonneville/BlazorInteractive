using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;

namespace BlazorInteractive
{
    internal static class KernelExtensions
    {
        internal static void LoadRequiredAssemblies(this Kernel kernel)
            => LoadRequiredAssemblies(kernel.FindKernel("csharp") as CSharpKernel);

        private static void LoadRequiredAssemblies(this CSharpKernel csharpKernel)
        {
            var referenceAssemblyLocations = new string[]
                {
                    typeof(ComponentBase).Assembly.Location,   // Microsoft.AspNetCore.Components
                    typeof(WebAssemblyHost).Assembly.Location, // Microsoft.AspNetCore.Components.WebAssembly                    
                    typeof(DataType).Assembly.Location,        // System.ComponentModel.DataAnnotations
                    typeof(JsonContent).Assembly.Location      // System.Net.Http.Json
                }
                .Select(x => x.Replace("\\", "/"));

            foreach (var location in referenceAssemblyLocations)
            {
                csharpKernel.DeferCommand(new SubmitCode($@"#r ""{location}"""));
            }            
        }
    }
}