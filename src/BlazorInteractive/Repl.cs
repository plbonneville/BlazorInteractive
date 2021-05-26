using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BlazorRepl.Core;

namespace BlazorInteractive
{
    public class BlazorAssemblyCompiler
    {
        private const string MainComponentCodePrefix = "@page \"/__main\"\n";

        public BlazorAssemblyCompiler(string code)
        {
            CodeFiles.Add("__Main.razor", new CodeFile() { Content = code, Path = "__Main.razor" });
        }

        //[Inject]
        public BlazorCompilationService CompilationService { get; set; }// = new BlazorCompilationService();

        public IReadOnlyCollection<CompilationDiagnostic> Diagnostics { get; set; } = Array.Empty<CompilationDiagnostic>();

        private bool AreDiagnosticsShown { get; set; }

        private string LoaderText { get; set; }

        private bool ShowLoader { get; set; }

        private IDictionary<string, CodeFile> CodeFiles { get; } = new Dictionary<string, CodeFile>();

        public async Task<CompileToAssemblyResult> CompileAsync()
        {
            this.ShowLoader = true;
            this.LoaderText = "Processing";

            await Task.Delay(1); // Ensure rendering has time to be called

            //if (this.PackagesToRestore.Any())
            //{
            //    await this.PackageManagerComponent.RestorePackagesAsync();
            //}

            CompileToAssemblyResult compilationResult = null;
            CodeFile mainComponent = null;
            string originalMainComponentContent = null;
            try
            {
                ////////this.UpdateActiveCodeFileContent();

                // Add the necessary main component code prefix and store the original content so we can revert right after compilation.
                if (this.CodeFiles.TryGetValue(CoreConstants.MainComponentFilePath, out mainComponent))
                {
                    originalMainComponentContent = mainComponent.Content;
                    mainComponent.Content = MainComponentCodePrefix + originalMainComponentContent;
                }

                compilationResult = await this.CompilationService.CompileToAssemblyAsync(
                    this.CodeFiles.Values,
                    this.UpdateLoaderTextAsync);

                this.Diagnostics = compilationResult.Diagnostics.OrderByDescending(x => x.Severity).ThenBy(x => x.Code).ToList();
                this.AreDiagnosticsShown = true;
            }
            catch (Exception)
            {
                ////this.PageNotificationsComponent.AddNotification(NotificationType.Error, content: "Error while compiling the code.");
            }
            finally
            {
                if (mainComponent != null)
                {
                    mainComponent.Content = originalMainComponentContent;
                }

                this.ShowLoader = false;
            }

            if (compilationResult?.AssemblyBytes?.Length > 0)
            {
                ////// Make sure the DLL is updated before reloading the user page
                ////await this.JsRuntime.InvokeVoidAsync("App.CodeExecution.updateUserComponentsDll", compilationResult.AssemblyBytes);

                ////var userPagePath = this.InstalledPackagesCount > 0 || this.StaticAssetsCount > 0
                ////    ? $"{MainUserPagePath}#{this.SessionId}"
                ////    : MainUserPagePath;

                ////// TODO: Add error page in iframe
                ////this.JsRuntime.InvokeVoid("App.reloadIFrame", "user-page-window", userPagePath);
            }

            return compilationResult;
        }

        private Task UpdateLoaderTextAsync(string loaderText)
        {
            this.LoaderText = loaderText;

            //this.StateHasChanged();
            return Task.Delay(1); // Ensure rendering has time to be called
        }
    }
}
