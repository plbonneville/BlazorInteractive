using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BlazorRepl.Core;

namespace BlazorInteractive
{
    internal class BlazorAssemblyCompiler
    {
        public BlazorAssemblyCompiler(string code, string componentName)
        {
            CodeFiles.Add($"{componentName}.razor", new CodeFile { Content = code, Path = $"{componentName}.razor" });
        }

        public BlazorCompilationService CompilationService { get; init; }

        public IReadOnlyCollection<CompilationDiagnostic> Diagnostics { get; private set; } = Array.Empty<CompilationDiagnostic>();

        private IDictionary<string, CodeFile> CodeFiles { get; } = new Dictionary<string, CodeFile>();

        public async Task<(CompileToAssemblyResult, string)> CompileAsync()
        {
            string code = null;
            CompileToAssemblyResult compilationResult = null;

            try
            {
                (compilationResult, code) = await CompilationService.CompileToAssemblyAsync(
                    CodeFiles.Values,
                    _ => Task.CompletedTask);

                Diagnostics = compilationResult.Diagnostics.OrderByDescending(x => x.Severity).ThenBy(x => x.Code).ToList();
            }
            catch (Exception e)
            {
                throw new Exception("Error while compiling the code.", e);
            }

            return (compilationResult, code);
        }
    }
}
