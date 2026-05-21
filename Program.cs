using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

// Copyright © Charlie Howard 2026 All rights reserved.

namespace CardPriceUpdaterGui
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        private static bool quiet = false;
        private static string? logFile = null;
        private static CurrencyMode currencyMode = CurrencyMode.AUTO;
        static void Log(string msg)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {msg}";
            if (!quiet)
                Console.WriteLine(line);
            if (!string.IsNullOrEmpty(logFile))
                File.AppendAllText(logFile, line + Environment.NewLine);
        }
        static void DrawProgress(int current, int total, string label)
        {
            if (quiet) return;
            int width = 30;
            double pct = (double)current / total;
            int filled = (int)(pct * width);
            string bar = "[" + new string('#', filled) + new string('-', width - filled) + "]";
            Console.Write($"\r{bar} {current}/{total} {label}   ");
        }
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            if (args.Length > 0)
            {
                AllocConsole();
                string? file = null;
                string? dir = null;
                bool recursive = false;
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i].ToLowerInvariant();
                    if (arg == "/f" && i + 1 < args.Length)
                        file = args[i + 1];
                    if (arg == "/d")
                    {
                        if (i + 1 < args.Length && !args[i + 1].StartsWith("/"))
                            dir = args[i + 1];
                        else
                            dir = AppDomain.CurrentDomain.BaseDirectory;
                    }
                    if (arg == "/r")
                        recursive = true;
                    if (arg == "/q" || arg == "/quiet" || arg == "/s" || arg == "/silent")
                        quiet = true;
                    if (arg == "/log" && i + 1 < args.Length)
                        logFile = args[i + 1];
                    if (arg == "/c" && i + 1 < args.Length)
                    {
                        var c = args[i + 1].ToLowerInvariant();
                        currencyMode = c switch
                        {
                            "p" => CurrencyMode.GBP,
                            "e" => CurrencyMode.EUR,
                            _ => CurrencyMode.AUTO
                        };
                    }
                }
                var updater = new PriceUpdater(
                    Log,
                    currencyMode,
                    PriceType.avg7
                );
                List<string> files = new();
                if (!string.IsNullOrWhiteSpace(file))
                {
                    files.Add(file);
                }
                else if (!string.IsNullOrWhiteSpace(dir))
                {
                    var opt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    files = Directory.GetFiles(dir, "*.xlsx", opt).ToList();
                }
                else
                {
                    Log("No input provided (/f or /d)");
                    return;
                }
                int total = files.Count;
                for (int i = 0; i < total; i++)
                {
                    DrawProgress(i + 1, total, Path.GetFileName(files[i]));
                    updater.Run(files[i]);
                }
                Console.WriteLine();
                Log("ALL FILES COMPLETE");
                DateTime end = DateTime.Now.AddSeconds(15);
                while (DateTime.Now < end)
                {
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        break;
                    }
                    Thread.Sleep(100);
                }
                return;
            }
            Application.Run(new CardmarketPriceUpdater());
        }
    }
}