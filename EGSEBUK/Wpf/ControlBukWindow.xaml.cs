namespace Egse.Defaults
{
    using System;
    using System.Windows;
    using Egse.Devices;
    using Egse.Protocols;
    using Egse.Utilites;

    /// <summary>
    /// Interaction logic for ControlBukWindow.xaml
    /// </summary>
    public partial class ControlBukWindow : Window
    {
        private EgseBukNotify intfEGSE;

        public ControlBukWindow()
        {
            InitializeComponent();
        }

        public void Init(EgseBukNotify intfEGSE)
        {
            this.intfEGSE = intfEGSE;           
            DataContext = this.intfEGSE;          
        }    

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
