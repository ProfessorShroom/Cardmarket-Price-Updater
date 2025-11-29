using OfficeOpenXml;               // EPPlus
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardPriceUpdaterGui
{
    public partial class CardmarketPriceUpdater : Form
    {
        // Use an array of guides 1,3,6
        private static readonly string[] PRICE_GUIDE_URLS = new[]
        {
            "https://downloads.s3.cardmarket.com/productCatalog/priceGuide/price_guide_1.json",
            "https://downloads.s3.cardmarket.com/productCatalog/priceGuide/price_guide_3.json",
            "https://downloads.s3.cardmarket.com/productCatalog/priceGuide/price_guide_6.json"
        };

        private const string FX_API_URL = "https://api.exchangerate.host/latest?base=EUR&symbols=GBP";
        private const int MAX_HEADER_SCAN_ROWS = 20;

        public CardmarketPriceUpdater()
        {
            InitializeComponent();
            startButton.Click += StartButton_Click;
            outputBox.Font = new System.Drawing.Font("Consolas", 10);
            outputBox.ReadOnly = true;

            // Show version link at bottom of GUI
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
            var versionLabel = new LinkLabel()
            {
                Text = $"Version {version}",
                AutoSize = true,
                Left = 10,
                Top = this.ClientSize.Height - 25,
                LinkColor = System.Drawing.Color.Black
            };
            versionLabel.Click += (_, __) => System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/ProfessorShroom/Cardmarket-Price-Updater/blob/main/README.md",
                UseShellExecute = true
            });
            this.Controls.Add(versionLabel);
        }

        private async void StartButton_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog()
            {
                Title = "Select your collection workbook (.xlsx)",
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            var path = dlg.FileName;
            AppendLog($"Selected: {path}");

            startButton.Enabled = false;
            try
            {
                await RunUpdateWithTempCopyAsync(path);
            }
            catch (Exception ex)
            {
                AppendLog($"Fatal error: {ex}");
                MessageBox.Show($"An unexpected error occurred:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                startButton.Enabled = true;
                AppendLog("Operation finished.");
            }
        }

        private void AppendLog(string text)
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}{Environment.NewLine}";
            if (outputBox.InvokeRequired)
                outputBox.Invoke((Action)(() => outputBox.AppendText(line)));
            else
                outputBox.AppendText(line);
        }

        private async Task RunUpdateWithTempCopyAsync(string workbookPath)
        {
            AppendLog("Starting update workflow...");

            using var http = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };

            // Download price guides (1,3,6)
            AppendLog("Downloading Cardmarket price guides (MTG, Yu-Gi-Oh!, Pokémon)...");
            JsonDocument[] priceDocs = Array.Empty<JsonDocument>();
            try
            {
                priceDocs = await DownloadPriceGuidesAsync(http, PRICE_GUIDE_URLS);
                AppendLog($"Downloaded {priceDocs.Length} price guide documents.");
            }
            catch (Exception ex)
            {
                AppendLog($"Failed to download price guides: {ex.Message}");
                return;
            }

            Dictionary<int, decimal> priceMap;
            try
            {
                priceMap = BuildPriceMap(priceDocs);
                AppendLog($"Built price map for {priceMap.Count} products.");
            }
            finally
            {
                foreach (var d in priceDocs) d?.Dispose();
            }

            AppendLog("Fetching EUR->GBP rate...");
            decimal fx = await GetEurToGbpRateAsync(http);
            AppendLog($"FX: 1 EUR = {fx.ToString(CultureInfo.InvariantCulture)} GBP");

            string tempPath = Path.Combine(Path.GetTempPath(), $"cardupdate_{Guid.NewGuid():N}.xlsx");
            File.Copy(workbookPath, tempPath, true);
            AppendLog($"Copied original workbook to temp file: {tempPath}");

            var sw = Stopwatch.StartNew();
            try
            {
                OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("ProfessorShroom");

                using var package = new ExcelPackage(new FileInfo(tempPath));
                var ws = package.Workbook.Worksheets[0] as ExcelWorksheet;
                if (ws == null)
                {
                    AppendLog("No worksheet found in workbook.");
                    return;
                }

                var (headerRow, priceCol, tsCol, pidCol, gameCol) = FindHeaderRowAndCols(ws);
                if (headerRow == -1 || priceCol == -1 || tsCol == -1 || pidCol == -1 || gameCol == -1)
                {
                    AppendLog("Couldn't find required columns (Card Price, Price Updated, Cardmarket ID, Game).");
                    return;
                }

                AppendLog($"Using header row {headerRow} (price col {priceCol}, ts col {tsCol}, pid col {pidCol}, game col {gameCol})");

                int updatedRows = 0, skippedRows = 0;
                string dateStr = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                int lastRow = ws.Dimension?.End.Row ?? headerRow;

                AppendLog($"Processing rows {headerRow + 1}..{lastRow} (~{Math.Max(0, lastRow - headerRow)})");

                int progressCounter = 0;
                for (int row = headerRow + 1; row <= lastRow; row++)
                {
                    progressCounter++;

                    var pidText = ws.Cells[row, pidCol].Text.Trim();
                    var gameText = ws.Cells[row, gameCol].Text.Trim().ToLowerInvariant();

                    // Skip empty rows
                    if (string.IsNullOrEmpty(pidText) && string.IsNullOrEmpty(gameText))
                        continue;

                    // Normalize game text: remove punctuation/accents
                    string cleanGame = Regex.Replace(gameText.Normalize(NormalizationForm.FormD), @"[^a-z0-9]", "");

                    int guideIndex = cleanGame switch
                    {
                        "mtg" => 0,
                        "yugioh" => 1,
                        "pokemon" => 2,
                        _ => -1
                    };

                    if (guideIndex == -1)
                    {
                        skippedRows++;
                        AppendLog($"Row {row}: unknown game '{gameText}', skipped.");
                        continue;
                    }

                    if (!int.TryParse(pidText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pid))
                    {
                        skippedRows++;
                        AppendLog($"Row {row}: invalid Cardmarket ID, skipped.");
                        continue;
                    }

                    if (!priceMap.TryGetValue(pid, out decimal priceEur))
                    {
                        skippedRows++;
                        AppendLog($"Row {row}: price not found for product ID {pid}, skipped.");
                        continue;
                    }

                    bool useGbp = true;
                    var rbGbp = this.Controls.ContainsKey("rbGbp") ? this.Controls["rbGbp"] as RadioButton : null;
                    var rbEur = this.Controls.ContainsKey("rbEur") ? this.Controls["rbEur"] as RadioButton : null;
                    if (rbGbp != null && rbEur != null)
                    {
                        useGbp = rbGbp.Checked;
                    }

                    decimal priceValue;
                    string currencyFormat;
                    if (useGbp)
                    {
                        priceValue = Math.Round(priceEur * fx, 2, MidpointRounding.AwayFromZero);
                        currencyFormat = "£#,##0.00";
                    }
                    else
                    {
                        priceValue = priceEur;
                        currencyFormat = "€#,##0.00";
                    }

                    ws.Cells[row, priceCol].Value = priceValue;
                    ws.Cells[row, priceCol].Style.Numberformat.Format = currencyFormat;
                    ws.Cells[row, tsCol].Value = dateStr;

                    updatedRows++;
                    if ((progressCounter % 500) == 0)
                        AppendLog($"Processed {progressCounter} rows. Elapsed: {sw.Elapsed.TotalSeconds:N1}s");
                }

                AppendLog($"Finished processing rows in {sw.Elapsed.TotalSeconds:N2}s. Updated {updatedRows}, skipped {skippedRows}.");

                AppendLog("Saving updated workbook...");
                package.Save();
                AppendLog("Workbook saved successfully.");

                File.Copy(tempPath, workbookPath, true);
                AppendLog("Original workbook replaced.");
            }
            catch (Exception ex)
            {
                AppendLog($"Failed during workbook update: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                        AppendLog("Deleted temp file.");
                    }
                }
                catch (Exception e)
                {
                    AppendLog($"Warning: couldn't delete temp file: {e.Message}");
                }
            }
        }

        private async Task<JsonDocument[]> DownloadPriceGuidesAsync(HttpClient http, string[] urls)
        {
            var downloadTasks = new List<Task<(string url, string content)>>();
            foreach (var url in urls)
                downloadTasks.Add(Task.Run(async () => (url, await http.GetStringAsync(url))));

            var results = await Task.WhenAll(downloadTasks);

            var docs = new List<JsonDocument>();
            foreach (var r in results)
            {
                try
                {
                    docs.Add(JsonDocument.Parse(r.content));
                    AppendLog($"Parsed price guide from {r.url}");
                }
                catch (Exception ex)
                {
                    AppendLog($"Failed to parse JSON from {r.url}: {ex.Message}");
                }
            }

            return docs.ToArray();
        }

        private Dictionary<int, decimal> BuildPriceMap(JsonDocument[] docs)
        {
            var map = new Dictionary<int, decimal>();
            int total = 0, skipped = 0;

            foreach (var doc in docs)
            {
                if (doc == null) continue;
                if (!doc.RootElement.TryGetProperty("priceGuides", out var guides))
                    continue;

                foreach (var entry in guides.EnumerateArray())
                {
                    total++;
                    if (!entry.TryGetProperty("idProduct", out var pidEl)) { skipped++; continue; }

                    int pid = pidEl.ValueKind switch
                    {
                        JsonValueKind.Number when pidEl.TryGetInt32(out var n) => n,
                        JsonValueKind.String when int.TryParse(pidEl.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var s) => s,
                        _ => -1
                    };
                    if (pid == -1) { skipped++; continue; }

                    if (!entry.TryGetProperty("trend", out var trendEl)) { skipped++; continue; }

                    decimal trend = trendEl.ValueKind switch
                    {
                        JsonValueKind.Number when trendEl.TryGetDecimal(out var n) => n,
                        JsonValueKind.String when decimal.TryParse(trendEl.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var s) => s,
                        _ => -1
                    };
                    if (trend == -1) { skipped++; continue; }

                    map[pid] = trend;
                }
            }

            AppendLog($"Parsed price guide entries across documents: total={total}, accepted={map.Count}, skipped={skipped}");
            return map;
        }

        private async Task<decimal> GetEurToGbpRateAsync(HttpClient http)
        {
            try
            {
                var txt = await http.GetStringAsync(FX_API_URL);
                using var j = JsonDocument.Parse(txt);
                if (j.RootElement.TryGetProperty("rates", out var rates) &&
                    rates.TryGetProperty("GBP", out var gbpEl))
                {
                    if (gbpEl.ValueKind == JsonValueKind.Number && gbpEl.TryGetDecimal(out var rate))
                        return rate;
                    if (gbpEl.ValueKind == JsonValueKind.String && decimal.TryParse(gbpEl.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var s))
                        return s;
                }
            }
            catch (Exception e)
            {
                AppendLog($"FX fetch failed, falling back to 0.85: {e.Message}");
            }
            return 0.85m;
        }

        private (int headerRow, int priceCol, int tsCol, int pidCol, int gameCol) FindHeaderRowAndCols(ExcelWorksheet ws)
        {
            int headerRow = -1, priceCol = -1, tsCol = -1, pidCol = -1, gameCol = -1;
            int maxRow = Math.Min(MAX_HEADER_SCAN_ROWS, ws.Dimension?.End.Row ?? 0);
            int maxCol = ws.Dimension?.End.Column ?? 0;

            for (int r = 1; r <= maxRow; r++)
            {
                int _price = -1, _ts = -1, _pid = -1, _game = -1;
                for (int c = 1; c <= maxCol; c++)
                {
                    string hdr = (ws.Cells[r, c].Text ?? string.Empty).Trim().ToLowerInvariant();
                    if (hdr.Contains("card price") && hdr.Contains("£")) _price = c;
                    else if (hdr.Contains("price updated")) _ts = c;
                    else if (hdr.Contains("cardmarket id")) _pid = c;
                    else if (hdr.Contains("game")) _game = c;
                }

                if (_pid != -1 && (_price != -1 || _ts != -1) && _game != -1)
                {
                    headerRow = r; priceCol = _price; tsCol = _ts; pidCol = _pid; gameCol = _game;
                    break;
                }
            }

            return (headerRow, priceCol, tsCol, pidCol, gameCol);
        }
    }
}