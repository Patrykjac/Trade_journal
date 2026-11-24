using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradeJournal.Core;
using TradeJournal.Core.Exceptions;
using TradeJournal.Core.Repositories;

namespace TradeJournal.Web.Pages.Accounts;

public class IndexModel : PageModel
{
    private readonly IAccountRepository _repository;

    public IndexModel(IAccountRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Account> Accounts { get; private set; } = Array.Empty<Account>();

    public string? Error { get; private set; }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string StartingBalanceText { get; set; } = string.Empty;

    public void OnGet()
    {
        Accounts = _repository.GetAll();
    }

    public IActionResult OnPost()
    {
        if (!MoneyParser.TryParse(StartingBalanceText, out var balance))
        {
            return ShowError("Saldo początkowe musi być liczbą, np. 50000 lub 50000,50.");
        }

        try
        {
            _repository.Add(new Account(Name, balance));
            return RedirectToPage();
        }
        catch (Exception ex) when (ex is InvalidAccountException or DuplicateAccountException)
        {
            return ShowError(ex.Message);
        }
    }

    private IActionResult ShowError(string message)
    {
        Error = message;
        Accounts = _repository.GetAll();
        return Page();
    }
}