using System.Linq;
using ExhaustiveMatching.Analyzer.Enums.Analysis;
using ExhaustiveMatching.Analyzer.Enums.Semantics;
using ExhaustiveMatching.Analyzer.Enums.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ExhaustiveMatching.Analyzer.Enums
{
    internal class EnumSwitchExpressionAnalyzer
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, SwitchExpressionSyntax switchExpression)
        {
            if (!IsExhaustive(context, switchExpression)) return;

            ReportUnsupportedPatterns(context, switchExpression);

            var switchOnType = context.GetExpressionConvertedType(switchExpression.GoverningExpression);

            if (switchOnType != null && switchOnType.IsEnum(context, out var enumType, out var nullable))
                AnalyzeSwitchOnEnum(context, switchExpression, enumType, nullable);

            // TODO report warning that throws invalid enum isn't checked for exhaustiveness
        }

        private static bool IsExhaustive(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchExpression)
        {
            // If there is no discard arm or it doesn't throw, we assume the
            // dev doesn't want an exhaustive match.
            var discardArm = switchExpression.Arms.LastOrDefault(a => a.Pattern is DiscardPatternSyntax);
            if (!(discardArm?.Expression is ThrowExpressionSyntax throwExpression))
                return false;

            var exceptionType = context.GetExpressionType(throwExpression.Expression);
            return exceptionType != null
                   && !(exceptionType is IErrorTypeSymbol)
                   && exceptionType.IsInvalidEnumArgumentException();
        }

        private static void ReportUnsupportedPatterns(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchExpression)
        {
            var unsupportedPatterns = switchExpression.Arms
                .Select(a => a.Pattern)
                .Where(p => !(p is ConstantPatternSyntax) && !(p is DiscardPatternSyntax));

            foreach (var pattern in unsupportedPatterns)
                Diagnostics.ReportCasePatternNotSupported(context, pattern);
        }

        private static void AnalyzeSwitchOnEnum(
            SyntaxNodeAnalysisContext context,
            SwitchExpressionSyntax switchExpression,
            INamedTypeSymbol enumType,
            bool nullRequired)
        {
            var casePatterns = switchExpression.Arms
                .Select(a => a.Pattern)
                .OfType<ConstantPatternSyntax>()
                .ToList();

            // If null were not required, and there were a null case, that would already be a compile error.
            if (nullRequired && !casePatterns.Any(p => p.Expression.IsNullConstantExpression()))
                Diagnostics.ReportNotExhaustiveNullableEnumSwitch(context, switchExpression);

            var caseExpressions = casePatterns.Select(p => p.Expression);
            var unusedSymbols = SwitchOnEnumAnalyzer.UnusedEnumValues(context, enumType, caseExpressions);
            Diagnostics.ReportNotExhaustiveEnumSwitch(context, switchExpression, unusedSymbols);
        }
    }
}
