using TradeJournal.Core.Rules;

namespace TradeJournal.Tests;

public class TopstepPresetTests
{
    [Theory]
    [InlineData(TopstepSize.Size50K, 50000, 2000, 1000)]
    [InlineData(TopstepSize.Size100K, 100000, 3000, 2000)]
    [InlineData(TopstepSize.Size150K, 150000, 4500, 3000)]
    public void Parameters_MatchAccountSize(
        TopstepSize size, decimal balance, decimal maxLoss, decimal dailyLoss)
    {
        Assert.Equal(balance, TopstepPreset.StartingBalance(size));
        Assert.Equal(maxLoss, TopstepPreset.MaximumLossLimit(size));
        Assert.Equal(dailyLoss, TopstepPreset.DailyLossLimit(size));
    }

    [Fact]
    public void CreateCombineRules_HasTwoRulesByDefault()
    {
        var engine = TopstepPreset.CreateCombineRules(TopstepSize.Size50K);

        Assert.Equal(2, engine.Rules.Count);
        Assert.IsType<TopstepMaximumLossLimitRule>(engine.Rules[0]);
        Assert.IsType<ConsistencyRule>(engine.Rules[1]);
    }

    [Fact]
    public void CreateCombineRules_AddsDailyLossLimitOnRequest()
    {
        var engine = TopstepPreset.CreateCombineRules(TopstepSize.Size50K, withDailyLossLimit: true);

        Assert.Equal(3, engine.Rules.Count);
        Assert.IsType<DailyLossLimitRule>(engine.Rules[2]);
    }

    [Theory]
    [InlineData(TopstepSize.Size50K, 2000)]
    [InlineData(TopstepSize.Size100K, 3000)]
    [InlineData(TopstepSize.Size150K, 4500)]
    public void CreateCombineRules_UsesLimitForSize(TopstepSize size, decimal expected)
    {
        var engine = TopstepPreset.CreateCombineRules(size);

        var rule = Assert.IsType<TopstepMaximumLossLimitRule>(engine.Rules[0]);
        Assert.Equal(expected, rule.MaxDrawdown);
    }

    [Fact]
    public void CreateCombineRules_UsesConsistencyShare()
    {
        var engine = TopstepPreset.CreateCombineRules(TopstepSize.Size50K);

        var rule = Assert.IsType<ConsistencyRule>(engine.Rules[1]);
        Assert.Equal(TopstepPreset.ConsistencyShare, rule.MaxBestDayShare);
    }

    // konto testowe ma saldo 50 000, więc pasuje do rozmiaru 50K
    [Fact]
    public void OverallStatus_IsOkForEmptyAccount()
    {
        var engine = TopstepPreset.CreateCombineRules(TopstepSize.Size50K);

        Assert.Equal(RuleStatus.Ok, engine.OverallStatus(TestHelpers.AccountWith()));
    }

    [Fact]
    public void OverallStatus_IsViolatedAtMaximumLossLimit()
    {
        var engine = TopstepPreset.CreateCombineRules(TopstepSize.Size50K);

        Assert.Equal(RuleStatus.Violated, engine.OverallStatus(TestHelpers.AccountWith(-2000m)));
    }

    [Fact]
    public void OverallStatus_WarnsWhenOneDayIsAllTheProfit()
    {
        var engine = TopstepPreset.CreateCombineRules(TopstepSize.Size50K);

        Assert.Equal(RuleStatus.Warning, engine.OverallStatus(TestHelpers.AccountWith(3000m)));
    }

    [Fact]
    public void Methods_ThrowForUnknownSize()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => TopstepPreset.CreateCombineRules((TopstepSize)99));
    }
}
