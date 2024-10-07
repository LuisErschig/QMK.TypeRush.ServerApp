using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace QMK.TypeRush.ServerApp.Pages;

public partial class TypeRush
{
    private bool InputDisabled { get; set; } = true;
    private bool StartButtonDisabled { get; set; }
    private bool AuswertungButtonDisabled { get; set; } = true;
    private bool CountdownBoxDisabled { get; set; } = true;

    private const string TextToType = "Das ist ein Testtext mit hallo und dass als Beispiel.";
    private string userInput = "";
    private string countdown = "3";
    private double gameTimeElapsed;
    private int errors;

    private readonly Stopwatch stopwatch = new();
    private readonly Timer countdownTimer = new(1000);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        this.countdownTimer.Elapsed += CountdownElapsed;
    }

    private void StartCountdown()
    {
        this.StartButtonDisabled = true;
        this.CountdownBoxDisabled = false;
        this.countdown = "3";
        this.userInput = "";
        this.errors = 0;
        this.countdownTimer.Start();
    }

    private void CountdownElapsed(object? sender, ElapsedEventArgs e)
    {
        switch (this.countdown)
        {
            case "3":
                this.countdown = "2";
                break;
            case "2":
                this.countdown = "1";
                break;
            case "1":
                this.countdown = "GO";
                this.InputDisabled = false;
                this.countdownTimer.Stop();
                _ = SetFocusAsync();
                StartGame();
                break;
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
            this.AuswertungButtonDisabled = false;

            this.Logger.LogTrace($"Spiel ausgewertet. Anzahl Fehler: {this.errors}, benötigte Zeit: {this.gameTimeElapsed}.");
        }
    }

    private async Task EndGame()
    {
        this.stopwatch.Stop();
        this.InputDisabled = true;
        this.gameTimeElapsed = this.stopwatch.Elapsed.TotalSeconds;
        this.errors = await CalculateErrors();
    }

    private async Task<int> CalculateErrors()
    {
        await Task.Delay(100);

        return CountMistakesWithTolerance(TextToType, this.userInput);
    }

    private int CountMistakesWithTolerance(string expected, string input)
    {
        var mistakes = 0;
        var i = 0;
        var j = 0;

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
        for (var offset = 1; offset <= maxOffset; offset++)
        {
            if (i + offset < first.Length && j < second.Length && first[i + offset] == second[j])
            {
                return true;
            }
        }
        return false;
    }

    private void NavigateToEnterLeaderboard()
    {
        var uri = $"EnterLeaderboard?errors={this.errors}&time={this.gameTimeElapsed}";

        this.NavigationManager.NavigateTo(uri);
    }

    private void ReloadTypeRush()
    {
        this.NavigationManager.NavigateTo("typerush", true);
    }
}
