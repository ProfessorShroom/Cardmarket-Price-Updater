using System;
using System.Windows.Forms;

namespace CardPriceUpdaterGui
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new CardmarketPriceUpdater());
        }
    }
}
