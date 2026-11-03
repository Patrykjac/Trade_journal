namespace TradeJournal.Core.Exceptions;

public class DuplicateEntryException : Exception
{
    public DuplicateEntryException(DateOnly date)
        : base($"Wpis dla dnia {date:yyy-MM-dd} już istnieje na tym koncie.")
    {
    }
}