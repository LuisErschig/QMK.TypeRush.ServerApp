using Newtonsoft.Json;
using QMK.TypeRush.ServerApp.DataObjects;

namespace QMK.TypeRush.ServerApp.Pages;

public partial class TextSelection
{
    private List<TextBib>? textBib;

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

            this.textBib = JsonConvert.DeserializeObject<List<TextBib>>(json);
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

    private async Task SetActiveText(TextBib activatedText)
    {
        var filePath = Path.Combine(this.Env.WebRootPath, "data", "text-selection.json");

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            foreach (var singleText in this.textBib!)
            {
                singleText.Aktiviert = singleText == activatedText;
            }

            var jsonToSave = JsonConvert.SerializeObject(this.textBib, Formatting.Indented);

            await File.WriteAllTextAsync(filePath, jsonToSave);
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
}
