using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;

namespace BlazorInteractive
{
    public class BlazorKernel :
        Kernel,
        IKernelCommandHandler<SubmitCode>,
        IKernelCommandHandler<SubmitBlazorCode>

    {
        public BlazorKernel() : base("blazor-kernel")
        {
        }

        public Task HandleAsync(SubmitCode command, KernelInvocationContext context)
            => HandleAsync(SubmitBlazorCode.From(command), context);

        public Task HandleAsync(SubmitBlazorCode command, KernelInvocationContext context)
        {
            var markdown = new BlazorMarkdown(command.Code, command.ComponentName);
            context.Display(markdown);
            return Task.CompletedTask;
        }
    }
}