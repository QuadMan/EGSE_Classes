//-----------------------------------------------------------------------
// <copyright file="SimSDWindow.xaml.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Defaults
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using EGSE.Devices;

    /// <summary>
    /// Interaction logic for SimSDWindow.xaml
    /// </summary>
    public partial class SimSDWindow : Window
    {
        /// <summary>
        /// Интерфейс управления прибором.
        /// </summary>
        private EgseBukNotify _intfEGSE;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SimSDWindow" />.
        /// </summary>
        public SimSDWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Инициализирует интерфейс управления прибором.
        /// </summary>
        /// <param name="intfEGSE">Интерфейс управления прибором.</param>
        public void Init(EgseBukNotify intfEGSE)
        {
            _intfEGSE = intfEGSE;
            DataContext = _intfEGSE;
            GridSimSD.DataContext = _intfEGSE.Spacewire4Notify;
        }

        /// <summary>
        /// Handles the 11 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            if ("false" == (string)(sender as Button).Tag)
            {
                _intfEGSE.Spacewire4Notify.IsRecordSend = true;
            }
        }

        /// <summary>
        /// Handles the 5 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire4Notify.IsIntfOn = !_intfEGSE.Spacewire4Notify.IsIntfOn;
        }

        /// <summary>
        /// Handles the 6 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire4Notify.IsTimeMark = !_intfEGSE.Spacewire4Notify.IsTimeMark;
        }

        /// <summary>
        /// Handles the 7 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire4Notify.IsEOPSend = !_intfEGSE.Spacewire4Notify.IsEOPSend;
        }

        /// <summary>
        /// Handles the 8 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire4Notify.IsAutoSend = !_intfEGSE.Spacewire4Notify.IsAutoSend;
        }

        /// <summary>
        /// Handles the 9 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire4Notify.IsEEPSend = !_intfEGSE.Spacewire4Notify.IsEEPSend;
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
