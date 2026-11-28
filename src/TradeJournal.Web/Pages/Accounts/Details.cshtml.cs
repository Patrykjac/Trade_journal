using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradeJournal.Core;
using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Repositories;

namespace TradeJournal.Web.Pages.Accounts;

public class DetailsModel : PageModel
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly IAccountRepository _repository;

    public DetailsModel(IAccountRepository repository)
    {
        _repository = repository;
    }

    public Account Account { get; private set; } = null!;

    public IReadOnlyList<DayEntry> Entries { get; private set; } = Array.Empty<DayEntry>();

    public string? Error { get; private set; }

    [BindProperty]
    public string? DateText { get; set; }

    [BindProperty]
    public string? PnlText { get; set; }

    [BindProperty]
    public string? Note { get; set; }

    public IActionResult OnGet(Guid id)
    {
        if (!Load(id))
            return NotFound();

        DateText = DateOnly.FromDateTime(DateTime.Today)
            .ToString(DateFormat, CultureInfo.InvariantCulture);

        return Page();
    }

    public IActionResult OnPost(Guid id)
    {
        if (!Load(id))
            return NotFound();

        if (!DateOnly.TryParseExact(
                DateText, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return ShowError("Podaj poprawną datę.");
        }

        if (!MoneyParser.TryParse(PnlText, out var pnl))
        {
            return ShowError("PnL musi być liczbą, np. 250 lub -120,50.");
        }

        try
        {
            var entry = new DayEntry(date, pnl);
            entry.SetNote(Note);

            Account.AddEntry(entry);
            _repository.Update(Account);

            return RedirectToPage(new { id });
        }
        catch (Exception ex) when (ex is InvalidDayEntryException or DuplicateEntryException)
        {
            return ShowError(ex.Message);
        }
    }

    private bool Load(Guid id)
    {
        var account = _repository.GetById(id);

        if (account is null)
            return false;

        Account = account;
        Entries = account.Entries.OrderByDescending(e => e.Date).ToList();

        return true;
    }

    private IActionResult ShowError(string message)
    {
        Error = message;
        return Page();
    }
}