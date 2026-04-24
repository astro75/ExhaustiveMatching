using System.Threading.Tasks;
using ExhaustiveMatching.Analyzer.Testing.Verifiers;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace ExhaustiveMatching.Analyzer.Tests
{
    public class CSharp14CompatibilityTests : DiagnosticVerifier
    {
        [Fact]
        public async Task CSharp14SyntaxParsesAndAnalyzes()
        {
            const string source = @"using System;
using System.Collections.Generic;
using System.ComponentModel;
using ExhaustiveMatching;

enum CoinFlip { Heads = 1, Tails }

public static class StringExtensions
{
    extension(string value)
    {
        public bool IsEmpty => value.Length == 0;
    }
}

partial class PartialMembers
{
    public partial PartialMembers();
    public partial event EventHandler Happened;
}

partial class PartialMembers
{
    public partial PartialMembers() { }
    public partial event EventHandler Happened
    {
        add { }
        remove { }
    }
}

class Customer
{
    public int Order { get; set; }
}

class TestClass
{
    private string Message
    {
        get;
        set => field = value ?? string.Empty;
    } = string.Empty;

    private delegate bool TryParse<T>(string text, out T result);

    void TestMethod(CoinFlip coinFlip)
    {
        _ = nameof(List<>);
        TryParse<int> parse = (text, out result) => int.TryParse(text, out result);
        Customer customer = null;
        customer?.Order = 1;

        _ = coinFlip switch
        {
            CoinFlip.Heads => ""Heads!"",
            CoinFlip.Tails => ""Tails!"",
            _ => throw ExhaustiveMatch.Failed(coinFlip),
        };
    }
}";

            await VerifyCSharpDiagnosticsAsync(source);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
            => new ExhaustiveMatchAnalyzer();
    }
}
