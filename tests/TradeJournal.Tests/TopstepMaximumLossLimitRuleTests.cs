using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Rules;

namespace TradeJournal.Tests;

public class TopstepMaximumLossLimitRuleTests
{
    // saldo początkowe 50 000, limit 2 000: poziom startowy to 48 000
    private readonly TopstepMaximumLossLimitRule _rule = new(2000m);

    [Fact]
    public void Evaluate_IsOkWithoutEntries()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith());

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_IsViolatedAtInitialFloor()
    {
        var result = _rule.Evaluate(TestHelpers.AccountWith(-2000m));

        Assert.Equal(RuleStatus.Violated, result.Status);
    }

    [Fact]
    public void Evaluate_FloorTrailsUpWithEndOfDayPeak()
    {
        // szczyt 51 000, poziom 49 000, saldo 49 000
        var result = _rule.Evaluate(TestHelpers.AccountWith(1000m, -2000m));

        Assert.Equal(RuleStatus.Violated, result.Status);
    }

    [Fact]
    public void Evaluate_FloorLocksAtStartingBalance()
    {
        // szczyt 53 000, zwykły trailing dałby poziom 51 000, tu poziom zablokowany na 50 000
        // saldo 50 500, do poziomu 500 (więcej niż 20% z 2 000)
        var result = _rule.Evaluate(TestHelpers.AccountWith(3000m, -2500m));

        Assert.Equal(RuleStatus.Ok, result.Status);
    }

    [Fact]
    public void Evaluate_WarnsNearLockedFloor()
    {
        // saldo 50 100, do zablokowanego poziomu 100
        var result = _rule.Evaluate(TestHelpers.AccountWith(3000m, -2900m));

        Assert.Equal(RuleStatus.Warning, result.Status);
    }

    [Fact]
    public void Evaluate_IsViolatedWhenBalanceReachesLockedFloor()
    {
        // saldo 50 000 = zablokowany poziom
        var result = _rule.Evaluate(TestHelpers.AccountWith(3000m, -3000m));

        Assert.Equal(RuleStatus.Violated, result.Status);
    }

    [Fact]
    public void Evaluate_DiffersFromPlainTrailingAfterLock()
    {
        var account = TestHelpers.AccountWith(3000m, -2500m);

        var plain = new TrailingEndOfDayDrawdownRule(2000m).Evaluate(account);
        var topstep = _rule.Evaluate(account);

        Assert.Equal(RuleStatus.Violated, plain.Status);
        Assert.Equal(RuleStatus.Ok, topstep.Status);
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
        Assert.Throws<InvalidRuleException>(() => new TopstepMaximumLossLimitRule(limit));
    }
}