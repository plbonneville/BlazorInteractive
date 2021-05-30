using System;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.FSharp;
using Microsoft.DotNet.Interactive.Tests.Utility;
using Xunit;

namespace BlazorInteractive.IntegrationTests
{
    public sealed class BlazorInteractiveTests : IDisposable
    {
        private readonly Kernel _kernel;

        public BlazorInteractiveTests()
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
        public async Task It_can_reference_a_component_defined_in_previous_compilation()
        {
            // Arrange
            await _kernel.SubmitCodeAsync(@"#!blazor
                <h1>hello world</h1>");

            using var events = _kernel.KernelEvents.ToSubscribedList();

            // Act
            await _kernel.SubmitCodeAsync(@"typeof(__Main).Name");

            // Assert
            events
                .Should()
                .ContainSingle<DisplayEvent>()
                .Which
                .FormattedValues
                .Should()
                .ContainSingle(v => v.MimeType == "text/plain")
                .Which
                .Value
                .Should()
                .Be("__Main");
        }

        [Fact]
        public async Task It_can_instantiate_a_component_defined_in_previous_compilation()
        {
            // Arrange
            await _kernel.SubmitCodeAsync(@"#!blazor
                <h1>hello world</h1>");

            using var events = _kernel.KernelEvents.ToSubscribedList();

            // Act
            await _kernel.SubmitCodeAsync(@"var component = new __Main();");

            // Assert
            events
                .Should()
                .ContainSingle<CommandSucceeded>()
                .Which
                .Command.As<SubmitCode>()
                .Code
                .Should()
                .Be("var component = new __Main();");
        }

        [Fact]
        public async Task It_can_reference_a_named_component_defined_in_previous_compilation()
        {
            // Arrange
            await _kernel.SubmitCodeAsync(@"#!blazor --name HelloWorld
                <h1>hello world</h1>");

            using var events = _kernel.KernelEvents.ToSubscribedList();

            // Act
            await _kernel.SubmitCodeAsync(@"typeof(HelloWorld).Name");

            // Assert
            events
                .Should()
                .ContainSingle<DisplayEvent>()
                .Which
                .FormattedValues
                .Should()
                .ContainSingle(v => v.MimeType == "text/plain")
                .Which
                .Value
                .Should()
                .Be("HelloWorld");
        }

        [Fact]
        public async Task It_can_instantiate_a_named_component_defined_in_previous_compilation()
        {
            // Arrange
            await _kernel.SubmitCodeAsync(@"#!blazor -n HelloWorld
                <h1>hello world</h1>");

            using var events = _kernel.KernelEvents.ToSubscribedList();

            // Act
            await _kernel.SubmitCodeAsync(@"var component = new HelloWorld();");

            // Assert
            events
                .Should()
                .ContainSingle<CommandSucceeded>()
                .Which
                .Command.As<SubmitCode>()
                .Code
                .Should()
                .Be("var component = new HelloWorld();");
        }
    }
}