// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Cake.Generator;

public partial class CakeGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodInfo"/> class.
    /// </summary>
    /// <param name="symbol">The method symbol.</param>
    /// <param name="isPropertyAlias">A value indicating whether the method is a property alias.</param>
    /// <param name="isCached">A value indicating whether the property alias is cached.</param>
    /// <param name="namespaceImports">The namespaces to import based on CakeNamespaceImportAttribute.</param>
    internal class MethodInfo(IMethodSymbol symbol, bool isPropertyAlias = false, bool isCached = false, params ImmutableArray<string> namespaceImports)
    {
        /// <summary>
        /// Gets the method symbol.
        /// </summary>
        public IMethodSymbol Symbol { get; } = symbol;

        /// <summary>
        /// Gets a value indicating whether the method is a property alias.
        /// </summary>
        public bool IsPropertyAlias { get; } = isPropertyAlias;

        /// <summary>
        /// Gets a value indicating whether the property alias is cached.
        /// </summary>
        public bool IsCached { get; } = isCached;

        /// <summary>
        /// Gets the namespaces to import based on CakeNamespaceImportAttribute.
        /// </summary>
        public ImmutableArray<string> NamespaceImports { get; } = namespaceImports;
    }
}