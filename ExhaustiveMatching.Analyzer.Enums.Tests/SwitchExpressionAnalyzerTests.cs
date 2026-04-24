using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Testing.Helpers;
using ExhaustiveMatching.Analyzer.Testing.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Enums.Tests
{
    public class SwitchExpressionAnalyzerTests : DiagnosticVerifier
    {
        [Fact]
        public async Task NotExhaustiveReportsDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        var result = coinFlip ◊1⟦switch⟧
        {
            CoinFlip.Heads => ""Heads!"",
            _ => throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip)),
        };";

            var source = CodeContext.CoinFlip(args, test);
            var expectedTails = DiagnosticResult.Error("EM0001", "Enum value not handled by switch 'CoinFlip.Tails'")
                                                .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedTails);
        }

        [Fact]
        public async Task NullableEnumWithoutNullReportsDiagnostic()
        {
            const string args = "CoinFlip? coinFlip";
            const string test = @"
        var result = coinFlip ◊1⟦switch⟧
        {
            CoinFlip.Heads => ""Heads!"",
            CoinFlip.Tails => ""Tails!"",
            _ => throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip)),
        };";

            var source = CodeContext.CoinFlip(args, test);
            var expectedNull = DiagnosticResult.Error("EM0002", "'null' value not handled by switch")
                                               .AddLocation(source, 1);

            await VerifyCSharpDiagnosticsAsync(source, expectedNull);
        }

        [Fact]
        public async Task ExhaustiveReportsNoDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        var result = coinFlip switch
        {
            CoinFlip.Heads => ""Heads!"",
            CoinFlip.Tails => ""Tails!"",
            _ => throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip)),
        };";

            await VerifyCSharpDiagnosticsAsync(CodeContext.CoinFlip(args, test));
        }

        [Fact]
        public async Task UnsupportedPatternReportsDiagnostic()
        {
            const string args = "CoinFlip coinFlip";
            const string test = @"
        var result = coinFlip ◊1⟦switch⟧
        {
            CoinFlip.Heads => ""Heads!"",
            ◊2⟦CoinFlip flip⟧ when false => flip.ToString(),
            _ => throw new InvalidEnumArgumentException(nameof(coinFlip), (int)coinFlip, typeof(CoinFlip)),
        };";

            var source = CodeContext.CoinFlip(args, test);
            var expectedTails = DiagnosticResult.Error("EM0001", "Enum value not handled by switch 'CoinFlip.Tails'")
                                                .AddLocation(source, 1);
            var unsupportedPattern = DiagnosticResult
                .Error("EM0101", "Case pattern not supported in exhaustive switch on enum type 'CoinFlip flip'")
                .AddLocation(source, 2);

            await VerifyCSharpDiagnosticsAsync(source, expectedTails, unsupportedPattern);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ExhaustiveMatchEnumAnalyzer();
    }
}
