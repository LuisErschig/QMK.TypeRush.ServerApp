using Microsoft.JSInterop;
using Newtonsoft.Json;
using QMK.TypeRush.ServerApp.DataObjects;

namespace QMK.TypeRush.ServerApp.Pages;

public partial class EnterLeaderboard
{
    private readonly LeaderboardEntries leaderboardEntries = new();

    private string activeLeaderboardPath = string.Empty;

    private bool parameterAreIncomplete;

    private string NameCssClass = "";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync(); 
        
        GetParametersFromUri();

        await GetCurrentLeaderboardPath();
    }

    private void GetParametersFromUri()
    {
        var uri = this.NavigationManager.ToAbsoluteUri(this.NavigationManager.Uri);
        var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

        if (queryParams.TryGetValue("errors", out var errors))
        {
            this.leaderboardEntries.Errors = Convert.ToInt32(errors);
        }
        else
        {
            this.parameterAreIncomplete = true;
        }

        if (queryParams.TryGetValue("time", out var time))
        {
            this.leaderboardEntries.Time = Convert.ToDouble(time);
        }
        else
        {
            this.parameterAreIncomplete = true;
        }
    }

    private async Task GetCurrentLeaderboardPath()
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

            var textId = textBib!.Single(e => e.Aktiviert).Id;

            this.activeLeaderboardPath = $"leaderboard-text-{textId}.json";
        }
        catch (FileNotFoundException)
        {
            this.Logger.LogError($"Leaderboard.json wurde nicht gefunden. Verwendeter Pfad: {filePath}");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Exception occured. ExceptionMessage: {ex.Message}");
        }
    }

    protected async Task CreateEntryAsync()
    {
        var filePath = Path.Combine(this.Env.WebRootPath, "data", this.activeLeaderboardPath);

        try
        {
            if (string.IsNullOrEmpty(this.leaderboardEntries.Name))
            {
                this.NameCssClass = string.IsNullOrEmpty(this.leaderboardEntries.Name) ? "input-error shake" : "";
                await Task.Delay(2000);
                this.NameCssClass = "";
                return;
            }

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
