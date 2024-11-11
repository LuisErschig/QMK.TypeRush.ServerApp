using System.ComponentModel.DataAnnotations;

namespace QMK.TypeRush.ServerApp.DataObjects;

public class LeaderboardEntries
{
    [Required(ErrorMessage = "Name ist erforderlich.")]
    public string Name { get; set; } = null!;

    public string? Class { get; set; }

    public double Time { get; set; }

    public int Errors { get; set; }
}
