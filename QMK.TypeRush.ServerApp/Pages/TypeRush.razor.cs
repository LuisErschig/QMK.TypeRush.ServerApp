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

    private readonly string textToType = "Das ist ein Testtext mit hallo und dass als Beispiel.";
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
            _ = SetFocusAsync();
            StartGame();
        }

        InvokeAsync(StateHasChanged);
    }

    private async Task SetFocusAsync()
    {
        await Task.Delay(5);
        await this.JsRuntime.InvokeVoidAsync("focusElement", "UserInputBox");
    }

    private void StartGame()
    {
        this.stopwatch.Restart();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await EndGame();
        }
    }

    private async Task EndGame()
    {
        this.stopwatch.Stop();
        this.InputDiasbled = true;
        this.gameTimeElapsed = this.stopwatch.Elapsed.TotalSeconds;
        this.errors = await CalculateErrors();
    }

    private async Task<int> CalculateErrors()
    {
        await Task.Delay(5);

        return CountMistakesWithTolerance(this.textToType, this.userInput);
    }

    private int CountMistakesWithTolerance(string expected, string input)
    {
        int mistakes = 0;
        int i = 0, j = 0;

        while (i < expected.Length && j < input.Length)
        {
            if (expected[i] != input[j])
            {
                mistakes++;

                if (IsMatchWithinNextNCharacters(expected, input, i, j, 3))
                {
                    i++;
                }
                else if (IsMatchWithinNextNCharacters(input, expected, j, i, 3))
                {
                    j++;
                }
            }

            i++;
            j++;
        }

        if (i < expected.Length)
        {
            mistakes += expected.Length - i;
        }
        else if (j < input.Length)
        {
            mistakes += input.Length - j;
        }

        return mistakes;
    }

    private bool IsMatchWithinNextNCharacters(string first, string second, int i, int j, int maxOffset)
    {
        for (int offset = 1; offset <= maxOffset; offset++)
        {
            if (i + offset < first.Length && j < second.Length && first[i + offset] == second[j])
            {
                return true;
            }
        }
        return false;
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
