using TradeJournal.Core;
using TradeJournal.Core.Exceptions;

namespace TradeJournal.Tests;

public class AccountIdTests
{
    [Fact]
    public void NewAccounts_HaveDifferentIds()
    {
        var first = new Account("Konto A", 1000m);
        var second = new Account("Konto B", 1000m);

        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Constructor_KeepsGivenId()
    {
        var id = Guid.NewGuid();

        var account = new Account(id, "Konto A", 1000m);

        Assert.Equal(id, account.Id);
        Assert.Equal("Konto A", account.Name);
    }

    [Fact]
    public void Constructor_ThrowsForEmptyId()
    {
        Assert.Throws<InvalidAccountException>(() => new Account(Guid.Empty, "Konto A", 1000m));
    }

    [Fact]
    public void Constructor_WithId_StillValidatesNameAndBalance()
    {
        Assert.Throws<InvalidAccountException>(() => new Account(Guid.NewGuid(), " ", 1000m));
        Assert.Throws<InvalidAccountException>(() => new Account(Guid.NewGuid(), "Konto A", 0m));
    }
}