using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Rules;

namespace TradeJournal.Tests;

public class TrailingEndOfDayDrawdownRuleTests
{
    // limit 2000 liczony od dotychczasowego szczytu salda
    private readonly TrailingEndOfDayDrawdownRule _rule = new(2000m);

    [Fact]
    public void Evaluate_IsOkWithoutEntries()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith());

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_IsOkWhenStillAboveRaisedFloor()
    {
        // szczyt 51 000, poziom 49 000, saldo 49 500
        var result = _rule.Evaluate(TestHelpers.AccountWith(1000m, -1500m));

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_WarnsWhenCloseToRaisedFloor()
    {
        // szczyt 51 000, poziom 49 000, saldo 49 300
        var result = _rule.Evaluate(TestHelpers.AccountWith(1000m, -1700m));

        Assert.Equal(RuleStatus.Warning, result.Status);
    }

    [Fact]
    public void Evaluate_IsViolatedWhenBalanceDropsToRaisedFloor()
    {
        // szczyt 51 000, poziom 49 000, saldo 49 000
        var result = _rule.Evaluate(TestHelpers.AccountWith(1000m, -2000m));

        Assert.Equal(RuleStatus.Violated, result.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Constructor_ThrowsForNonPositiveLimit(decimal limit)
    {
        Assert.Throws<InvalidRuleException>(() => new TrailingEndOfDayDrawdownRule(limit));
    }

    [Fact]
    public void Evaluate_ThrowsForNullAccount()
    {
        Assert.Throws<ArgumentNullException>(() => _rule.Evaluate(null!));
    }
}