using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;

namespace BlazorInteractive
{
    public static class BlazorExtensions
    {
        public static T UseBlazor<T>(this T kernel) where T : Kernel
        {
            Formatter.Register<BlazorMarkdown>((markdown, writer) =>
            {
                IHtmlContent html = new HtmlString("");

                Task.Run(async () =>
                {
                    var (assemblyBytes, code) = await GenerateAssemblyAndCodeFile(markdown);

                    html = GenerateHtml(assemblyBytes, markdown.ComponentName);

                    await AddComponentTypeToInteractiveWorkspace(kernel, code);
                })
                .Wait();

                html.WriteTo(writer, HtmlEncoder.Default);

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

        private static async Task AddComponentTypeToInteractiveWorkspace(Kernel kernel, string code)
        {
            // TODO: Replace with regex?
            // START
            // (?<namespace_start>namespace BlazorRepl.UserComponents\r?\n\{\r?\n)
            // END
            // (?<namespace_end>^\})
            var c = code
                .Split('\n')
                .Where(x => !x.StartsWith("namespace BlazorRepl.UserComponents"))
                .Where(x => !x.StartsWith("{"))
                .Where(x => !x.StartsWith("}"))
                .Aggregate(new StringBuilder(), (sb, x) => sb.Append(x))
                .ToString();

            await kernel.SubmitCodeAsync(c);
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

            var html = Html.ToHtmlContent($"<div id=\"{id}\">{markup}</div>");

            return html;
        }
    }
}