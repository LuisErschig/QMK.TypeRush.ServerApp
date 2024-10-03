namespace QMK.TypeRush.ServerApp.DataObjects;

public class LeaderboardEntries
{
    public string Name { get; set; } = null!;

    public string? Class { get; set; }

    public double Time { get; set; }

    public int Errors { get; set; }
}
