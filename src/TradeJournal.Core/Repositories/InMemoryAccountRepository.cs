using TradeJournal.Core.Exceptions;

namespace TradeJournal.Core.Repositories;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly Dictionary<Guid, Account> _accounts = new();

    public void Add(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        var nameTaken = _accounts.Values.Any(a =>
            string.Equals(a.Name, account.Name, StringComparison.OrdinalIgnoreCase));

        if (nameTaken || _accounts.ContainsKey(account.Id))
            throw new DuplicateAccountException(account.Name);

        _accounts.Add(account.Id, account);
    }
        public void Update(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (!_accounts.ContainsKey(account.Id))
            throw new AccountNotFoundException(account.Id);

        _accounts[account.Id] = account;
    }

    public Account? GetById(Guid id) =>
        _accounts.TryGetValue(id, out var account) ? account : null;

    public IReadOnlyList<Account> GetAll() =>
        _accounts.Values
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public bool Remove(Guid id) => _accounts.Remove(id);
}