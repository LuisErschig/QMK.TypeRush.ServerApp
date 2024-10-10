using Newtonsoft.Json;
using QMK.TypeRush.ServerApp.DataObjects;

namespace QMK.TypeRush.ServerApp.Pages;

public partial class EnterLeaderboard
{
    private readonly LeaderboardEntries leaderboardEntries = new();

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

    protected async Task CreateEntryAsync()
    {
        if (!string.IsNullOrEmpty(this.leaderboardEntries.Name))
        {
            var filePath = Path.Combine(this.Env.WebRootPath, "data", "leaderboard.json");

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

    protected void CancelAsync()
    {
        this.NavigationManager.NavigateTo("type-rush");
    }
}
