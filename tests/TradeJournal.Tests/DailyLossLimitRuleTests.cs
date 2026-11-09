using TradeJournal.Core;
using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Rules;

namespace TradeJournal.Tests;

public class DailyLossLimitRuleTest
{
    private readonly DailyLossLimitRule _rule = new(1000m);

    [Fact]
    public void Evaluate_IsOkWithoutEntries()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith());

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_IsOkAfterWinningDay()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith(-3000m,500m));

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Theory]
    [InlineData(-500, RuleStatus.Ok)]
    [InlineData(-799, RuleStatus.Ok)]
    [InlineData(-800, RuleStatus.Warning)]
    [InlineData(-999, RuleStatus.Warning)]
    [InlineData(-1000, RuleStatus.Violated)]
    [InlineData(-1500, RuleStatus.Violated)]
    public void Evaluate_ClassifiesLatestDayLoss(decimal pnl, RuleStatus expected)
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith(pnl));

        Assert.Equal(expected, result.Status);
    }

    [Fact]
    public void Evaluate_UsesLatestDateNotInsertionOrder()
    {
        var account = new Account("Konto testowe", 50000m);
        account.AddEntry(new DayEntry(new DateOnly(2025, 1, 12), 100m));
        account.AddEntry(new DayEntry(new DateOnly(2025, 1, 10), -2000m));

        var result = _rule.Evaluate(account);

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
        Assert.Throws<InvalidRuleException>(() => new DailyLossLimitRule(limit));
    }

    [Fact]
    public void Evaluate_ThrowsForNullAccount()
    {
        Assert.Throws<ArgumentNullException>(() => _rule.Evaluate(null!));
    }
}