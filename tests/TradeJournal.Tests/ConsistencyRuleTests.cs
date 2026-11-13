using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Rules;

namespace TradeJournal.Tests;

public class ConsistencyRuleTests
{
    private readonly ConsistencyRule _rule = new(0.55m);

    [Fact]
    public void Evaluate_IsOkWithoutEntries()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith());

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_IsOkWhenThereIsNoProfit()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith(-500m, 300m));

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_IsOkWhenBestDayIsBelowLimit()
    {
        // najlepszy dzień 500 z 1000 = 50%
        var result = _rule.Evaluate(TestHelpers.AccountWith(500m, 300m, 200m));

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_IsOkWhenBestDayIsExactlyAtLimit()
    {
        // 550 z 1000 = 55%
        var result = _rule.Evaluate(TestHelpers.AccountWith(550m, 450m));

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_WarnsWhenBestDayExceedsLimit()
    {
        // 600 z 1000 = 60%
        var result = _rule.Evaluate(TestHelpers.AccountWith(600m, 200m, 200m));

        Assert.Equal(RuleStatus.Warning, result.Status);
    }

    [Fact]
    public void Evaluate_UsesNetProfitIncludingLosingDays()
    {
        // najlepszy dzień 800, zysk netto 1200 = 66,7%
        var result = _rule.Evaluate(TestHelpers.AccountWith(800m, -200m, 600m));

        Assert.Equal(RuleStatus.Warning, result.Status);
    }

    [Fact]
    public void Evaluate_WarnsForSingleWinningDay()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith(1000m));

        Assert.Equal(RuleStatus.Warning, result.Status);
    }

    [Fact]
    public void Evaluate_NeverReturnsViolated()
    {
        // przekroczenie progu to ostrzeżenie: podnosi cel zysku, nie kończy konta
        var result = _rule.Evaluate(TestHelpers.AccountWith(5000m, 100m));

        Assert.NotEqual(RuleStatus.Violated, result.Status);
    }

    [Fact]
    public void Evaluate_ResultContainsRuleName()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith());

        Assert.Equal(_rule.Name, result.RuleName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(2)]
    public void Constructor_ThrowsForShareOutsideRange(decimal share)
    {
        Assert.Throws<InvalidRuleException>(() => new ConsistencyRule(share));
    }

    [Fact]
    public void Evaluate_ThrowsForNullAccount()
    {
        Assert.Throws<ArgumentNullException>(() => _rule.Evaluate(null!));
    }
}