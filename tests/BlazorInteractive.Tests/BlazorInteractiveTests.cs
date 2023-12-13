using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.FSharp;
using Microsoft.DotNet.Interactive.Tests.Utility;
using Xunit;

namespace BlazorInteractive.Tests;

public sealed class BlazorInteractiveTests : IDisposable
{
    private readonly Kernel _kernel;

    public BlazorInteractiveTests()
    {
        // Prevent SynchronizationContext-induced deadlocks given the following sync-over-async code.
        ExecutionContext.SuppressFlow();

        try
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
        finally
        {
            ExecutionContext.RestoreFlow();
        }
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
        await _kernel.SubmitCodeAsync("#!blazor");

        // Assert
        KernelEvents
            .Should()
            .ContainSingle<CommandSucceeded>()
            .Which.Command.Should().BeOfType<SubmitCode>()
            .Which.Code.Should().Be("#!blazor");
    }

    [Fact]
    public async Task It_formats_BlazorCode_as_html()
    {
        // Arrange
        using var events = _kernel.KernelEvents.ToSubscribedList();

        // Act
        await _kernel.SubmitCodeAsync(
            """
                #!blazor
                <h1>hello world</h1>
                """);

        // Assert
        events
            .Should()
            .ContainSingle<DisplayEvent>()
            .Which
            .FormattedValues
            .Should()
            .ContainSingle(v => v.MimeType == "text/html");
    }

    [Fact]
    public async Task It_interprets_BlazorCode()
    {
        // Arrange
        const string code =
            """
                #!blazor
                <h1>Hello world @name!</h1>

                @code {
                    string name = "Alice";
                }
                """;

        using var events = _kernel.KernelEvents.ToSubscribedList();

        // Act
        await _kernel.SubmitCodeAsync(code);

        // Assert
        events
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
    public async Task It_interpret_BlazorCode_with_a_method()
    {
        // Arrange
        const string code =
            """
                #!blazor
                <h1>Counter</h1>

                <p>
                    Current count: @currentCount
                </p>

                <button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

                @code {
                    int currentCount = 0;

                    void IncrementCount()
                    {
                        currentCount++;
                    }
                }
                """;

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
        await _kernel.SubmitCodeAsync(
            """
                #!blazor
                <h1>hello world</h1>
                """);

        await Task.Delay(1000);

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

    [Fact]
    public async Task It_can_reference_a_component_defined_in_previous_compilation()
    {
        // Arrange
        await _kernel.SubmitCodeAsync(
            """
                #!blazor
                <h1>hello world</h1>
                """);

        using var events = _kernel.KernelEvents.ToSubscribedList();

        // Act
        await _kernel.SubmitCodeAsync(
            """
                #!csharp
                typeof(__Main).Name
                """);

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
        await _kernel.SubmitCodeAsync(
            """
                #!blazor
                <h1>hello world</h1>
                """);

        using var events = _kernel.KernelEvents.ToSubscribedList();

        // Act
        await _kernel.SubmitCodeAsync(
            """
                #!csharp
                var component = new __Main();
                """);

        // Assert
        events
            .Should()
            .ContainSingle<CommandSucceeded>()
            .Which
            .Command.As<SubmitCode>()
            .Code
            .Should()
            .Be(
                """
                    #!csharp
                    var component = new __Main();
                    """);
    }

    [Fact]
    public async Task It_can_reference_a_named_component_defined_in_previous_compilation()
    {
        // Arrange
        await _kernel.SubmitCodeAsync(
            """
                #!blazor --name HelloWorld
                <h1>hello world</h1>
                """);

        using var events = _kernel.KernelEvents.ToSubscribedList();

        // Act
        await _kernel.SubmitCodeAsync(
            """
                #!csharp
                typeof(HelloWorld).Name
                """);

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
        await _kernel.SubmitCodeAsync(
            """
                #!blazor -n HelloWorld
                <h1>hello world</h1>
                """);

        using var events = _kernel.KernelEvents.ToSubscribedList();

        // Act
        await _kernel.SubmitCodeAsync(
            """
                #!csharp
                var component = new HelloWorld();
                """);

        // Assert
        events
            .Should()
            .ContainSingle<CommandSucceeded>()
            .Which
            .Command.As<SubmitCode>()
            .Code
            .Should()
            .Be(
                """
                    #!csharp
                    var component = new HelloWorld();
                    """);
    }
}