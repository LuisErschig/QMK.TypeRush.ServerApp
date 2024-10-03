using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using QMK.TypeRush.ServerApp.DataObjects;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace QMK.TypeRush.ServerApp.Pages;

public partial class TypeRush
{
    protected bool InputDiasbled { get; set; } = true;


    protected string TextToType = "This is the text that you need to type correctly!";
    protected string UserInput = "";
    protected bool GameStarted = false;
    protected bool GameFinished = false;
    protected int Countdown = 0;
    protected double GameTimeElapsed = 0;
    protected string PlayerName = "";
    protected string PlayerClass = "";
    protected int Errors = 0;

    private Stopwatch stopwatch = new();
    private Timer countdownTimer;

    protected override void OnInitialized()
    {
        this.countdownTimer = new Timer(1000);
        this.countdownTimer.Elapsed += CountdownElapsed;
    }

    protected async Task StartCountdown()
    {
        this.Countdown = 3;
        this.GameStarted = false;
        this.GameFinished = false;
        this.UserInput = "";
        this.Errors = 0;
        this.countdownTimer.Start();
    }

    private void CountdownElapsed(object sender, ElapsedEventArgs e)
    {
        if (this.Countdown > 0)
        {
            this.Countdown--;
        }
        else
        {
            this.countdownTimer.Stop();
            StartGame();
        }
        InvokeAsync(StateHasChanged);
    }

    protected void StartGame()
    {
        this.GameStarted = true;
        this.stopwatch.Restart();
    }

    protected void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            EndGame();
        }
    }

    protected void EndGame()
    {
        this.GameStarted = false;
        this.GameFinished = true;
        this.stopwatch.Stop();
        this.GameTimeElapsed = this.stopwatch.Elapsed.TotalSeconds;
        this.Errors = CalculateErrors();
    }

    private int CalculateErrors()
    {
        return LevenshteinDistance(this.TextToType, this.UserInput);
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

    protected async Task SubmitScore()
    {
        if (!string.IsNullOrEmpty(this.PlayerName))
        {
            var entry = new LeaderboardEntries
            {
                Name = this.PlayerName,
                Class = this.PlayerClass,
                Time = this.GameTimeElapsed,
                Errors = this.Errors
            };

            var filePath = Path.Combine(this.Env.WebRootPath, "data", "leaderboard.json");

            List<LeaderboardEntries> entries;
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                entries = JsonSerializer.Deserialize<List<LeaderboardEntries>>(json) ?? new List<LeaderboardEntries>();
            }
            else
            {
                entries = new List<LeaderboardEntries>();
            }

            entries.Add(entry);

            var jsonToSave = JsonSerializer.Serialize(entries);
            await File.WriteAllTextAsync(filePath, jsonToSave);
        }
    }
}
