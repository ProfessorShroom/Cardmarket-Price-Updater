// Copyright © Charlie Howard 2026 All rights reserved.

using System.Drawing;
using System.Windows.Forms;

namespace CardPriceUpdaterGui
{
    partial class CardmarketPriceUpdater
    {
        private System.ComponentModel.IContainer components = null;

        private Button startButton;
        private TextBox outputBox;
        private RadioButton checkBoxGBP;
        private RadioButton checkBoxEUR;
        private ComboBox cmbPriceType;
        private Panel topPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            startButton = new Button();
            outputBox = new TextBox();
            checkBoxGBP = new RadioButton();
            checkBoxEUR = new RadioButton();
            cmbPriceType = new ComboBox();
            topPanel = new Panel();
            closeButton = new Button();
            topPanel.SuspendLayout();
            SuspendLayout();
            // 
            // startButton
            // 
            startButton.BackColor = Color.FromArgb(60, 60, 60);
            startButton.Cursor = Cursors.Hand;
            startButton.FlatAppearance.BorderSize = 0;
            startButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 45, 45);
            startButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(85, 85, 85);
            startButton.FlatStyle = FlatStyle.Flat;
            startButton.ForeColor = Color.White;
            startButton.Location = new Point(12, 12);
            startButton.Name = "startButton";
            startButton.Size = new Size(150, 32);
            startButton.TabIndex = 0;
            startButton.Text = "Get Prices (Browse Files)";
            startButton.UseVisualStyleBackColor = false;
            // 
            // outputBox
            // 
            outputBox.BackColor = Color.FromArgb(45, 45, 45);
            outputBox.BorderStyle = BorderStyle.None;
            outputBox.ForeColor = Color.White;
            outputBox.Location = new Point(12, 61);
            outputBox.Multiline = true;
            outputBox.Name = "outputBox";
            outputBox.ScrollBars = ScrollBars.Vertical;
            outputBox.Size = new Size(760, 368);
            outputBox.TabIndex = 1;
            // 
            // checkBoxGBP
            // 
            checkBoxGBP.Appearance = Appearance.Button;
            checkBoxGBP.BackColor = Color.FromArgb(60, 60, 60);
            checkBoxGBP.Checked = true;
            checkBoxGBP.Cursor = Cursors.Hand;
            checkBoxGBP.FlatAppearance.BorderSize = 0;
            checkBoxGBP.FlatAppearance.CheckedBackColor = Color.FromArgb(90, 90, 90);
            checkBoxGBP.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 75, 75);
            checkBoxGBP.FlatStyle = FlatStyle.Flat;
            checkBoxGBP.ForeColor = Color.White;
            checkBoxGBP.Location = new Point(170, 14);
            checkBoxGBP.Name = "checkBoxGBP";
            checkBoxGBP.Size = new Size(35, 28);
            checkBoxGBP.TabIndex = 1;
            checkBoxGBP.TabStop = true;
            checkBoxGBP.Text = "£";
            checkBoxGBP.TextAlign = ContentAlignment.MiddleCenter;
            checkBoxGBP.UseVisualStyleBackColor = false;
            // 
            // checkBoxEUR
            // 
            checkBoxEUR.Appearance = Appearance.Button;
            checkBoxEUR.BackColor = Color.FromArgb(60, 60, 60);
            checkBoxEUR.Cursor = Cursors.Hand;
            checkBoxEUR.FlatAppearance.BorderSize = 0;
            checkBoxEUR.FlatAppearance.CheckedBackColor = Color.FromArgb(90, 90, 90);
            checkBoxEUR.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 75, 75);
            checkBoxEUR.FlatStyle = FlatStyle.Flat;
            checkBoxEUR.ForeColor = Color.White;
            checkBoxEUR.Location = new Point(210, 14);
            checkBoxEUR.Name = "checkBoxEUR";
            checkBoxEUR.Size = new Size(35, 28);
            checkBoxEUR.TabIndex = 2;
            checkBoxEUR.Text = "€";
            checkBoxEUR.TextAlign = ContentAlignment.MiddleCenter;
            checkBoxEUR.UseVisualStyleBackColor = false;
            // 
            // cmbPriceType
            // 
            cmbPriceType.BackColor = Color.FromArgb(60, 60, 60);
            cmbPriceType.Cursor = Cursors.Hand;
            cmbPriceType.FlatStyle = FlatStyle.Flat;
            cmbPriceType.ForeColor = Color.White;
            cmbPriceType.Location = new Point(260, 16);
            cmbPriceType.Name = "cmbPriceType";
            cmbPriceType.Size = new Size(160, 23);
            cmbPriceType.TabIndex = 3;
            // 
            // topPanel
            // 
            topPanel.BackColor = Color.FromArgb(40, 40, 40);
            topPanel.Controls.Add(closeButton);
            topPanel.Controls.Add(startButton);
            topPanel.Controls.Add(checkBoxGBP);
            topPanel.Controls.Add(checkBoxEUR);
            topPanel.Controls.Add(cmbPriceType);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Size = new Size(784, 55);
            topPanel.TabIndex = 0;
            // 
            // closeButton
            // 
            closeButton.BackColor = Color.FromArgb(60, 60, 60);
            closeButton.Cursor = Cursors.Hand;
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 45, 45);
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(85, 85, 85);
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.ForeColor = Color.White;
            closeButton.Location = new Point(695, 12);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(77, 32);
            closeButton.TabIndex = 4;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = false;
            closeButton.Click += closeButton_Click;
            // 
            // CardmarketPriceUpdater
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(30, 30, 30);
            ClientSize = new Size(784, 461);
            Controls.Add(topPanel);
            Controls.Add(outputBox);
            Name = "CardmarketPriceUpdater";
            Text = "Cardmarket Price Updater";
            topPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
            this.Icon = global::Cardmarket_Price_Updater.Properties.Resources.CardmarketPriceUpdaterLogo;
        }

        private Button closeButton;
    }
}