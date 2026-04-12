using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

// Copyright © Charlie Howard 2026 All rights reserved.

namespace CardPriceUpdaterGui
{
    public partial class CardmarketPriceUpdater : Form
    {
        private readonly LinkLabel versionLabel;

        public CardmarketPriceUpdater()
        {
            InitializeComponent();

            // ---------------- LOGGING BOX ----------------
            outputBox.Font = new Font("Consolas", 10);
            outputBox.ReadOnly = true;

            // ---------------- VERSION LABEL ----------------
            var version = typeof(CardmarketPriceUpdater)
                .Assembly
                .GetName()
                .Version!
                .ToString(4);

            versionLabel = new LinkLabel
            {
                Text = $"Version {version}",
                AutoSize = true,
                Left = 10,
                Top = this.ClientSize.Height - 25,
                LinkColor = Color.Black
            };

            versionLabel.Click += (_, __) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://professorshroom.com/projects/Cardmarket_Price_Updater/#changelog",
                    UseShellExecute = true
                });
            };

            Controls.Add(versionLabel);

            // keep pinned to bottom on resize
            this.Resize += (_, __) =>
            {
                versionLabel.Top = this.ClientSize.Height - 25;
            };

            startButton.Click += StartButton_Click;
        }

        // ---------------- LOGGING ----------------
        private void AppendLog(string msg)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {msg}";

            if (outputBox.InvokeRequired)
            {
                outputBox.Invoke(new Action(() =>
                    outputBox.AppendText(line + Environment.NewLine)));
            }
            else
            {
                outputBox.AppendText(line + Environment.NewLine);
            }
        }

        // ---------------- START BUTTON ----------------
        private async void StartButton_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            startButton.Enabled = false;

            try
            {
                outputBox.Clear();

                AppendLog("=== STARTING UPDATE ===");
                AppendLog($"File: {dlg.FileName}");

                await Task.Run(() =>
                {
                    new PriceUpdater(AppendLog, CurrencyMode.AUTO)
                        .Run(dlg.FileName);
                });

                AppendLog("=== PROCESS COMPLETE ===");
            }
            catch (Exception ex)
            {
                AppendLog("ERROR: " + ex);
            }
            finally
            {
                startButton.Enabled = true;
            }
        }
    }
}