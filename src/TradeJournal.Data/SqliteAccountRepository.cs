using System.Globalization;
using Microsoft.Data.Sqlite;
using TradeJournal.Core;
using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Repositories;

namespace TradeJournal.Data;

public class SqliteAccountRepository : IAccountRepository
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly string _connectionString;

    public SqliteAccountRepository(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
        CreateTables();
    }

    public void Add(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        if (IsTaken(connection, transaction, account))
            throw new DuplicateAccountException(account.Name);

        Execute(connection, transaction,
            "INSERT INTO accounts (id, name, starting_balance) VALUES ($id, $name, $balance)",
            ("$id", account.Id.ToString()),
            ("$name", account.Name),
            ("$balance", FormatDecimal(account.StartingBalance)));

        InsertEntries(connection, transaction, account);

        transaction.Commit();
    }

    public void Update(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        if (!Exists(connection, transaction, account.Id))
            throw new AccountNotFoundException(account.Id);

        Execute(connection, transaction,
            "DELETE FROM day_entries WHERE account_id = $id",
            ("$id", account.Id.ToString()));

        InsertEntries(connection, transaction, account);

        transaction.Commit();
    }

    public Account? GetById(Guid id) =>
        LoadAll().FirstOrDefault(a => a.Id == id);

    public IReadOnlyList<Account> GetAll() =>
        LoadAll()
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public bool Remove(Guid id)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        Execute(connection, transaction,
            "DELETE FROM day_entries WHERE account_id = $id",
            ("$id", id.ToString()));

        var removed = Execute(connection, transaction,
            "DELETE FROM accounts WHERE id = $id",
            ("$id", id.ToString()));

        transaction.Commit();

        return removed > 0;
    }

    private void CreateTables()
    {
        using var connection = OpenConnection();

        Execute(connection, null, """
            CREATE TABLE IF NOT EXISTS accounts (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                starting_balance TEXT NOT NULL
            )
            """);

        Execute(connection, null, """
            CREATE TABLE IF NOT EXISTS day_entries (
                account_id TEXT NOT NULL,
                date TEXT NOT NULL,
                pnl TEXT NOT NULL,
                note TEXT NOT NULL,
                PRIMARY KEY (account_id, date)
            )
            """);
    }

    private List<Account> LoadAll()
    {
        using var connection = OpenConnection();

        var headers = new List<(Guid Id, string Name, decimal Balance)>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT id, name, starting_balance FROM accounts";
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                headers.Add((
                    Guid.Parse(reader.GetString(0)),
                    reader.GetString(1),
                    ParseDecimal(reader.GetString(2))));
            }
        }

        var accounts = new List<Account>();

        foreach (var header in headers)
        {
            var account = new Account(header.Id, header.Name, header.Balance);

            using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT date, pnl, note FROM day_entries WHERE account_id = $id ORDER BY date";
            command.Parameters.AddWithValue("$id", header.Id.ToString());

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var date = DateOnly.ParseExact(
                    reader.GetString(0), DateFormat, CultureInfo.InvariantCulture);

                var entry = new DayEntry(date, ParseDecimal(reader.GetString(1)));
                entry.SetNote(reader.GetString(2));
                account.AddEntry(entry);
            }

            accounts.Add(account);
        }

        return accounts;
    }

    private static void InsertEntries(
        SqliteConnection connection, SqliteTransaction transaction, Account account)
    {
        foreach (var entry in account.Entries)
        {
            Execute(connection, transaction,
                "INSERT INTO day_entries (account_id, date, pnl, note) VALUES ($id, $date, $pnl, $note)",
                ("$id", account.Id.ToString()),
                ("$date", entry.Date.ToString(DateFormat, CultureInfo.InvariantCulture)),
                ("$pnl", FormatDecimal(entry.Pnl)),
                ("$note", entry.Note));
        }
    }

    private static bool IsTaken(
        SqliteConnection connection, SqliteTransaction transaction, Account account)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT id, name FROM accounts";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var sameId = Guid.Parse(reader.GetString(0)) == account.Id;
            var sameName = string.Equals(
                reader.GetString(1), account.Name, StringComparison.OrdinalIgnoreCase);

            if (sameId || sameName)
                return true;
        }

        return false;
    }

    private static bool Exists(
        SqliteConnection connection, SqliteTransaction transaction, Guid id)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(1) FROM accounts WHERE id = $id";
        command.Parameters.AddWithValue("$id", id.ToString());

        return Convert.ToInt64(command.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static int Execute(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);

        return command.ExecuteNonQuery();
    }

    private static string FormatDecimal(decimal value) =>
        value.ToString(CultureInfo.InvariantCulture);

    private static decimal ParseDecimal(string text) =>
        decimal.Parse(text, CultureInfo.InvariantCulture);
}