namespace Cake.Generator;

public partial class CakeGenerator
{
    private static List<MethodInfo> ScanTypeMembers(
        INamespaceSymbol namespaceSymbol,
        INamedTypeSymbol? cakeMethodAliasAttributeSymbol,
        INamedTypeSymbol? cakePropertyAliasAttributeSymbol,
        INamedTypeSymbol? cakeNamespaceImportAttributeSymbol,
        INamedTypeSymbol iCakeContextSymbol)
    {
        var validMethods = new List<MethodInfo>();
        ScanNamespaceMembers(namespaceSymbol, cakeMethodAliasAttributeSymbol, cakePropertyAliasAttributeSymbol, cakeNamespaceImportAttributeSymbol, iCakeContextSymbol, validMethods);
        return validMethods;
    }

    private static void ScanNamespaceMembers(
        INamespaceSymbol namespaceSymbol,
        INamedTypeSymbol? cakeMethodAliasAttributeSymbol,
        INamedTypeSymbol? cakePropertyAliasAttributeSymbol,
        INamedTypeSymbol? cakeNamespaceImportAttributeSymbol,
        INamedTypeSymbol iCakeContextSymbol,
        List<MethodInfo> validMethods)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamedTypeSymbol typeSymbol)
            {
                ScanTypeMembers(typeSymbol, cakeMethodAliasAttributeSymbol,
                              cakePropertyAliasAttributeSymbol, cakeNamespaceImportAttributeSymbol, iCakeContextSymbol, validMethods);
            }
            else if (member is INamespaceSymbol nestedNamespace)
            {
                ScanNamespaceMembers(nestedNamespace, cakeMethodAliasAttributeSymbol,
                                   cakePropertyAliasAttributeSymbol, cakeNamespaceImportAttributeSymbol, iCakeContextSymbol, validMethods);
            }
        }
    }

    private static void ScanTypeMembers(
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol? cakeMethodAliasAttributeSymbol,
        INamedTypeSymbol? cakePropertyAliasAttributeSymbol,
        INamedTypeSymbol? cakeNamespaceImportAttributeSymbol,
        INamedTypeSymbol iCakeContextSymbol,
        List<MethodInfo> validMethods)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IMethodSymbol methodSymbol
                &&
                methodSymbol.IsStatic
                &&
                methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                var methodInfo = GetValidCakeMethod(methodSymbol, cakeMethodAliasAttributeSymbol, cakePropertyAliasAttributeSymbol, cakeNamespaceImportAttributeSymbol, iCakeContextSymbol);
                if (methodInfo != null)
                {
                    validMethods.Add(methodInfo);
                }
            }
        }

        // Scan nested types
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            ScanTypeMembers(nestedType, cakeMethodAliasAttributeSymbol, cakePropertyAliasAttributeSymbol, cakeNamespaceImportAttributeSymbol, iCakeContextSymbol, validMethods);
        }
    }

    private static MethodInfo? GetValidCakeMethod(IMethodSymbol methodSymbol,
        INamedTypeSymbol? cakeMethodAliasAttributeSymbol,
        INamedTypeSymbol? cakePropertyAliasAttributeSymbol,
        INamedTypeSymbol? cakeNamespaceImportAttributeSymbol,
        INamedTypeSymbol iCakeContextSymbol)
    {
        var attributes = methodSymbol.GetAttributes();

        // Check if method has CakeMethodAlias or CakePropertyAlias attribute - cache results
        AttributeData? cakeMethodAttribute = null;
        AttributeData? cakePropertyAttribute = null;

        if (cakeMethodAliasAttributeSymbol != null || cakePropertyAliasAttributeSymbol != null)
        {
            foreach (var attr in attributes)
            {
                if (cakeMethodAliasAttributeSymbol != null &&
                    SymbolEqualityComparer.Default.Equals(attr.AttributeClass, cakeMethodAliasAttributeSymbol))
                {
                    cakeMethodAttribute = attr;
                }
                else if (cakePropertyAliasAttributeSymbol != null &&
                         SymbolEqualityComparer.Default.Equals(attr.AttributeClass, cakePropertyAliasAttributeSymbol))
                {
                    cakePropertyAttribute = attr;
                }
            }
        }

        if (cakeMethodAttribute == null && cakePropertyAttribute == null)
        {
            return null;
        }

        // Check if first parameter is ICakeContext
        if (methodSymbol.Parameters.Length == 0)
        {
            return null;
        }

        var firstParam = methodSymbol.Parameters[0];
        if (!SymbolEqualityComparer.Default.Equals(firstParam.Type, iCakeContextSymbol))
        {
            return null;
        }

        // Determine if this is a property alias and if it's cached
        bool isPropertyAlias = cakePropertyAttribute != null;
        bool isCached = false;

        if (isPropertyAlias && cakePropertyAttribute != null)
        {
            // Check if Cache parameter is set to true
            var cacheArg = cakePropertyAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Cache");
            if (cacheArg.Key == "Cache" && cacheArg.Value.Value is bool cacheValue)
            {
                isCached = cacheValue;
            }
        }

        // Extract namespace imports from CakeNamespaceImportAttribute
        var namespaceImports = new List<string>();
        if (cakeNamespaceImportAttributeSymbol != null)
        {
            foreach (var attr in attributes)
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, cakeNamespaceImportAttributeSymbol))
                {
                    if (attr.ConstructorArguments.Length > 0 &&
                        attr.ConstructorArguments[0].Value is string namespaceValue &&
                        !string.IsNullOrWhiteSpace(namespaceValue))
                    {
                        namespaceImports.Add(namespaceValue);
                    }
                }
            }
        }

        return new MethodInfo(methodSymbol, isPropertyAlias, isCached, namespaceImports.ToArray());
    }
}