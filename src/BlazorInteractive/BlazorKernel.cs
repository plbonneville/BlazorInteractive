using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;

namespace BlazorInteractive
{
    public class BlazorKernel : Kernel, IKernelCommandHandler<SubmitCode>
    {
        public BlazorKernel() : base("blazor")
        {
        }

        public Task HandleAsync(SubmitCode command, KernelInvocationContext context)
        {
            var componentName = GetComponentName(command.Parent as SubmitCode);

            var markdown = new BlazorMarkdown(command.Code, componentName);
            context.Display(markdown);
            return Task.CompletedTask;
        }

        private static string GetComponentName(SubmitCode command)
        {
            _ = command ?? throw new ArgumentNullException(nameof(command));

            /*
                #!blazor -n HelloWorld
                <h1>hello world</h1>

                or

                #!blazor --name HelloWorld
                <h1>hello world</h1>
            */
            const string pattern = @"#!blazor -n (?<name>\w+)|--name (?<name>\w+)";

            var regex = new Regex(pattern);
            var result = regex.Match(command.Code);
            var componentName = result.Success ? result.Groups["name"].Value : "__Main";

            return componentName;
        }
    }
}