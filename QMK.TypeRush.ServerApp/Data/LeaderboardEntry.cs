using System.ComponentModel.DataAnnotations;

namespace QMK.TypeRush.ServerApp.Data;

public class LeaderboardEntry
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Class { get; set; }

    public double Time { get; set; }

    public int Errors { get; set; }
}
