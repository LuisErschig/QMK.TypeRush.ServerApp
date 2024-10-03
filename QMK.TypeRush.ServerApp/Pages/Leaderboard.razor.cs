using Microsoft.JSInterop;
using Newtonsoft.Json;
using QMK.TypeRush.ServerApp.DataObjects;

namespace QMK.TypeRush.ServerApp.Pages;

//public partial class Leaderboard
//{
//    private List<LeaderboardEntry> leaderboardEntries = new();

//    private const string PageHeader = "Leaderboard";

//    private string currentSortColumn = "Errors";

//    private bool isSortAscending = true;

//    protected override async Task OnInitializedAsync()
//    {
//        await base.OnInitializedAsync();

//        this.leaderboardEntries = await leaderboardservice.GetAllEntriesAsync();

//        this.leaderboardEntries = this.leaderboardEntries.OrderBy(e => e.Errors)
//            .ThenBy(e => e.Time)
//            .ThenBy(e => e.Name)
//            .ToList();
//    }

//    public void SortTable(string columnName)
//    {
//        if (this.currentSortColumn != columnName)
//        {
//            this.currentSortColumn = columnName;
//            this.isSortAscending = true;
//        }

//        this.leaderboardEntries = columnName switch
//        {
//            "Errors" => this.isSortAscending
//                ? this.leaderboardEntries.OrderBy(e => e.Errors)
//                    .ThenBy(e => e.Time)
//                    .ThenBy(e => e.Name)
//                    .ThenBy(e => e.Class)
//                    .ToList()
//                : this.leaderboardEntries.OrderByDescending(e => e.Errors)
//                    .ThenByDescending(e => e.Time)
//                    .ThenByDescending(e => e.Name)
//                    .ThenByDescending(e => e.Class)
//                    .ToList(),
//            "Time" => this.isSortAscending
//                ? this.leaderboardEntries.OrderBy(e => e.Time)
//                    .ThenBy(e => e.Errors)
//                    .ThenBy(e => e.Name)
//                    .ThenBy(e => e.Class)
//                    .ToList()
//                : this.leaderboardEntries.OrderByDescending(e => e.Time)
//                    .ThenByDescending(e => e.Errors)
//                    .ThenByDescending(e => e.Name)
//                    .ThenByDescending(e => e.Class)
//                    .ToList(),
//            "Name" => this.isSortAscending
//                ? this.leaderboardEntries.OrderBy(e => e.Name)
//                    .ThenBy(e => e.Errors)
//                    .ThenBy(e => e.Time)
//                    .ThenBy(e => e.Class)
//                    .ToList()
//                : this.leaderboardEntries.OrderByDescending(e => e.Name)
//                    .ThenByDescending(e => e.Errors)
//                    .ThenByDescending(e => e.Time)
//                    .ThenByDescending(e => e.Class)
//                    .ToList(),
//            "Class" => this.isSortAscending
//                ? this.leaderboardEntries.OrderBy(e => e.Class)
//                    .ThenBy(e => e.Errors)
//                    .ThenBy(e => e.Time)
//                    .ThenBy(e => e.Name)
//                    .ToList()
//                : this.leaderboardEntries.OrderByDescending(e => e.Class)
//                    .ThenByDescending(e => e.Errors)
//                    .ThenByDescending(e => e.Time)
//                    .ThenByDescending(e => e.Name)
//                    .ToList(),
//            _ => this.leaderboardEntries.OrderBy(e => e.Errors)
//                .ThenBy(e => e.Time)
//                .ThenBy(e => e.Name)
//                .ThenBy(e => e.Class)
//                .ToList()
//        };
//    }

//    public string GetSortIcon(string columnName)
//    {
//        if (this.currentSortColumn != columnName)
//        {
//            return "";
//        }

//        return this.isSortAscending ? "▲" : "▼";
//    }

//    public async Task DeleteEntry(LeaderboardEntry entry)
//    {
//        var confirmed = await this.JsRuntime.InvokeAsync<bool>("confirm", $"Eintrag von {entry.Name} löschen?");

//        if (confirmed)
//        {
//            var successful = await this.leaderboardservice.DeleteEntryAsync(entry);

//            if (successful)
//            {
//                this.leaderboardEntries.Remove(entry);
//                logger.LogTrace($"Eintrags von {entry.Name} gelöscht.");
//            }
//        }
//        else
//        {
//            logger.LogTrace($"Löschen des Eintrags von {entry.Name} abgebrochen.");
//        }
//    }

//    public string GeneralizeTime(double time)
//    {
//        var timeString = Math.Round(time, 3).ToString().Replace(",", ".");

//        switch (timeString.Substring(timeString.IndexOf(".") + 1).Length)
//        {
//            case 0:
//                timeString = $"{timeString}000";
//                return timeString;
//            case 1:
//                timeString = $"{timeString}00";
//                return timeString;
//            case 2:
//                timeString = $"{timeString}0";
//                return timeString;
//            default:
//                return timeString;
//        }
//    }
//}

public partial class Leaderboard
{
    private List<LeaderboardEntries>? leaderboardEntries;

    private const string PageHeader = "Leaderboard";

