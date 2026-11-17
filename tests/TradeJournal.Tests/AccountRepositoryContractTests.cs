using TradeJournal.Core;
using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Repositories;

namespace TradeJournal.Tests;

// Wspólny zestaw testów kontraktu IAccountRepository.
// Każda implementacja dziedziczy po tej klasie i podaje własne repozytorium.
public abstract class AccountRepositoryContractTests
{
    private IAccountRepository? _repository;

    protected abstract IAccountRepository CreateRepository();

    protected IAccountRepository Repository => _repository ??= CreateRepository();

    [Fact]
    public void GetAll_IsEmptyInitially()
    {
        Assert.Empty(Repository.GetAll());
    }

    [Fact]
    public void Add_ThenGetById_ReturnsAccountWithSameIdAndName()
    {
        var account = new Account("Topstep 50K", 50000m);

        Repository.Add(account);

        var loaded = Repository.GetById(account.Id);
        Assert.NotNull(loaded);
        Assert.Equal(account.Id, loaded!.Id);
        Assert.Equal("Topstep 50K", loaded.Name);
    }

    [Fact]
    public void GetById_ReturnsNullForUnknownId()
    {
        Assert.Null(Repository.GetById(Guid.NewGuid()));
    }

    [Fact]
    public void Add_ThrowsForDuplicateNameIgnoringCase()
    {
        Repository.Add(new Account("Topstep 50K", 50000m));

        Assert.Throws<DuplicateAccountException>(
            () => Repository.Add(new Account("topstep 50k", 50000m)));
    }

    [Fact]
    public void Add_ThrowsForDuplicateId()
    {
        var id = Guid.NewGuid();
        Repository.Add(new Account(id, "Konto A", 1000m));

        Assert.Throws<DuplicateAccountException>(
            () => Repository.Add(new Account(id, "Konto B", 1000m)));
    }

    [Fact]
    public void Add_ThrowsForNull()
    {
        Assert.Throws<ArgumentNullException>(() => Repository.Add(null!));
    }

    [Fact]
    public void GetAll_ReturnsAccountsSortedByName()
    {
        Repository.Add(new Account("Zeta", 1000m));
        Repository.Add(new Account("alfa", 1000m));
        Repository.Add(new Account("Beta", 1000m));

        var names = Repository.GetAll().Select(a => a.Name);

        Assert.Equal(new[] { "alfa", "Beta", "Zeta" }, names);
    }

    [Fact]
    public void Remove_DeletesExistingAccount()
    {
        var account = new Account("Konto A", 1000m);
        Repository.Add(account);

        var removed = Repository.Remove(account.Id);

        Assert.True(removed);
        Assert.Null(Repository.GetById(account.Id));
    }

    [Fact]
    public void Remove_ReturnsFalseForUnknownId()
    {
        Assert.False(Repository.Remove(Guid.NewGuid()));
    }

    [Fact]
    public void Remove_FreesTheNameForReuse()
    {
        var account = new Account("Konto A", 1000m);
        Repository.Add(account);
        Repository.Remove(account.Id);

        Repository.Add(new Account("Konto A", 2000m));

        Assert.Single(Repository.GetAll());
    }
        [Fact]
    public void GetById_ReturnsAccountWithNameAndStartingBalance()
    {
        var account = new Account("Konto A", 25000m);
        Repository.Add(account);

        var loaded = Repository.GetById(account.Id)!;

        Assert.Equal("Konto A", loaded.Name);
        Assert.Equal(25000m, loaded.StartingBalance);
    }

    [Fact]
    public void Add_KeepsEntriesWithDateAmountAndNote()
    {
        var account = new Account("Konto A", 1000m);
        var entry = new DayEntry(new DateOnly(2025, 1, 10), 123.45m);
        entry.SetNote("dobry dzień");
        account.AddEntry(entry);
        account.AddEntry(new DayEntry(new DateOnly(2025, 1, 11), -50m));

        Repository.Add(account);

        var loaded = Repository.GetById(account.Id)!;
        var first = loaded.Entries.OrderBy(e => e.Date).First();
        Assert.Equal(2, loaded.Entries.Count);
        Assert.Equal(new DateOnly(2025, 1, 10), first.Date);
        Assert.Equal(123.45m, first.Pnl);
        Assert.Equal("dobry dzień", first.Note);
    }

    [Fact]
    public void Update_PersistsEntriesAddedAfterAdd()
    {
        var account = new Account("Konto A", 1000m);
        Repository.Add(account);
        account.AddEntry(new DayEntry(new DateOnly(2025, 1, 10), 100m));

        Repository.Update(account);

        var loaded = Repository.GetById(account.Id)!;
        Assert.Single(loaded.Entries);
    }

    [Fact]
    public void Update_ThrowsForUnknownAccount()
    {
        var account = new Account("Konto A", 1000m);

        Assert.Throws<AccountNotFoundException>(() => Repository.Update(account));
    }

    [Fact]
    public void Update_ThrowsForNull()
    {
        Assert.Throws<ArgumentNullException>(() => Repository.Update(null!));
    }
}