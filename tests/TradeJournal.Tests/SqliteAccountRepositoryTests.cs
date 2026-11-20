using TradeJournal.Core;
using TradeJournal.Core.Repositories;
using TradeJournal.Data;

namespace TradeJournal.Tests;

// Dziedziczy cały kontrakt z AccountRepositoryContractTests,
// a tu dokłada tylko to, co dotyczy bazy w pliku.
public class SqliteAccountRepositoryTests : AccountRepositoryContractTests, IDisposable
{
    private readonly string _path =
        Path.Combine(Path.GetTempPath(), $"tradejournal-test-{Guid.NewGuid():N}.db");

    protected override IAccountRepository CreateRepository() =>
        new SqliteAccountRepository($"Data Source={_path};Pooling=False");

    [Fact]
    public void Data_SurvivesCreatingNewRepositoryOnTheSameFile()
    {
        var account = new Account("Konto A", 25000m);
        account.AddEntry(new DayEntry(new DateOnly(2025, 1, 10), 123.45m));
        Repository.Add(account);

        var reopened = CreateRepository();

        var loaded = Assert.Single(reopened.GetAll());
        Assert.Equal(account.Id, loaded.Id);
        Assert.Equal(25000m, loaded.StartingBalance);
        Assert.Equal(123.45m, Assert.Single(loaded.Entries).Pnl);
    }

    [Fact]
    public void Update_ReplacesEntriesInsteadOfDuplicatingThem()
    {
        var account = new Account("Konto A", 1000m);
        Repository.Add(account);
        account.AddEntry(new DayEntry(new DateOnly(2025, 1, 10), 100m));

        Repository.Update(account);
        Repository.Update(account);

        Assert.Single(Repository.GetById(account.Id)!.Entries);
    }

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }
}