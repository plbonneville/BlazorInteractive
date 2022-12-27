using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;

namespace BlazorInteractive;

public class BlazorKernel : Kernel, IKernelCommandHandler<SubmitCode>
{
    private const string DefaultComponentName = "__Main";

    public BlazorKernel() : base("blazor")
    {
    }

    private string ComponentName { get; set; } = DefaultComponentName;

    public Task HandleAsync(SubmitCode command, KernelInvocationContext context)
    {
        var markdown = new BlazorMarkdown(command.Code, ComponentName);
        context.Display(markdown);
        return Task.CompletedTask;
    }

    public override ChooseKernelDirective ChooseKernelDirective
    {
        get
        {
            var nameOption = new Option<string>(
                new[] { "-n", "--name" },
                "The Razor component's (.razor) type name. The default value is '__Main'")
            {
                IsRequired = false
            };

            return new BlazorKernelOptionsDirective(this)
                {
                    nameOption,
                    //fromFileOption,
                };
        }
    }
        

    private class BlazorKernelOptionsDirective : ChooseKernelDirective
    {
        private readonly BlazorKernel _kernel;

        public BlazorKernelOptionsDirective(BlazorKernel kernel, string description = "Compile and render Razor components (.razor).") : base(kernel, description)
        {
            _kernel = kernel;
        }

        protected override async Task Handle(KernelInvocationContext kernelInvocationContext, InvocationContext commandLineInvocationContext)
        {
            var options = BlazorDirectiveOptions.Create(commandLineInvocationContext.ParseResult);

            //if (options.FromFile is { } fromFile)
            //{
            //    var value = await File.ReadAllTextAsync(fromFile.FullName);
            //    await _kernel.StoreValueAsync(value, options, kernelInvocationContext);
            //}

            if (options.Name is { })
            {
                _kernel.ComponentName = options.Name;
            }

            await base.Handle(kernelInvocationContext, commandLineInvocationContext);
        }
    }

    private class BlazorDirectiveOptions
    {
        private static readonly ModelBinder<BlazorDirectiveOptions> ModelBinder = new();

        //public static BlazorDirectiveOptions Create(ParseResult parseResult) =>
        //    ModelBinder.CreateInstance(new BindingContext(parseResult)) as BlazorDirectiveOptions;

        public static BlazorDirectiveOptions Create(ParseResult parseResult)
        {
            var invocationContext = new InvocationContext(parseResult);
            var bindingContext = invocationContext.BindingContext;

            return ModelBinder.CreateInstance(bindingContext) as BlazorDirectiveOptions;
        }

        public string Name { get; set; }

        //public FileInfo FromFile { get; set; }
    }
}