    private const string RequestUri = "data/leaderboard.json";

    private string currentSortColumn = "Errors";

    private bool isSortAscending = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var filePath = Path.Combine(Env.WebRootPath, "data", "leaderboard.json");

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            var json = await File.ReadAllTextAsync(filePath);

            this.leaderboardEntries = JsonConvert.DeserializeObject<List<LeaderboardEntries>>(json);

            this.leaderboardEntries = this.leaderboardEntries?.OrderBy(e => e.Errors)
                .ThenBy(e => e.Time)
                .ThenBy(e => e.Name)
                .ToList();
        }
        catch (FileNotFoundException)
        {
            this.logger.LogTrace($"Leaderboard.json wurde nicht gefunden. Verwendeter Pfad: {filePath}");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"Exception occured. ExceptionMessage: {ex.Message}");
        }
    }

    public void SortTable(string columnName)
    {
        if (this.currentSortColumn == columnName)
        {
            this.isSortAscending = !this.isSortAscending;
        }
        else
        {
            this.currentSortColumn = columnName;
            this.isSortAscending = true;
        }

        this.leaderboardEntries = columnName switch
        {
            "Errors" => this.isSortAscending
                ? this.leaderboardEntries?.OrderBy(e => e.Errors)
                    .ThenBy(e => e.Time)
                    .ThenBy(e => e.Name)
                    .ThenBy(e => e.Class)
                    .ToList()
                : this.leaderboardEntries?.OrderByDescending(e => e.Errors)
                    .ThenByDescending(e => e.Time)
                    .ThenByDescending(e => e.Name)
                    .ThenByDescending(e => e.Class)
                    .ToList(),
            "Time" => this.isSortAscending
                ? this.leaderboardEntries?.OrderBy(e => e.Time)
                    .ThenBy(e => e.Errors)
                    .ThenBy(e => e.Name)
                    .ThenBy(e => e.Class)
                    .ToList()
                : this.leaderboardEntries?.OrderByDescending(e => e.Time)
                    .ThenByDescending(e => e.Errors)
                    .ThenByDescending(e => e.Name)
                    .ThenByDescending(e => e.Class)
                    .ToList(),
            "Name" => this.isSortAscending
                ? this.leaderboardEntries?.OrderBy(e => e.Name)
                    .ThenBy(e => e.Errors)
                    .ThenBy(e => e.Time)
                    .ThenBy(e => e.Class)
                    .ToList()
                : this.leaderboardEntries?.OrderByDescending(e => e.Name)
                    .ThenByDescending(e => e.Errors)
                    .ThenByDescending(e => e.Time)
                    .ThenByDescending(e => e.Class)
                    .ToList(),
            "Class" => this.isSortAscending
                ? this.leaderboardEntries?.OrderBy(e => e.Class)
                    .ThenBy(e => e.Errors)
                    .ThenBy(e => e.Time)
                    .ThenBy(e => e.Name)
                    .ToList()
                : this.leaderboardEntries?.OrderByDescending(e => e.Class)
                    .ThenByDescending(e => e.Errors)
                    .ThenByDescending(e => e.Time)
                    .ThenByDescending(e => e.Name)
                    .ToList(),
            _ => this.leaderboardEntries?.OrderBy(e => e.Errors)
                .ThenBy(e => e.Time)
                .ThenBy(e => e.Name)
                .ThenBy(e => e.Class)
                .ToList()
        };
    }

    public string GetSortIcon(string columnName)
    {
        if (this.currentSortColumn != columnName)
        {
            return "";
        }

        return this.isSortAscending ? "▲" : "▼";
    }

    public string GeneralizeTime(double time)
    {
        var timeString = Math.Round(time, 3).ToString().Replace(",", ".");

        switch (timeString.Substring(timeString.IndexOf(".") + 1).Length)
        {
            case 0:
                timeString = $"{timeString}000";
                return timeString;
            case 1:
                timeString = $"{timeString}00";
                return timeString;
            case 2:
                timeString = $"{timeString}0";
                return timeString;
            default:
                return timeString;
        }
    }

    public async Task DeleteEntry(LeaderboardEntries entry)
    {
        try
        {
            var confirmed = await this.JsRuntime.InvokeAsync<bool>("confirm", $"Eintrag von {entry.Name} löschen?");

            if (confirmed == false)
            {
                this.logger.LogTrace($"Löschen des Eintrags von {entry.Name} abgebrochen.");
                return;
            }

            if (entry == null || this.leaderboardEntries == null)
            {
                throw new ArgumentNullException();
            }

            this.leaderboardEntries.Remove(entry);

            var filePath = Path.Combine(Env.WebRootPath, "data", "leaderboard.json");
            var json = JsonConvert.SerializeObject(this.leaderboardEntries);
            await File.WriteAllTextAsync(filePath, json);

            this.logger.LogTrace($"Eintrags von {entry.Name} gelöscht.");
        }
        catch (ArgumentNullException)
        {
            this.logger.LogError($"Übergebener Eintrag oder Leaderboard darf nicht NULL sein.");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"Exception occured. ExceptionMessage: {ex.Message}");
        }
    }
}
