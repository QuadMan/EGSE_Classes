//-----------------------------------------------------------------------
// <copyright file="TeleBukWindow.xaml.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Defaults
{
    using System;
    using System.Windows;
    using Egse.Devices;
    using Egse.Protocols;
    using Egse.Utilites;

    /// <summary>
    /// Interaction logic for TeleBukWindow.xaml
    /// </summary>
    public partial class TeleBukWindow : Window
    {
        private EgseBukNotify intfEGSE;

        public TeleBukWindow()
        {
            InitializeComponent();
        }

        public void Init(EgseBukNotify intfEGSE)
        {
            this.intfEGSE = intfEGSE;
            this.GridBuk.DataContext = this.intfEGSE.TeleBukNotify;
            DataContext = this.intfEGSE;
        }  

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
