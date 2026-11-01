using TradeJournal.Core;
using TradeJournal.Core.Exceptions;

namespace TradeJournal.Tests;

public class DayEntryTests
{
    private static readonly DateOnly SomeDay = new(2025, 1, 15);

    [Fact]
    public void Constructor_SetsDateAndPnl()
    {
        var entry = new DayEntry(SomeDay, 250m);

        Assert.Equal(SomeDay, entry.Date);
        Assert.Equal(250m, entry.Pnl);
    }

    [Fact]
    public void IsWinning_TrueForPositivePnl()
    {
        var entry = new DayEntry(SomeDay, 100m);

        Assert.True(entry.IsWinning);
        Assert.False(entry.IsLosing);
    }

    [Fact]
    public void IsLosing_TrueForNegativePnl()
    {
        var entry = new DayEntry(SomeDay, -100m);

        Assert.True(entry.IsLosing);
        Assert.False(entry.IsWinning);
    }

    [Fact]
    public void ZeroPnl_IsNeitherWinningNorLosing()
    {
        var entry = new DayEntry(SomeDay, 0m);

        Assert.False(entry.IsWinning);
        Assert.False(entry.IsLosing);
    }

    [Fact]
    public void Constructor_ThrowsForFutureDate()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

        Assert.Throws<InvalidDayEntryException>(() => new DayEntry(tomorrow, 10m));
    }

    [Fact]
    public void SetNote_TrimsWhitespace()
    {
        var entry = new DayEntry(SomeDay, 10m);

        entry.SetNote("  dobry dzień  ");

        Assert.Equal("dobry dzień", entry.Note);
    }

    [Fact]
    public void SetNote_ThrowsWhenTooLong()
    {
        var entry = new DayEntry(SomeDay, 10m);
        var tooLong = new string('x', DayEntry.MaxNoteLength + 1);

        Assert.Throws<InvalidDayEntryException>(() => entry.SetNote(tooLong));
    }

    [Fact]
    public void Constructor_ThrowsForDefaultDate()
    {
        Assert.Throws<InvalidDayEntryException>(() => new DayEntry(default, 10m));
    }
}