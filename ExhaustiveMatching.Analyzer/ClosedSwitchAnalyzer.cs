using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ExhaustiveMatching.Analyzer.Semantics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer
{
    internal static class ClosedSwitchAnalyzer
    {
        public static void Analyze(
            SyntaxNodeAnalysisContext context,
            ITypeSymbol type,
            SyntaxToken switchKeyword,
            ExpressionSyntax governingExpression,
            bool nullRequired,
            bool hasNullCase,
            Func<HashSet<ITypeSymbol>, bool, ImmutableHashSet<ITypeSymbol>> getTypesUsed)
        {
            var closedAttributeType = context.GetClosedAttributeType();
            var isClosed = type.HasAttribute(closedAttributeType);

            var allCases = type.GetClosedTypeCases(closedAttributeType);
            var allConcreteTypes = allCases
                .Where(t => t.IsConcreteOrLeaf(closedAttributeType));

            if (!isClosed && type.TryGetStructurallyClosedTypeCases(context, out allCases))
            {
                isClosed = true;
                allConcreteTypes = allCases
                    .Where(t => t.IsConcrete());
            }

            var typesUsed = getTypesUsed(allCases, isClosed);

            // If it is an open type, we don't want to actually check for uncovered types, but
            // we still needed to check the switch cases.
            if (!isClosed)
            {
                context.ReportOpenTypeNotSupported(type, governingExpression);
                return;
            }

            if (nullRequired && !hasNullCase)
                context.ReportNotExhaustiveNullableObjectSwitch(switchKeyword);

            var uncoveredTypes = allConcreteTypes
                .Where(t => !typesUsed.Any(t.IsSubtypeOf))
                .ToArray();

            context.ReportNotExhaustiveObjectSwitch(switchKeyword, uncoveredTypes);
        }
    }
}
