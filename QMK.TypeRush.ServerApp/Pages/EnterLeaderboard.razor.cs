using Microsoft.JSInterop;
using Newtonsoft.Json;
using QMK.TypeRush.ServerApp.DataObjects;

namespace QMK.TypeRush.ServerApp.Pages;

public partial class EnterLeaderboard
{
    private readonly LeaderboardEntries leaderboardEntries = new();

    private string activeLeaderboardPath = string.Empty;

    protected override void OnInitialized()
    {
        var uri = this.NavigationManager.ToAbsoluteUri(this.NavigationManager.Uri);
        var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (queryParams.TryGetValue("errors", out var errors))
        {
            this.leaderboardEntries.Errors = Convert.ToInt32(errors);
        }

        if (queryParams.TryGetValue("time", out var time))
        {
            this.leaderboardEntries.Time = Convert.ToDouble(time);
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var textId = await GetSelectedText();

        if (textId == -1)
        {
            return;
        }

        this.activeLeaderboardPath = $"leaderboard-text-{textId}.json";
    }

    private async Task<int> GetSelectedText()
    {
        var filePath = Path.Combine(this.Env.WebRootPath, "data", "text-selection.json");

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            var json = await File.ReadAllTextAsync(filePath);

            var textBib = JsonConvert.DeserializeObject<List<TextBib>>(json);

            return textBib!.Single(e => e.Aktiviert).Id;
        }
        catch (FileNotFoundException)
        {
            this.Logger.LogError($"Leaderboard.json wurde nicht gefunden. Verwendeter Pfad: {filePath}");
            return -1;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Exception occured. ExceptionMessage: {ex.Message}");
            return -1;
        }
    }

    protected async Task CreateEntryAsync()
    {
        var filePath = Path.Combine(this.Env.WebRootPath, "data", this.activeLeaderboardPath);

        try
        {
            if (!string.IsNullOrEmpty(this.leaderboardEntries.Name))
            {
                List<LeaderboardEntries> entries;
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    entries = JsonConvert.DeserializeObject<List<LeaderboardEntries>>(json) ?? [];
                }
                else
                {
                    entries = [];
                }

                entries.Add(this.leaderboardEntries);

                var jsonToSave = JsonConvert.SerializeObject(entries, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, jsonToSave);
            }

            this.NavigationManager.NavigateTo("type-rush");
        }
        catch (FileNotFoundException)
        {
            this.Logger.LogError($"Leaderboard.json wurde nicht gefunden. Verwendeter Pfad: {filePath}");
            await this.JsRuntime.InvokeVoidAsync("alert", "Ein Fehler ist aufgetreten. Es wurde kein Datensatz erstellt.");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Exception occured. ExceptionMessage: {ex.Message}");
            await this.JsRuntime.InvokeVoidAsync("alert", "Ein Fehler ist aufgetreten. Es wurde kein Datensatz erstellt.");
        }
    }

    protected void CancelAsync()
    {
        this.NavigationManager.NavigateTo("type-rush");
    }
}
