using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Formatting;

namespace BlazorInteractive;

public sealed class BlazorKernelExtension : IKernelExtension, IStaticContentSource
{
    public string Name => "Blazor";

    public async Task OnLoadAsync(Kernel kernel)
    {
        if (kernel is not CompositeKernel compositeKernel)
        {
            throw new InvalidOperationException("The Blazor kernel can only be added into a CompositeKernel.");
        }

        // Add a BlazorKernel as a child kernel to the CompositeKernel
        compositeKernel.Add(new BlazorKernel());

        await compositeKernel
            .UseBlazor()
            .LoadRequiredAssemblies();

        var message = new HtmlString(
            """
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
            </details>
            """);

        var formattedValue = new FormattedValue(
            HtmlFormatter.MimeType,
            message.ToDisplayString(HtmlFormatter.MimeType));

        await compositeKernel.SendAsync(new DisplayValue(formattedValue, Guid.NewGuid().ToString()));
    }
}