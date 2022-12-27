using System.Text.Encodings.Web;

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Formatting;

namespace BlazorInteractive;

internal static class BlazorExtensions
{
    /// <summary>
    /// Registers the type <see cref="BlazorMarkdown"/> as a formatter.
    /// </summary>
    /// <param name="kernel">A <see cref="Kernel"/>.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static T UseBlazor<T>(this T kernel) where T : Kernel
    {
        Formatter.Register<BlazorMarkdown>((markdown, writer) =>
        {
            // Formatter.Register() doesn't support async formatters yet.
            // Prevent SynchronizationContext-induced deadlocks given the following sync-over-async code.
            ExecutionContext.SuppressFlow();

            try
            {
                Task.Run(async () =>
                {
                    var (assemblyBytes, code) = await GenerateAssemblyAndCodeFile(markdown);

                    var html = GenerateHtml(assemblyBytes, markdown.ComponentName);
                    html.WriteTo(writer, HtmlEncoder.Default);

                    AddComponentTypeToInteractiveWorkspace(kernel, code);
                })
                .Wait();
            }
            finally
            {
                ExecutionContext.RestoreFlow();
            }
        }, HtmlFormatter.MimeType);

        return kernel;
    }

    private static async Task<(byte[] assemblyBytes, string code)> GenerateAssemblyAndCodeFile(BlazorMarkdown markdown)
    {
        var blazorCompilationService = new BlazorCompilationService();
        await blazorCompilationService.InitializeAsync();

        var blazorAssemblyCompiler = new BlazorAssemblyCompiler(markdown.ToString(), markdown.ComponentName)
        {
            CompilationService = blazorCompilationService
        };

        var (compileToAssemblyResult, code) = await blazorAssemblyCompiler.CompileAsync();

        return (compileToAssemblyResult.AssemblyBytes, code);
    }

    private static void AddComponentTypeToInteractiveWorkspace(Kernel kernel, string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetCompilationUnitRoot();
        var codeWithoutNamespace = root.RemoveNamespace();

        var csharpKernel = kernel.FindKernelByName("csharp") as CSharpKernel;

        csharpKernel.DeferCommand(new SubmitCode(codeWithoutNamespace));
    }

    private static IHtmlContent GenerateHtml(byte[] assemblyBytes, string componentName)
    {
        var assembly = AppDomain.CurrentDomain.Load(assemblyBytes);

        var type = assembly.GetType($"BlazorRepl.UserComponents.{componentName}");

        using var ctx = new TestContext();
        var method = typeof(TestContext).GetMethods().First(x => x.Name == nameof(TestContext.RenderComponent));
        var generic = method.MakeGenericMethod(type);
        var results = generic.Invoke(ctx, new object[] { Array.Empty<ComponentParameter>() });
        var cut = results as IRenderedComponent<IComponent>;

        var markup = cut.Markup;

        var id = "blazorExtension" + Guid.NewGuid().ToString("N");

        var html = $"""
                   <div id="{id}">{markup}</div>
                   """.ToHtmlContent();

        return html;
    }
}