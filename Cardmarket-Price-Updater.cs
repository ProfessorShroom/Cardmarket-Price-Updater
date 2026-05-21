using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardPriceUpdaterGui
{
    public partial class CardmarketPriceUpdater : Form
    {
        private readonly LinkLabel versionLabel;

        public CardmarketPriceUpdater()
        {
            InitializeComponent();

            // ---------------- PRICE TYPE DROPDOWN ----------------
            cmbPriceType.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbPriceType.DataSource = Enum.GetValues(typeof(PriceType))
                .Cast<PriceType>()
                .Select(x => new
                {
                    Value = x,
                    Text = GetPriceTypeText(x)
                })
                .ToList();

            cmbPriceType.DisplayMember = "Text";
            cmbPriceType.ValueMember = "Value";
            cmbPriceType.SelectedValue = PriceType.avg30;

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
                LinkColor = Color.White
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

            Resize += (_, __) =>
            {
                versionLabel.Top = this.ClientSize.Height - 25;
            };

            startButton.Click += StartButton_Click;
        }

        // ---------------- DISPLAY TEXT ----------------
        private string GetPriceTypeText(PriceType type)
        {
            return type switch
            {
                PriceType.trend => "Trend",
                PriceType.avg7 => "7-Day Average",
                PriceType.avg30 => "30-Day Average",
                _ => type.ToString()
            };
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
                AppendLog("Starting Update");
                AppendLog($"File: {dlg.FileName}");

                // ✅ READ UI VALUE ON UI THREAD FIRST
                var selectedPriceType = (PriceType)cmbPriceType.SelectedValue!;

                // Now safe to go background
                await Task.Run(() =>
                {
                    new PriceUpdater(
                        AppendLog,
                        CurrencyMode.AUTO,
                        selectedPriceType
                    ).Run(dlg.FileName);
                });

                AppendLog("Prices Updated!");
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

        // ---------------- CLOSE BUTTON ----------------

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}