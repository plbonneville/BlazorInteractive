using System;
using System.Linq;
using System.Reflection;

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;

namespace BlazorInteractive
{
    internal static class BlazorMiddleware
    {
        public static void AddBlazor(this Kernel kernel)
        {
            kernel.AddMiddleware(async (command, context, next) =>
            {
                if (command is SubmitCode sc
                    && context.Command is SubmitCode cell
                    && cell.Code.StartsWith("#!blazor"))
                {
                    try
                    {
                        var blazorAssemblyCompiler = new BlazorAssemblyCompiler(sc.Code);

                        var blazorCompilationService = new BlazorCompilationService();
                        await blazorCompilationService.InitializeAsync();
                        blazorAssemblyCompiler.CompilationService = blazorCompilationService;

                        var compileToAssemblyResult = await blazorAssemblyCompiler.CompileAsync();

                        foreach (var item in blazorAssemblyCompiler.Diagnostics)
                        {
                            context.Display(item.Description);
                        }

                        var assembly = Assembly.Load(compileToAssemblyResult.AssemblyBytes);

                        var type = assembly.GetType("BlazorRepl.UserComponents.__Main");

                        using var ctx = new TestContext();                        
                        var method = typeof(TestContext).GetMethods().First(x => x.Name == nameof(TestContext.RenderComponent));
                        var generic = method.MakeGenericMethod(type);
                        var results = generic.Invoke(ctx, new object[] { Array.Empty<ComponentParameter>() });
                        var cut = results as IRenderedComponent<IComponent>;

                        var markup = cut.Markup;

                        context.Display(markup, "text/html");
                    }
                    catch (Exception e)
                    {
                        context.DisplayStandardError(e.ToString());
                    }

                    return;
                }

                await next(command, context);
            });
        }
    }
}