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
}