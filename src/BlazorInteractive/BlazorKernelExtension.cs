using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

using Microsoft.DotNet.Interactive;

namespace BlazorInteractive
{
    public class BlazorKernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(Kernel kernel)
        {
            var razorCommand = new Command("#!blazor", "Renders the code block as Blazor markup.")
            {
                Handler = CommandHandler.Create<KernelInvocationContext>(context =>
                {
                    context.HandlingKernel.AddBlazor();
                })
            };

            kernel.AddDirective(razorCommand);

            return Task.CompletedTask;
        }
    }
}
