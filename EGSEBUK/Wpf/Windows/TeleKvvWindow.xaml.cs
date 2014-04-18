namespace Egse.Defaults
{
    using System;
    using System.Windows;
    using Egse.Devices;
    using Egse.Protocols;
    using Egse.Utilites;

    /// <summary>
    /// Interaction logic for TeleKvvWindow.xaml
    /// </summary>
    public partial class TeleKvvWindow : Window
    {
        private EgseBukNotify intfEGSE;

        public TeleKvvWindow()
        {
            InitializeComponent();
        }

        public void Init(EgseBukNotify intfEGSE)
        {
            this.intfEGSE = intfEGSE;
            this.GridKvv.DataContext = this.intfEGSE.TeleKvvNotify;
            DataContext = this.intfEGSE;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

    }
}
