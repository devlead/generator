// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Cake.Generator;

public partial class CakeGenerator
{
    private static List<string> ScanForMainMethods(Compilation compilation)
    {
        var mainMethods = new List<string>();

        // Find the Program type in the current compilation
        var programType = compilation.GlobalNamespace.GetTypeMembers("Program").FirstOrDefault();
        if (programType == null)
        {
            return mainMethods;
        }

        // Scan for static private void methods prefixed with Main_ and no parameters
        foreach (var member in programType.GetMembers())
        {
            if (member is IMethodSymbol methodSymbol &&
                methodSymbol.IsStatic &&
                methodSymbol.DeclaredAccessibility == Accessibility.Private &&
                methodSymbol.ReturnType.SpecialType == SpecialType.System_Void &&
                methodSymbol.Parameters.Length == 0 &&
                methodSymbol.Name.StartsWith("Main_"))
            {
                mainMethods.Add(methodSymbol.Name);
            }
        }

        return mainMethods;
    }
}