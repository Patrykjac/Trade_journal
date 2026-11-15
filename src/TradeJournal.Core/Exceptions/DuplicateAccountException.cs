namespace TradeJournal.Core.Exceptions;

public class DuplicateAccountException : Exception
{
    public DuplicateAccountException(string name)
        : base($"Konto o nazwie '{name}' już istnieje.")
    {
    }
}