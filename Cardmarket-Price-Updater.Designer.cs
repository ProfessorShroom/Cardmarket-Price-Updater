namespace CardPriceUpdaterGui
{
    partial class CardmarketPriceUpdater
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.TextBox outputBox;

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
            SuspendLayout();
            // 
            // startButton
            // 
            startButton.Location = new Point(12, 12);
            startButton.Name = "startButton";
            startButton.Size = new Size(150, 35);
            startButton.TabIndex = 0;
            startButton.Text = "Get Prices (Select File...)";
            startButton.UseVisualStyleBackColor = true;
            // 
            // outputBox
            // 
            outputBox.Location = new Point(12, 60);
            outputBox.Multiline = true;
            outputBox.Name = "outputBox";
            outputBox.ScrollBars = ScrollBars.Vertical;
            outputBox.Size = new Size(760, 367);
            outputBox.TabIndex = 1;
            // 
            // checkBoxGBP
            // 
            checkBoxGBP.Appearance = Appearance.Button;
            checkBoxGBP.AutoSize = true;
            checkBoxGBP.Checked = true;
            checkBoxGBP.Location = new Point(168, 17);
            checkBoxGBP.Name = "checkBoxGBP";
            checkBoxGBP.Size = new Size(23, 25);
            checkBoxGBP.TabIndex = 2;
            checkBoxGBP.TabStop = true;
            checkBoxGBP.Text = "£";
            checkBoxGBP.UseVisualStyleBackColor = true;
            // 
            // checkBoxEUR
            // 
            checkBoxEUR.Appearance = Appearance.Button;
            checkBoxEUR.AutoSize = true;
            checkBoxEUR.Location = new Point(197, 17);
            checkBoxEUR.Name = "checkBoxEUR";
            checkBoxEUR.Size = new Size(23, 25);
            checkBoxEUR.TabIndex = 3;
            checkBoxEUR.TabStop = true;
            checkBoxEUR.Text = "€";
            checkBoxEUR.UseVisualStyleBackColor = true;
            // 
            // CardmarketPriceUpdater
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 461);
            Controls.Add(checkBoxEUR);
            Controls.Add(checkBoxGBP);
            Controls.Add(outputBox);
            Controls.Add(startButton);
            Icon = Cardmarket_Price_Updater.Properties.Resources.CardmarketPriceUpdaterLogo;
            Name = "CardmarketPriceUpdater";
            Text = "Cardmarket Price Updater";
            ResumeLayout(false);
            PerformLayout();

        }

        private RadioButton checkBoxGBP;
        private RadioButton checkBoxEUR;
    }
}
