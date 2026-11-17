namespace TradeJournal.Core.Exceptions;

public class AccountNotFoundException : Exception
{
    public AccountNotFoundException(Guid id)
        : base($"Nie znaleziono konta o identyfikatorze {id}.")
    {
    }
}