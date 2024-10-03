using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using QMK.TypeRush.ServerApp.DataObjects;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace QMK.TypeRush.ServerApp.Pages;

public partial class TypeRush
{
    private bool InputDiasbled { get; set; } = true;
    private bool StartButtonDiasbled { get; set; } = false;
    private bool CountdownBoxDisabled { get; set; } = true;

    private readonly string textToType = "This is the text that you need to type correctly!";
    private string userInput = "";
    private string countdown = "3";
    private double gameTimeElapsed = 0;
    private string playerName = "";
    private string playerClass = "";
    private int errors = 0;

    private readonly Stopwatch stopwatch = new();
    private readonly Timer countdownTimer = new(1000);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        this.countdownTimer.Elapsed += CountdownElapsed;
    }

    private void StartCountdown()
    {
        this.StartButtonDiasbled = true;
        this.CountdownBoxDisabled = false;
        this.countdown = "3";
        this.userInput = "";
        this.errors = 0;
        this.countdownTimer.Start();
    }

    private void CountdownElapsed(object? sender, ElapsedEventArgs e)
    {
        if (this.countdown == "3")
        {
            this.countdown = "2";
        }
        else if (this.countdown == "2")
        {
            this.countdown = "1";
        }
        else if (this.countdown == "1")
        {
            this.countdown = "GO";
            this.InputDiasbled = false;
            this.countdownTimer.Stop();
            _ = SetFocus();
            StartGame();
        }

        InvokeAsync(StateHasChanged);
    }

    private async Task SetFocus()
    {
        await Task.Delay(1);
        await this.JsRuntime.InvokeVoidAsync("focusElement", "UserInputBox");
    }

    private void StartGame()
    {
        this.stopwatch.Restart();
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        this.stopwatch.Stop();
        this.gameTimeElapsed = this.stopwatch.Elapsed.TotalSeconds;
        this.errors = CalculateErrors();
    }

    private int CalculateErrors()
    {
        return LevenshteinDistance(this.textToType, this.userInput);
    }

    private int LevenshteinDistance(string s, string t)
    {
        int[,] d = new int[s.Length + 1, t.Length + 1];

        for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++) d[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return d[s.Length, t.Length];
    }

    private async Task SubmitScore()
    {
        if (!string.IsNullOrEmpty(this.playerName))
        {
            var entry = new LeaderboardEntries
            {
                Name = this.playerName,
                Class = this.playerClass,
                Time = this.gameTimeElapsed,
                Errors = this.errors
            };

            var filePath = Path.Combine(this.Env.WebRootPath, "data", "leaderboard.json");

            List<LeaderboardEntries> entries;
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                entries = JsonConvert.DeserializeObject<List<LeaderboardEntries>>(json) ?? new List<LeaderboardEntries>();
            }
            else
            {
                entries = new List<LeaderboardEntries>();
            }

            entries.Add(entry);

            var jsonToSave = JsonConvert.SerializeObject(entries);
            await File.WriteAllTextAsync(filePath, jsonToSave);
        }
    }
}
