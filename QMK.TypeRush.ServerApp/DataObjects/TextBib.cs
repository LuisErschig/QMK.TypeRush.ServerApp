namespace QMK.TypeRush.ServerApp.DataObjects;

public class TextBib
{
    public int Id { get; set; }

    public string Schwierigkeit { get; set; } = null!;

    public bool Aktiviert { get; set; }

    public string Text { get; set; } = null!;
}
