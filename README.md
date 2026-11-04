# Trade_journal

Dziennik tradera: kalendarz dziennego PnL, statystyki i śledzenie reguł kont fundowanych.cs

> Status: wersja w budowie (projekt zaliczeniowy).

## Funkcje

Zrobione:
- wpis dnia handlowego (`DayEntry`): data, PnL, notatka, walidacja danych

Planowane do v1.0.0:
- konta i dzienne wpisy PnL, kalendarz
- statystyki: win rate, średni wygrany/przegrany dzień, profit factor, drawdown, equity curve
- załączniki: obrazy (Ctrl+V) i linki TradingView
- silnik reguł prop firm (preset Topstep + inn)

## Technologia

C# (.NET 10), ASP.NET Core Razor Pages, SQLite (od v0.2.0), xUnit.

## Struktura

```
src/TradeJournal.Core   logika domenowa (bez zależności od strony)
src/TradeJournal.Web    aplikacja webowa (Razor Pages)
tests/TradeJournal.Tests  testy jednostkowe (xUnit)
```

## Budowanie i testy

Wymagany .NET SDK 10.

```
dotnet build
dotnet test
```

## Uruchomienie

```
dotnet run --project src/TradeJournal.Web
```

## Koncepcje obiektowe

| Koncepcja | Klasy i pliki | Uzasadnienie |
|---|---|---|
| Hermetyzacja | `DayEntry.cs` | stan zmienia się tylko przez kontrolowane metody |
| Obsługa błędów | `InvalidDayEntryException.cs`, `DayEntry.cs` | jawne sygnalizowanie niepoprawnych danych |

## Rejestr użycia LLM

| Plik / fragment | Narzędzie | Do czego użyto i jak zweryfikowano |
|---|---|---|
| `.gitignore` | Claude | doradztwo (polecenie `dotnet new gitignore`), plik wygenerowany przez dotnet |
| `.gitattributes` | Claude | doradztwo (propozycja ustawienia), plik napisany według podpowiedzi |
| solucja, projekty Core/Web/Tests | Claude | doradztwo (podział na projekty), pliki wygenerowane szablonami `dotnet new` |
| `InvalidDayEntryException.cs` | Claude | wygenerowany,  |
| `DayEntry.cs` | Claude | wygenerowany; zweryfikowany 8 testami |
| `DayEntryTests.cs` | Claude | wygenerowany,  |
|`InInvalidDayEntryException.cs`| Claude | wygenerowany|
|`InvalidAccountException.cs`| Claude | wygenerowany, błąd składni poprawiono ręcznie|
|`DayEntryTets.cs`| Claude | wygenerowany|