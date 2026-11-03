using TradeJournal.Core;
using TradeJournal.Core.Exceptions;

namespace TradeJournal.Tests;

public class AccountTest
{
    private static DayEntry Entry(int day, decimal pnl) => new(new DateOnly(2025, 1, day), pnl);

    private static Account NewAccount() => new("Topstep 50K", 50000m);

    [Fact]
    public void Constructor_SetsNameAndStartingBalance()
    {
        var account = new Account("  Topstep 50K  ", 50000m);

        Assert.Equal("Topstep 50K", account.Name);
        Assert.Equal(50000m, account.StartingBalance);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_ThrowsForEmptyName(string? name)
    {
        Assert.Throws<InvalidAccountException>(() => new Account(name!, 1000m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Constructor_ThrowsForNonPositiveBalance(decimal balance)
    {
        Assert.Throws<InvalidAccountException>(() => new Account("Konto", balance));
    }
    
    [Fact]
    public void AddEntry_AddsEntryToAccount()
    {
        var account = NewAccount();

        account.AddEntry(Entry(10, 200m));

        Assert.Single(account.Entries);
        Assert.Equal(1, account.TradingDays);
    }

    [Fact]
    public void AddEntry_ThrowsForDuplicateDate()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 200m));

        Assert.Throws<DuplicateEntryException>(() => account.AddEntry(Entry(10, -50m)));
    }
    [Fact]
    public void AddEntry_ThrowsForNull()
    {
        var account = NewAccount();

        Assert.Throws<ArgumentNullException>(() => account.AddEntry(null!));
    }

    [Fact]
    public void TotalPnlAndCurrentBalance_SumAllEntries()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 300m));
        account.AddEntry(Entry(11, -100m));

        Assert.Equal(200m, account.TotalPnl);
        Assert.Equal(50200m, account.CurrentBalance);
    }

    [Fact]
    public void WinRate_IsZeroWithoutEntries()
    {
        Assert.Equal(0m, NewAccount().WinRate);
    }

    [Fact]
    public void WinRate_CountsOnlyWinningDays()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 100m));
        account.AddEntry(Entry(11, 200m));
        account.AddEntry(Entry(12, -50m));
        account.AddEntry(Entry(13, 0m));

        Assert.Equal(50m, account.WinRate);
    }

    [Fact]
    public void AverageWinningDay_IsMeanOfWinningDays()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 100m));
        account.AddEntry(Entry(11, 300m));
        account.AddEntry(Entry(12, -500m));

        Assert.Equal(200m, account.AverageWinningDay);
    }

    [Fact]
    public void AverageLosingDay_IsMeanOfLosingDays()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, -100m));
        account.AddEntry(Entry(11, -300m));
        account.AddEntry(Entry(12, 500m));

        Assert.Equal(-200m, account.AverageLosingDay);
    }

    [Fact]
    public void Averages_AreZeroWhenNoMatchingDays()
    {
        var account = NewAccount();
        account.AddEntry(Entry(10, 100m));

        Assert.Equal(0m, account.AverageLosingDay);
        Assert.Equal(0m, NewAccount().AverageWinningDay);
    }
}