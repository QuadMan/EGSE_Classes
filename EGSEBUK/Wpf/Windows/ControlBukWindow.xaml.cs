//-----------------------------------------------------------------------
// <copyright file="ControlBukWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
            GridControlBuk.DataContext = intfEGSE.ControlBukNotify;
            DataContext = this.intfEGSE;          
        }    

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
