using Cardmarket_Price_Updater.Core;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// Copyright © Charlie Howard 2026 All rights reserved.

namespace CardPriceUpdaterGui
{
    public class PriceUpdater
    {
        private readonly Action<string>? _log;
        private readonly CurrencyMode _mode;
        private readonly PriceType _priceType;
        private readonly int _maxRetries;
        private readonly double _retryDelaySeconds;
        private readonly int _backupRetentionCount;

        public PriceUpdater(
            Action<string>? log = null,
            CurrencyMode mode = CurrencyMode.AUTO,
            PriceType priceType = PriceType.avg30,
            int maxRetries = 3,
            double retryDelaySeconds = 2,
            int backupRetentionCount = 5)
        {
            _log = log;
            _mode = mode;
            _priceType = priceType;
            _maxRetries = maxRetries;
            _retryDelaySeconds = retryDelaySeconds;
            _backupRetentionCount = backupRetentionCount;
        }

        public PriceUpdater(Action<string>? log, AppConfig config, CurrencyMode? modeOverride = null, PriceType? priceTypeOverride = null)
            : this(
                log,
                modeOverride ?? config.CurrencyMode,
                priceTypeOverride ?? config.PriceType,
                config.MaxRetries,
                config.RetryDelaySeconds,
                config.BackupRetentionCount)
        {
        }

        private void Log(string msg)
        {
            _log?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");
        }

        private static readonly string[] PRICE_GUIDE_URLS =
        {
            "https://downloads.s3.cardmarket.com/productCatalog/priceGuide/price_guide_1.json",
            "https://downloads.s3.cardmarket.com/productCatalog/priceGuide/price_guide_3.json",
            "https://downloads.s3.cardmarket.com/productCatalog/priceGuide/price_guide_6.json"
        };

        private const string FX_API_URL =
            "https://api.frankfurter.app/latest?from=EUR&to=GBP,USD";

        public void Run(string workbookPath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Charlie Howard");
            Log($"File: {workbookPath}");
            Log("Starting...");

            CreateRotatingBackup(workbookPath);

            using var http = new HttpClient();

            var docs = RetryPolicy.RunAsync(
                () => DownloadPriceGuidesAsync(http, PRICE_GUIDE_URLS),
                _maxRetries, _retryDelaySeconds, _log, "Price guide download").Result;
            var map = BuildPriceMap(docs);

            decimal fx = 1m;
            bool convert = true;
            string targetSymbol = "GBP";

            switch (_mode)
            {
                case CurrencyMode.AUTO:
                    try
                    {
                        var rates = GetFxRatesAsync(http).Result;
                        fx = rates["GBP"];
                        targetSymbol = "GBP";
                        Log($"AUTO FX: 1 EUR = {fx} GBP");
                    }
                    catch
                    {
                        fx = 0.85m;
                        targetSymbol = "GBP";
                        Log("AUTO FX FAILED → fallback GBP 0.85");
                    }
                    break;

                case CurrencyMode.GBP:
                    fx = GetFxRatesAsync(http).Result["GBP"];
                    targetSymbol = "GBP";
                    Log("Forced GBP mode");
                    break;

                case CurrencyMode.USD:
                    fx = GetFxRatesAsync(http).Result["USD"];
                    targetSymbol = "USD";
                    Log("Forced USD mode");
                    break;

                case CurrencyMode.EUR:
                default:
                    convert = false;
                    Log("EUR mode selected");
                    break;
            }

            string temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");
            File.Copy(workbookPath, temp, true);
            using var pkg = new ExcelPackage(new FileInfo(temp));
            var ws = pkg.Workbook.Worksheets[0];
            var (row, priceCol, tsCol, pidCol, gameCol) = FindHeader(ws);
            Log($"Header found at row: {row}. Cols: Price={priceCol}, TS={tsCol}, PID={pidCol}, Game={gameCol}");

            if (row == -1)
            {
                Log("ERROR: Could not find required column headers in first 20 rows.");
                return;
            }

            int last = ws.Dimension.End.Row;
            int updatedCount = 0;
            var missing = new List<int>();

            for (int r = row + 1; r <= last; r++)
            {
                var pidText = ws.Cells[r, pidCol].Text.Trim();
                if (!int.TryParse(pidText, out int pid))
                    continue;

                if (!map.TryGetValue(pid, out var eur))
                {
                    missing.Add(pid);
                    continue;
                }
                decimal value = convert ? Math.Round(eur * fx, 2) : eur;
                ws.Cells[r, priceCol].Value = value;
                ws.Cells[r, tsCol].Value = DateTime.Now.ToString("yyyy-MM-dd");
                updatedCount++;
            }

            pkg.Save();
            File.Copy(temp, workbookPath, true);
            File.Delete(temp);

            if (missing.Count > 0)
            {
                Log($"{missing.Count} PID(s) not found in price guide: {string.Join(", ", missing.Distinct())}");
            }
            Log($"DONE. Updated {updatedCount} rows in {targetSymbol}{(convert ? "" : " (EUR, no conversion)")}.");
        }

