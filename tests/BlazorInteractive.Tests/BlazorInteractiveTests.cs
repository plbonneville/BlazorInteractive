using System;
using System.Threading.Tasks;

using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Tests.Utility;
using Xunit;

namespace BlazorInteractive.Tests
{
    public class BlazorInteractiveTests : IDisposable
    {
        private readonly Kernel _kernel;

        public BlazorInteractiveTests()
        {
            _kernel = new CompositeKernel
            {
                new CSharpKernel()
            };

            Task.Run(() => new BlazorKernelExtension().OnLoadAsync(_kernel))
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
            using var events = _kernel.KernelEvents.ToSubscribedList();

            await _kernel.SubmitCodeAsync("#!blazor");

            KernelEvents
                .Should()
                .ContainSingle<CommandSucceeded>()
                .Which
                .Command
                .Should()
                .Equals("#!blazor");
        }

        [Fact]
        public async Task It_formats_BlazorCode()
        {
            using var events = _kernel.KernelEvents.ToSubscribedList();

            await _kernel.SubmitCodeAsync(@"#!blazor
<h1>hello world</h1>");

            KernelEvents
                .Should()
                .ContainSingle<DisplayEvent>()
                .Which
                .FormattedValues
                .Should()
                .ContainSingle(v => v.MimeType == "text/html");
        }

        [Fact]
        public async Task It_can_interprets_BlazorCode()
        {
            const string code = @"#!blazor
<h1>Hello world @name!</h1>

@code {
    string name = ""Alice"";
}";

            using var events = _kernel.KernelEvents.ToSubscribedList();
            await _kernel.SubmitCodeAsync(code);

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

        [Fact]
        public async Task It_can_interprets_BlazorCode_with_method()
        {
            const string code = @"#!blazor
<h1>Counter</h1>

<p>
    Current count: @currentCount
</p>

<button class=""btn btn-primary"" @onclick=""IncrementCount"">Click me</button>

@code {
                int currentCount = 0;

                void IncrementCount()
                {
                    currentCount++;
                }
            }
            ";

            using var events = _kernel.KernelEvents.ToSubscribedList();
            await _kernel.SubmitCodeAsync(code);

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
                .Contain("Current count: 0")
                .And
                .Contain("Click me");
        }

        [Fact]
        public async Task It_renders_html_and_not_html_encoded_html()
        {
            // Arrange
            using var events = _kernel.KernelEvents.ToSubscribedList();

            // Act
            await _kernel.SubmitCodeAsync(@"#!blazor
            <h1>hello world</h1>");

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
                .Contain("<h1>hello world</h1>");
        }
    }
}