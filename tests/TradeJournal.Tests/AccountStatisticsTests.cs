using TradeJournal.Core;

namespace TradeJournal.Tests;

public class AccountStatisticsTests
{
    private static DayEntry Entry(int day, decimal pnl) =>
        new(new DateOnly(2025, 1, day), pnl);

    private static Account NewAccount() => new("Topstep 50K", 50000m);

    [Fact]
    public void ProfitFactor_IsGrossProfitDividedByGrossLoss()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 300m));
        account.AddEntry(Entry(11, 100m));
        account.AddEntry(Entry(12, -200m));

        Assert.Equal(2m, account.ProfitFactor!.Value);
    }

    [Fact]
    public void ProfitFactor_IsNullWhenThereAreNoLosses()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 300m));

        Assert.Null(account.ProfitFactor);
    }

    [Fact]
    public void ProfitFactor_IsNullWithoutEntries()
    {
        Assert.Null(NewAccount().ProfitFactor);
    }

    [Fact]
    public void BestAndWorstDay_ReturnExtremeEntries()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 100m));
        account.AddEntry(Entry(11, 500m));
        account.AddEntry(Entry(12, -300m));

        Assert.Equal(500m, account.BestDay!.Pnl);
        Assert.Equal(-300m, account.WorstDay!.Pnl);
    }

    [Fact]
    public void BestAndWorstDay_AreNullWithoutEntries()
    {
        var account = NewAccount();

        Assert.Null(account.BestDay);
        Assert.Null(account.WorstDay);
    }

    [Fact]
    public void GetEquityCurve_IsEmptyWithoutEntries()
    {
        Assert.Empty(NewAccount().GetEquityCurve());
    }

    [Fact]
    public void GetEquityCurve_IsSortedByDateAndCumulative()
    {
        var account = NewAccount();
        account.AddEntry(Entry(11, -300m));
        account.AddEntry(Entry(10, 100m));

        var curve = account.GetEquityCurve();

        Assert.Equal(2, curve.Count);
        Assert.Equal(new DateOnly(2025, 1, 10), curve[0].Date);
        Assert.Equal(50100m, curve[0].Balance);
        Assert.Equal(new DateOnly(2025, 1, 11), curve[1].Date);
        Assert.Equal(49800m, curve[1].Balance);
    }

    [Fact]
    public void MaxDrawdown_IsLargestDropFromPeak()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 500m));
        account.AddEntry(Entry(11, -300m));
        account.AddEntry(Entry(12, -400m));
        account.AddEntry(Entry(13, 1000m));

        Assert.Equal(700m, account.MaxDrawdown);
    }

    [Fact]
    public void MaxDrawdown_IsZeroWhenOnlyWins()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 100m));
        account.AddEntry(Entry(11, 200m));

        Assert.Equal(0m, account.MaxDrawdown);
    }

    [Fact]
    public void MaxDrawdown_CountsFromStartingBalance()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, -200m));

        Assert.Equal(200m, account.MaxDrawdown);
    }
}