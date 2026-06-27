using Cardmarket_Price_Updater.Core;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        public PriceUpdater(
            Action<string>? log = null,
            CurrencyMode mode = CurrencyMode.AUTO,
            PriceType priceType = PriceType.avg30)
        {
            _log = log;
            _mode = mode;
            _priceType = priceType;
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
            "https://api.frankfurter.app/latest?from=EUR&to=GBP";
        public void Run(string workbookPath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Charlie Howard");
            Log($"File: {workbookPath}");
            Log("Starting...");
            string backupPath = workbookPath + ".bak";
            File.Copy(workbookPath, backupPath, true);
            Log($"Backup created: {backupPath}");
            using var http = new HttpClient();
            var docs = DownloadPriceGuidesAsync(http, PRICE_GUIDE_URLS).Result;
            var map = BuildPriceMap(docs);
            decimal fx = 1m;
            bool useGbp = true;
            if (_mode == CurrencyMode.AUTO)
            {
                try
                {
                    fx = GetEurToGbpRateAsync(http).Result;
                    Log($"AUTO FX: 1 EUR = {fx} GBP");
                }
                catch
                {
                    fx = 0.85m;
                    Log("AUTO FX FAILED → fallback GBP 0.85");
                }
            }
            else if (_mode == CurrencyMode.GBP)
            {
                fx = GetEurToGbpRateAsync(http).Result;
                Log("Forced GBP mode");
            }
            else
            {
                useGbp = false;
                Log("EUR mode selected");
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
            for (int r = row + 1; r <= last; r++)
            {
                var pidText = ws.Cells[r, pidCol].Text.Trim();
                if (!int.TryParse(pidText, out int pid))
                    continue;

                if (!map.TryGetValue(pid, out var eur))
                {
                    Log($"Row {r}: PID {pid} not found in price guide.");
                    continue;
                }
                decimal value = useGbp ? Math.Round(eur * fx, 2) : eur;
                ws.Cells[r, priceCol].Value = value;
                ws.Cells[r, tsCol].Value = DateTime.Now.ToString("yyyy-MM-dd");
                updatedCount++;
            }
            pkg.Save();
            File.Copy(temp, workbookPath, true);
            File.Delete(temp);
            Log($"DONE. Updated {updatedCount} rows.");
        }
        private async Task<JsonDocument[]> DownloadPriceGuidesAsync(HttpClient http, string[] urls)
        {
            var list = new List<JsonDocument>();
            foreach (var url in urls)
            {
                var json = await http.GetStringAsync(url);
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
        private async Task<decimal> GetEurToGbpRateAsync(HttpClient http)
        {
            var txt = await http.GetStringAsync(FX_API_URL);
            using var j = JsonDocument.Parse(txt);
            return j.RootElement.GetProperty("rates").GetProperty("GBP").GetDecimal();
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