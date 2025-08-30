// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Cake.Generator.Core.Tests.Unit;

/// <summary>
/// Tests for the main <see cref="CakeGenerator"/> functionality.
/// </summary>
public sealed class CakeGeneratorTests
{
    [Fact]
    public async Task RunGenerators()
    {
        // Given
        var source = CakeGeneratorTestsBase.CommonSources.Program;
        var compilation = CakeGeneratorTestsBase.CreateCakeCompilation(source);

        // When
        var driver = CSharpGeneratorDriver.Create(new CakeGenerator());
        var result = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // Then
        await Verify(result.GetRunResult());
    }

    [Fact]
    public async Task RunGenerators_WithModules()
    {
        // Given
        var source = CakeGeneratorTestsBase.CommonSources.Program;

        // Add the BuildSystems.Module assembly reference to test module detection
        var buildSystemsModuleLocation = typeof(GitHubActions.Module.GitHubActionsModule).Assembly.Location;
        var moduleReference = MetadataReference.CreateFromFile(buildSystemsModuleLocation);

        var compilation = CakeGeneratorTestsBase.CreateCompilation(source, moduleReference);

        // When
        var driver = CSharpGeneratorDriver.Create(new CakeGenerator());
        var result = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // Then
        await Verify(result.GetRunResult());
    }

    [Fact]
    public async Task RunGenerators_WithMainMethods_GeneratesMainMethodCalls()
    {
        // Given
        var source = """
        using System;
        using Cake.Core.Scripting;

        public static partial class Program
        {
            private static void Main_Initialize()
            {
                // Initialization logic
            }

            private static void Main_Setup()
            {
                // Setup logic
            }

            private static void NotMain()
            {
                // Should not be picked up
            }
        }
        """;

        var compilation = CakeGeneratorTestsBase.CreateCakeCompilation(source);

        // When
        var driver = CSharpGeneratorDriver.Create(new CakeGenerator());
        var result = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // Then
        var runResult = result.GetRunResult();
        var generatedCode = string.Join("\n", runResult.Results.SelectMany(r => r.GeneratedSources).Select(s => s.SourceText.ToString()));

        await Verify(runResult);
    }
}
