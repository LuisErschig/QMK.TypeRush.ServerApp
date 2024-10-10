using iText.Kernel.Colors;
using iText.Kernel.Geom;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using QMK.TypeRush.ServerApp.DataObjects;
using System.Text;
using System.Xml.Linq;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Path = System.IO.Path;

namespace QMK.TypeRush.ServerApp.Pages;

public partial class Leaderboard
{
    private List<LeaderboardEntries>? leaderboardEntries;

    private string currentSortColumn = "Errors";

    private bool isSortAscending = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var filePath = Path.Combine(this.Env.WebRootPath, "data", "leaderboard.json");

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            var json = await File.ReadAllTextAsync(filePath);

            this.leaderboardEntries = JsonConvert.DeserializeObject<List<LeaderboardEntries>>(json);

            this.leaderboardEntries = this.leaderboardEntries?.OrderBy(e => e.Errors)
                .ThenBy(e => e.Time)
                .ThenBy(e => e.Name)
                .ToList();
        }
        catch (FileNotFoundException)
        {
            this.Logger.LogError($"Leaderboard.json wurde nicht gefunden. Verwendeter Pfad: {filePath}");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Exception occured. ExceptionMessage: {ex.Message}");
        }
    }

    public void SortTable(string columnName)
    {
        if (this.currentSortColumn == columnName)
        {
            this.isSortAscending = !this.isSortAscending;
        }
        else
        {
            this.currentSortColumn = columnName;
            this.isSortAscending = true;
        }

        this.leaderboardEntries = columnName switch
        {
            "Errors" => this.isSortAscending
                ? this.leaderboardEntries?.OrderBy(e => e.Errors)
                    .ThenBy(e => e.Time)
                    .ThenBy(e => e.Name)
                    .ThenBy(e => e.Class)
                    .ToList()
                : this.leaderboardEntries?.OrderByDescending(e => e.Errors)
                    .ThenByDescending(e => e.Time)
                    .ThenByDescending(e => e.Name)
                    .ThenByDescending(e => e.Class)
                    .ToList(),
            "Time" => this.isSortAscending
                ? this.leaderboardEntries?.OrderBy(e => e.Time)
                    .ThenBy(e => e.Errors)
                    .ThenBy(e => e.Name)
                    .ThenBy(e => e.Class)
                    .ToList()
                : this.leaderboardEntries?.OrderByDescending(e => e.Time)
                    .ThenByDescending(e => e.Errors)
                    .ThenByDescending(e => e.Name)
                    .ThenByDescending(e => e.Class)
                    .ToList(),
            "Name" => this.isSortAscending
                ? this.leaderboardEntries?.OrderBy(e => e.Name)
                    .ThenBy(e => e.Errors)
                    .ThenBy(e => e.Time)
                    .ThenBy(e => e.Class)
                    .ToList()
                : this.leaderboardEntries?.OrderByDescending(e => e.Name)
                    .ThenByDescending(e => e.Errors)
                    .ThenByDescending(e => e.Time)
                    .ThenByDescending(e => e.Class)
                    .ToList(),
            "Class" => this.isSortAscending
                ? this.leaderboardEntries?.OrderBy(e => e.Class)
                    .ThenBy(e => e.Errors)
                    .ThenBy(e => e.Time)
                    .ThenBy(e => e.Name)
                    .ToList()
                : this.leaderboardEntries?.OrderByDescending(e => e.Class)
                    .ThenByDescending(e => e.Errors)
                    .ThenByDescending(e => e.Time)
                    .ThenByDescending(e => e.Name)
                    .ToList(),
            _ => this.leaderboardEntries?.OrderBy(e => e.Errors)
                .ThenBy(e => e.Time)
                .ThenBy(e => e.Name)
                .ThenBy(e => e.Class)
                .ToList()
        };
    }

    public string GetSortIcon(string columnName)
    {
        if (this.currentSortColumn != columnName)
        {
            return "";
        }

        return this.isSortAscending ? "▲" : "▼";
    }

    public string GeneralizeTime(double time)
    {
        var timeString = Math.Round(time, 3).ToString().Replace(",", ".");

        switch (timeString.Substring(timeString.IndexOf(".", StringComparison.Ordinal) + 1).Length)
        {
            case 0:
                timeString = $"{timeString}000";
                return timeString;
            case 1:
                timeString = $"{timeString}00";
                return timeString;
            case 2:
                timeString = $"{timeString}0";
                return timeString;
            default:
                return timeString;
        }
    }

    public async Task ToggleDropdownAsync(string containerId)
    {
        await this.JsRuntime.InvokeVoidAsync("toggleDropdown", containerId);
    }

    public async Task DeleteEntry(LeaderboardEntries entry)
    {
        try
        {
            var confirmed = await this.JsRuntime.InvokeAsync<bool>("confirm", $"Eintrag von {entry.Name} löschen?");

            if (confirmed == false)
            {
                this.Logger.LogTrace($"Löschen des Eintrags von {entry.Name} abgebrochen.");
                return;
            }

            if (entry == null || this.leaderboardEntries == null)
            {
                throw new ArgumentNullException();
            }

            this.leaderboardEntries.Remove(entry);

            var filePath = Path.Combine(this.Env.WebRootPath, "data", "leaderboard.json");
            var json = JsonConvert.SerializeObject(this.leaderboardEntries, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);

            this.Logger.LogTrace($"Eintrags von {entry.Name} gelöscht.");
        }
        catch (ArgumentNullException)
        {
            this.Logger.LogError($"Übergebener Eintrag oder Leaderboard darf nicht NULL sein.");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"Exception occured. ExceptionMessage: {ex.Message}");
        }
    }

    private async Task DownloadXml()
    {
        const string fileName = "leaderboard.xml";

        if (this.leaderboardEntries == null)
        {
            await this.JsRuntime.InvokeVoidAsync("alert", "Noch keine Einträge im Leaderboard");
            return;
        }

        var xmlContent = ConvertJsonToXml();

        await this.JsRuntime.InvokeVoidAsync("downloadFile", fileName, xmlContent);
    }

    private string ConvertJsonToXml()
    {
        var xDocument = new XDocument(
            new XElement("Leaderboard",
                from entry in this.leaderboardEntries
                select new XElement("Entry",
                    new XElement("Name", entry.Name),
                    new XElement("Class", !string.IsNullOrWhiteSpace(entry.Class) ? entry.Class : "N / A"),
                    new XElement("Time", entry.Time),
                    new XElement("Errors", entry.Errors)
                )
            )
        );

        return xDocument.ToString();
    }

    private async Task DownloadCsv()
    {
        const string fileName = "leaderboard.csv";

        if (this.leaderboardEntries == null)
        {
            await this.JsRuntime.InvokeVoidAsync("alert", "Noch keine Einträge im Leaderboard");
            return;
        }

        var csvContent = ConvertToCsv();

        await this.JsRuntime.InvokeVoidAsync("downloadFile", fileName, csvContent);
    }

    private string ConvertToCsv()
    {
        var csv = new StringBuilder();
        csv.AppendLine("Name,Class,Time,Errors");

        foreach (var entry in this.leaderboardEntries!)
        {
            csv.AppendLine($"{entry.Name},{(!string.IsNullOrWhiteSpace(entry.Class) ? entry.Class : "N / A")},{entry.Time},{entry.Errors}");
        }

        return csv.ToString();
    }

    private async Task DownloadExcel()
    {
        const string fileName = "leaderboard.xlsx";

        if (this.leaderboardEntries == null)
        {
            await this.JsRuntime.InvokeVoidAsync("alert", "Noch keine Einträge im Leaderboard");
            return;
        }

        var excelBytes = CreateExcelFile();

        using var stream = new MemoryStream(excelBytes);

        await this.JsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, new DotNetStreamReference(stream: stream));
    }

    private byte[] CreateExcelFile()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage();

        var worksheet = package.Workbook.Worksheets.Add("Leaderboard");

        worksheet.Cells[1, 1].Value = "Name";
        worksheet.Cells[1, 2].Value = "Class";
        worksheet.Cells[1, 3].Value = "Time (sec)";
        worksheet.Cells[1, 4].Value = "Errors";

        using (var range = worksheet.Cells[1, 1, 1, 4])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        var row = 2;
        foreach (var entry in this.leaderboardEntries!)
        {
            worksheet.Cells[row, 1].Value = entry.Name;
            worksheet.Cells[row, 2].Value = !string.IsNullOrWhiteSpace(entry.Class) ? entry.Class : "N / A";
            worksheet.Cells[row, 3].Value = entry.Time;
            worksheet.Cells[row, 4].Value = entry.Errors;
            row++;
        }

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    private async Task DownloadPdf()
    {
        const string fileName = "leaderboard.pdf";

        if (this.leaderboardEntries == null)
        {
            await this.JsRuntime.InvokeVoidAsync("alert", "Noch keine Einträge im Leaderboard");
            return;
        }

        var pdfBytes = CreatePdfFile();

        using var stream = new MemoryStream(pdfBytes);
        await this.JsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, new DotNetStreamReference(stream: stream));
    }

    private byte[] CreatePdfFile()
    {
        using var memoryStream = new MemoryStream();

        using (var pdfWriter = new PdfWriter(memoryStream))
        {
            using (var pdfDocument = new PdfDocument(pdfWriter))
            {
                pdfDocument.SetDefaultPageSize(PageSize.A4.Rotate());
                
                var document = new Document(pdfDocument);
                document.SetMargins(30, 20, 30, 20);

                document.Add(new Paragraph("Leaderboard").SetFontSize(20).SetBold());

                var table = new Table(UnitValue.CreatePercentArray(new float[] { 25, 25, 25, 25 }))
                    .UseAllAvailableWidth();

                table.SetMarginTop(10F);
                table.SetBorder(new SolidBorder(Color.ConvertRgbToCmyk(new DeviceRgb(0,0,0)), 1.5F));

                var headerCell1 = new Cell().Add(new Paragraph("Name").SetFontSize(13).SetBold().SetUnderline(1F, -2F)).SetBorderBottom(new SolidBorder(Color.ConvertRgbToCmyk(new DeviceRgb(80, 120, 255)), 1.5F)).SetPaddings(10F, 20F, 1F, 10F);
                var headerCell2 = new Cell().Add(new Paragraph("Class").SetFontSize(13).SetBold().SetUnderline(1F, -2F)).SetBorderBottom(new SolidBorder(Color.ConvertRgbToCmyk(new DeviceRgb(80, 120, 255)), 1.5F)).SetPaddings(10F, 20F, 1F, 10F);
                var headerCell3 = new Cell().Add(new Paragraph("Time (sec)").SetFontSize(13).SetBold().SetUnderline(1F, -2F)).SetBorderBottom(new SolidBorder(Color.ConvertRgbToCmyk(new DeviceRgb(80, 120, 255)), 1.5F)).SetPaddings(10F, 20F, 1F, 10F);
                var headerCell4 = new Cell().Add(new Paragraph("Errors").SetFontSize(13).SetBold().SetUnderline(1F, -2F)).SetBorderBottom(new SolidBorder(Color.ConvertRgbToCmyk(new DeviceRgb(80, 120, 255)), 1.5F)).SetPaddings(10F, 20F, 1F, 10F);

                table.AddHeaderCell(headerCell1);
                table.AddHeaderCell(headerCell2);
                table.AddHeaderCell(headerCell3);
                table.AddHeaderCell(headerCell4);

                const float cellPaddingTop = 2F;
                const float cellPaddingRight = 10F;
                const float cellPaddingBottom = 2F;
                const float cellPaddingLeft = 10F;

                foreach (var entry in this.leaderboardEntries!)
                {
                    var cellName = new Cell().Add(new Paragraph(entry.Name)).SetPaddings(cellPaddingTop, cellPaddingRight, cellPaddingBottom, cellPaddingLeft);
                    var cellClass = new Cell().Add(new Paragraph(!string.IsNullOrWhiteSpace(entry.Class) ? entry.Class : "N / A")).SetPaddings(cellPaddingTop, cellPaddingRight, cellPaddingBottom, cellPaddingLeft);
                    var cellTime = new Cell().Add(new Paragraph(entry.Time.ToString("F3"))).SetPaddings(cellPaddingTop, cellPaddingRight, cellPaddingBottom, cellPaddingLeft);
                    var cellErrors = new Cell().Add(new Paragraph(entry.Errors.ToString())).SetPaddings(cellPaddingTop, cellPaddingRight, cellPaddingBottom, cellPaddingLeft);

                    table.AddCell(cellName);
                    table.AddCell(cellClass);
                    table.AddCell(cellTime);
                    table.AddCell(cellErrors);
                }

                document.Add(table);
                document.Close();
            }
        }
        return memoryStream.ToArray();
    }
}
