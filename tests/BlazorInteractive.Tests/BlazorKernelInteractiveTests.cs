using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.FSharp;
using Microsoft.DotNet.Interactive.Tests.Utility;
using Xunit;

namespace BlazorInteractive.Tests
{
    public sealed class BlazorKernelInteractiveTests : IDisposable
    {
        private readonly Kernel _kernel;

        public BlazorKernelInteractiveTests()
        {
            _kernel = new CompositeKernel
            {
                new CSharpKernel().UseNugetDirective(),
                new FSharpKernel().UseNugetDirective()
            };

            Task.Run(async () =>
            {
                var extension = new BlazorKernelExtension();
                await extension.OnLoadAsync(_kernel);
            })
            .Wait();

            KernelEvents = _kernel.KernelEvents.ToSubscribedList();
        }

        private SubscribedList<KernelEvent> KernelEvents { get; }

        public void Dispose()
        {
            _kernel?.Dispose();
            KernelEvents?.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task It_is_registered_as_a_directive()
        {
            // Arrange
            using var events = _kernel.KernelEvents.ToSubscribedList();

            // Act
            await _kernel.SubmitCodeAsync("#!blazor-kernel");

            // Assert
            KernelEvents
                .Should()
                .ContainSingle<CommandSucceeded>()
                .Which
                .Command
                .Should()
                .Equals("#!blazor-kernel");
        }

        [Fact]
        public async Task It_interprets_BlazorCode()
        {
            // Arrange
            const string code = @"#!blazor-kernel
<h1>Hello world @name!</h1>

@code {
    string name = ""Alice"";
}";

            using var events = _kernel.KernelEvents.ToSubscribedList();

            // Act
            await _kernel.SubmitCodeAsync(code);

            // Assert
            KernelEvents
                .Should()
                .ContainSingle<DisplayEvent>()
                .Which
                .FormattedValues
                .Should()
                .ContainSingle(v => v.MimeType == "text/html")
                .Which
                .Value
                .Should()
                .Contain("Hello world Alice!");
        }
    }
}