using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Rules;

namespace TradeJournal.Tests;

public class StaticDrawdownRuleTests
{
    // limit 2000 przy saldzie 50 000: poziom limitu to 48 000
    private readonly StaticDrawdownRule _rule = new(2000m);

    [Fact]
    public void Evaluate_IsOkWithoutEntries()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith());

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_IsOkWhenFarFromLimit()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith(500m));

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_WarnsWhenCloseToLimit()
    {
        // saldo 48 300, do limitu 300 (mniej niż 20% z 2000)
        var result = _rule.Evaluate(TestHelpers.AccountWith(-1700m));

        Assert.Equal(RuleStatus.Warning, result.Status);
    }

    [Theory]
    [InlineData(-2000)]
    [InlineData(-2500)]
    public void Evaluate_IsViolatedWhenBalanceReachesFloor(decimal pnl)
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith(pnl));

        Assert.Equal(RuleStatus.Violated, result.Status);
    }

    [Fact]
    public void Evaluate_IgnoresEarlierPeak()
    {
        // saldo 49 000: nadal 1000 nad stałym poziomem 48 000
        var result = _rule.Evaluate(TestHelpers.AccountWith(1000m, -2000m));

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_ResultContainsRuleName()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith());

        Assert.Equal(_rule.Name, result.RuleName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Constructor_ThrowsForNonPositiveLimit(decimal limit)
    {
        Assert.Throws<InvalidRuleException>(() => new StaticDrawdownRule(limit));
    }

    [Fact]
    public void Evaluate_ThrowsForNullAccount()
    {
        Assert.Throws<ArgumentNullException>(() => _rule.Evaluate(null!));
    }
}