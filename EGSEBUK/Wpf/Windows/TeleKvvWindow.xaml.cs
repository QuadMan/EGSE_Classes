﻿//-----------------------------------------------------------------------
// <copyright file="TeleKvvWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
    /// Interaction logic for TeleKvvWindow.xaml
    /// </summary>
    public partial class TeleKvvWindow : Window
    {
        /// <summary>
        /// The intf egse
        /// </summary>
        private EgseBukNotify intfEGSE;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TeleKvvWindow" />.
        /// </summary>
        public TeleKvvWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the specified intf egse.
        /// </summary>
        /// <param name="intfEGSE">The intf egse.</param>
        public void Init(EgseBukNotify intfEGSE)
        {
            this.intfEGSE = intfEGSE;
            this.GridKvv.DataContext = this.intfEGSE.TeleKvvNotify;
            DataContext = this.intfEGSE;
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