        private void CreateRotatingBackup(string workbookPath)
        {
            string? dir = Path.GetDirectoryName(Path.GetFullPath(workbookPath));
            dir ??= Directory.GetCurrentDirectory();
            string backupDir = Path.Combine(dir, "Backups");
            Directory.CreateDirectory(backupDir);

            string fileName = Path.GetFileName(workbookPath);
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string backupPath = Path.Combine(backupDir, $"{fileName}.{timestamp}.bak");

            File.Copy(workbookPath, backupPath, true);
            Log($"Backup created: {backupPath}");

            if (_backupRetentionCount <= 0)
                return;

            var oldBackups = new DirectoryInfo(backupDir)
                .GetFiles($"{fileName}.*.bak")
                .OrderByDescending(f => f.CreationTimeUtc)
                .Skip(_backupRetentionCount)
                .ToList();

            foreach (var old in oldBackups)
            {
                try
                {
                    old.Delete();
                    Log($"Pruned old backup: {old.Name}");
                }
                catch (Exception ex)
                {
                    Log($"Could not prune old backup {old.Name}: {ex.Message}");
                }
            }
        }

        private async Task<JsonDocument[]> DownloadPriceGuidesAsync(HttpClient http, string[] urls)
        {
            var list = new List<JsonDocument>();
            foreach (var url in urls)
            {
                var json = await RetryPolicy.RunAsync(
                    () => http.GetStringAsync(url),
                    _maxRetries, _retryDelaySeconds, _log, $"GET {Path.GetFileName(url)}");
                list.Add(JsonDocument.Parse(json));
            }
            return list.ToArray();
        }

        private Dictionary<int, decimal> BuildPriceMap(JsonDocument[] docs)
        {
            var map = new Dictionary<int, decimal>();

            // Update these field names to match the actual JSON keys
            string fieldName = _priceType switch
            {
                PriceType.trend => "trend",
                PriceType.avg7 => "avg7",
                PriceType.avg30 => "avg30",
                _ => "avg30"
            };

            Log($"Using price field: {fieldName}");

            foreach (var doc in docs)
            {
                if (!doc.RootElement.TryGetProperty("priceGuides", out var guides))
                    continue;

                foreach (var e in guides.EnumerateArray())
                {
                    if (!e.TryGetProperty("idProduct", out var idEl))
                        continue;

                    // This will now successfully match "avg7", "avg30", etc.
                    if (!e.TryGetProperty(fieldName, out var priceEl))
                        continue;

                    if (!int.TryParse(idEl.ToString(), out int id))
                        continue;

                    if (!decimal.TryParse(
                            priceEl.ToString(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var price))
                        continue;

                    map[id] = price;
                }
            }

            return map;
        }

        private async Task<Dictionary<string, decimal>> GetFxRatesAsync(HttpClient http)
        {
            return await RetryPolicy.RunAsync(async () =>
            {
                var txt = await http.GetStringAsync(FX_API_URL);
                using var j = JsonDocument.Parse(txt);
                var ratesEl = j.RootElement.GetProperty("rates");
                return new Dictionary<string, decimal>
                {
                    ["GBP"] = ratesEl.GetProperty("GBP").GetDecimal(),
                    ["USD"] = ratesEl.GetProperty("USD").GetDecimal()
                };
            }, _maxRetries, _retryDelaySeconds, _log, "FX rate lookup");
        }

        private (int headerRow, int priceCol, int tsCol, int pidCol, int gameCol) FindHeader(ExcelWorksheet ws)
        {
            int maxRow = Math.Min(20, ws.Dimension.End.Row);
            int maxCol = ws.Dimension.End.Column;
            for (int r = 1; r <= maxRow; r++)
            {
                int price = -1, ts = -1, pid = -1, game = -1;
                for (int c = 1; c <= maxCol; c++)
                {
                    var h = ws.Cells[r, c].Text.ToLower();
                    if (h.Contains("card price")) price = c;
                    if (h.Contains("price updated")) ts = c;
                    if (h.Contains("cardmarket id")) pid = c;
                    if (h.Contains("game")) game = c;
                }
                if (price != -1 && ts != -1 && pid != -1 && game != -1)
                    return (r, price, ts, pid, game);
            }
            return (-1, -1, -1, -1, -1);
        }
    }
}
