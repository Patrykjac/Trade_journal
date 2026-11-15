namespace TradeJournal.Core.Repositories;

public interface IAccountRepository
{
    void Add(Account account);

    Account? GetById(Guid id);

    IReadOnlyList<Account> GetAll();

    bool Remove(Guid id);
}