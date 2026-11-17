using TradeJournal.Core.Repositories;

namespace TradeJournal.Tests;

public class InMemoryAccountRepositoryTests : AccountRepositoryContractTests
{
    protected override IAccountRepository CreateRepository() =>
        new InMemoryAccountRepository();
}