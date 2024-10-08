using Newtonsoft.Json;
using QMK.TypeRush.ServerApp.DataObjects;

namespace QMK.TypeRush.ServerApp.Pages;

public partial class TextSelection
{
    private List<TextBib>? texts;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var filePath = Path.Combine(this.Env.WebRootPath, "data", "text-selection.json");

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            var json = await File.ReadAllTextAsync(filePath);

            this.texts = JsonConvert.DeserializeObject<List<TextBib>>(json);
        }
        catch (FileNotFoundException)
        {
            this.Logger.LogError($"text-selection.json wurde nicht gefunden. Verwendeter Pfad: {filePath}");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Exception occured. ExceptionMessage: {ex.Message}");
        }
    }

    private async Task SetActiveText(int id)
    {
        foreach (var text in texts)
        {
            text.Aktiviert = text.Id == id;
        }
    }
}
