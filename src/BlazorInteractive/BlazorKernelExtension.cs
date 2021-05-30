using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Formatting;

namespace BlazorInteractive
{
    public class BlazorKernelExtension : IKernelExtension, IStaticContentSource
    {
        public string Name => "Blazor";

        public async Task OnLoadAsync(Kernel kernel)
        {
            if (kernel is CompositeKernel compositeKernel)
            {
                compositeKernel.Add(new BlazorKernel());
            }

            kernel.UseBlazor();

            var message = new HtmlString(@"
<details>
    <summary>Compile and render Razor components (.razor) in .NET Interactive Notebooks.</summary>
    <p>This extension adds a new kernel that can render Blazor markdown.</p>

    <pre>
        <code>
#!blazor
<h1>Counter</h1>

<p>
    Current count: @currentCount
</p>

@code {
  int currentCount = 0;
      }</code>
    </pre>

    <p>This extension also adds the compiled component as a type to the interactive workspace.</p>

    <p>Options:</p>
    <ul>
    <li>-n, --name &nbsp;&nbsp;&nbsp;&nbsp;The Razor component's (.razor) type name. The default value is <code>__Main</code></li>
    </ul>
</details>");

            var formattedValue = new FormattedValue(
                HtmlFormatter.MimeType,
                message.ToDisplayString(HtmlFormatter.MimeType));

            var webAssemblyVersion = AppDomain.CurrentDomain.GetAssemblies()
                .First(x => x.GetName().Name == "Microsoft.AspNetCore.Components.WebAssembly")
                .GetName()
                .Version
                .ToString();

            // Load the required NuGet package
            await kernel.SubmitCodeAsync($@"#r ""nuget: Microsoft.AspNetCore.Components.WebAssembly, {webAssemblyVersion}""");

            await kernel.SendAsync(new DisplayValue(formattedValue, Guid.NewGuid().ToString()));
        }
    }
}