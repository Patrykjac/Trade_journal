using TradeJournal.Core;
using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Rules;

namespace TradeJournal.Tests;

public class RuleEngineTests
{
    // atrapa reguły: zwraca zawsze zadany status
    private sealed class FakeRule : IRule
    {
        private readonly RuleStatus _status;

        public FakeRule(string name, RuleStatus status)
        {
            Name = name;
            _status = status;
        }

        public string Name { get; }

        public RuleResult Evaluate(Account account) => new(Name, _status, "test");
    }

    [Fact]
    public void Evaluate_ReturnsOneResultPerRuleInOrder()
    {
        var engine = new RuleEngine(new IRule[]
        {
            new FakeRule("A", RuleStatus.Ok),
            new FakeRule("B", RuleStatus.Violated)
        });

        var results = engine.Evaluate(TestHelpers.AccountWith());

        Assert.Equal(2, results.Count);
        Assert.Equal("A", results[0].RuleName);
        Assert.Equal("B", results[1].RuleName);
    }

    [Fact]
    public void Evaluate_WorksWithDifferentRuleTypes()
    {
        // jeden dzień ze stratą 1700: obie reguły drawdownu ostrzegają, dzienny limit 1000 przekroczony
        var engine = new RuleEngine(new IRule[]
        {
            new StaticDrawdownRule(2000m),
            new TrailingEndOfDayDrawdownRule(2000m),
            new DailyLossLimitRule(1000m)
        });

        var results = engine.Evaluate(TestHelpers.AccountWith(-1700m));

        Assert.Equal(
            new[] { RuleStatus.Warning, RuleStatus.Warning, RuleStatus.Violated },
            results.Select(r => r.Status));
    }

    [Fact]
    public void OverallStatus_IsOkWithoutRules()
    {
        var engine = new RuleEngine(Array.Empty<IRule>());

        Assert.Equal(RuleStatus.Ok, engine.OverallStatus(TestHelpers.AccountWith()));
    }

    [Fact]
    public void OverallStatus_IsWorstOfAllResults()
    {
        var engine = new RuleEngine(new IRule[]
        {
            new FakeRule("A", RuleStatus.Ok),
            new FakeRule("B", RuleStatus.Violated),
            new FakeRule("C", RuleStatus.Warning)
        });

        Assert.Equal(RuleStatus.Violated, engine.OverallStatus(TestHelpers.AccountWith()));
    }

    [Fact]
    public void OverallStatus_IsWarningWhenNoViolation()
    {
        var engine = new RuleEngine(new IRule[]
        {
            new FakeRule("A", RuleStatus.Ok),
            new FakeRule("B", RuleStatus.Warning)
        });

        Assert.Equal(RuleStatus.Warning, engine.OverallStatus(TestHelpers.AccountWith()));
    }

    [Fact]
    public void Rules_ExposesConfiguredRules()
    {
        var engine = new RuleEngine(new IRule[] { new FakeRule("A", RuleStatus.Ok) });

        Assert.Single(engine.Rules);
    }

    [Fact]
    public void Constructor_ThrowsForNullCollection()
    {
        Assert.Throws<ArgumentNullException>(() => new RuleEngine(null!));
    }

    [Fact]
    public void Constructor_ThrowsForNullRuleInCollection()
    {
        Assert.Throws<InvalidRuleException>(() => new RuleEngine(new IRule[] { null! }));
    }

    [Fact]
    public void Evaluate_ThrowsForNullAccount()
    {
        var engine = new RuleEngine(Array.Empty<IRule>());

        Assert.Throws<ArgumentNullException>(() => engine.Evaluate(null!));
    }
}