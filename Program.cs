using Cardmarket_Price_Updater.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AutoUpdaterDotNET; // 1. Added the namespace

namespace CardPriceUpdaterGui
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;
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
            // 2. Start the update check immediately
            AutoUpdater.Start("https://raw.githubusercontent.com/ProfessorShroom/Cardmarket-Price-Updater/refs/heads/main/update.xml");

            ApplicationConfiguration.Initialize();

            if (args.Length > 0)
            {
                // ... (your existing CLI logic remains unchanged)
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i].ToLowerInvariant();
                    if (arg == "/q" || arg == "/quiet" || arg == "/s" || arg == "/silent")
                        quiet = true;
                    if (arg == "/log" && i + 1 < args.Length)
                        logFile = args[i + 1];
                }

                if (!quiet)
                {
                    if (!AttachConsole(ATTACH_PARENT_PROCESS))
                        AllocConsole();
                }

                string? file = null;
                string? dir = null;
                bool recursive = false;

                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i].ToLowerInvariant();
                    if (arg == "/f" && i + 1 < args.Length) file = args[i + 1];
                    if (arg == "/d")
                    {
                        dir = (i + 1 < args.Length && !args[i + 1].StartsWith("/")) ? args[i + 1] : AppDomain.CurrentDomain.BaseDirectory;
                    }
                    if (arg == "/r") recursive = true;
                    if (arg == "/c" && i + 1 < args.Length)
                    {
                        currencyMode = args[i + 1].ToLowerInvariant() switch { "p" => CurrencyMode.GBP, "e" => CurrencyMode.EUR, _ => CurrencyMode.AUTO };
                    }
                }

                var updater = new PriceUpdater(Log, currencyMode, PriceType.avg7);
                List<string> files = new();
                if (!string.IsNullOrWhiteSpace(file)) files.Add(file);
                else if (!string.IsNullOrWhiteSpace(dir)) files = Directory.GetFiles(dir, "*.xlsx", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                else { Log("No input provided (/f or /d)"); return; }

                int total = files.Count;
                for (int i = 0; i < total; i++)
                {
                    DrawProgress(i + 1, total, Path.GetFileName(files[i]));
                    updater.Run(files[i]);
                }

                Log("ALL FILES COMPLETE");

                if (!quiet)
                {
                    Console.WriteLine("Press any key to exit...");
                    DateTime end = DateTime.Now.AddSeconds(15);
                    while (DateTime.Now < end) { if (Console.KeyAvailable) { Console.ReadKey(true); break; } Thread.Sleep(100); }
                }
                return;
            }

            Application.Run(new CardmarketPriceUpdater());
        }
    }
}