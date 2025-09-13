#nullable enable
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cake.Generator.Core.BenchmarkSuite;

[MemoryDiagnoser]
public class CakeGenerator_ScanBenchmarks
{
    // OCE: Object Creation Expression for Roslyn Compilation and Symbols
    private Compilation _compilation = null!;
    private INamedTypeSymbol? _cakeMethodAliasAttributeSymbol;
    private INamedTypeSymbol? _cakePropertyAliasAttributeSymbol;
    private INamedTypeSymbol? _cakeNamespaceImportAttributeSymbol;
    private INamedTypeSymbol _iCakeContextSymbol = null!;
    private INamedTypeSymbol _cakeModuleAttributeSymbol = null!;
    private INamedTypeSymbol _iCakeModuleSymbol = null!;
    private INamedTypeSymbol? _obsoleteAttributeSymbol;
    [GlobalSetup]
    public void Setup()
    {
        // Define minimal Cake and system types in the syntax tree
        var code = @"
using System;

[AttributeUsage(AttributeTargets.Method)]
public class CakeMethodAliasAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)]
public class CakePropertyAliasAttribute : Attribute {
    public bool Cache { get; set; }
}
[AttributeUsage(AttributeTargets.Method)]
public class CakeNamespaceImportAttribute : Attribute {
    public CakeNamespaceImportAttribute(string ns) { }
}
public interface ICakeContext { }
[AttributeUsage(AttributeTargets.Assembly)]
public class CakeModuleAttribute : Attribute { }
public interface ICakeModule { }
public class Dummy : ICakeContext, ICakeModule { }
";
        // OCE: Create Roslyn syntax tree and compilation for benchmarks
        var tree = CSharpSyntaxTree.ParseText(code);
        var refs = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ImmutableArray<>).Assembly.Location)
        };
        _compilation = CSharpCompilation.Create("DummyAssembly", new[] { tree }, refs);
        _cakeMethodAliasAttributeSymbol = _compilation.GetTypeByMetadataName("CakeMethodAliasAttribute");
        _cakePropertyAliasAttributeSymbol = _compilation.GetTypeByMetadataName("CakePropertyAliasAttribute");
        _cakeNamespaceImportAttributeSymbol = _compilation.GetTypeByMetadataName("CakeNamespaceImportAttribute");
        _iCakeContextSymbol = _compilation.GetTypeByMetadataName("ICakeContext")!;
        _cakeModuleAttributeSymbol = _compilation.GetTypeByMetadataName("CakeModuleAttribute")!;
        _iCakeModuleSymbol = _compilation.GetTypeByMetadataName("ICakeModule")!;
        _obsoleteAttributeSymbol = _compilation.GetTypeByMetadataName("System.ObsoleteAttribute");
    }

    [Benchmark]
    public void ScanTypeMembers_Benchmark()
    {
        // OCE: Call static scan method for Cake alias discovery
        var methods = CakeGenerator.ScanTypeMembers(_compilation.Assembly.GlobalNamespace, _cakeMethodAliasAttributeSymbol, _cakePropertyAliasAttributeSymbol, _cakeNamespaceImportAttributeSymbol, _iCakeContextSymbol);
    }

    [Benchmark]
    public void ScanForModules_Benchmark()
    {
        // OCE: Call static scan method for Cake module discovery
        var modules = CakeGenerator.ScanForModules(_compilation.Assembly, _cakeModuleAttributeSymbol!, _iCakeModuleSymbol!, _obsoleteAttributeSymbol);
    }
}
#nullable restore