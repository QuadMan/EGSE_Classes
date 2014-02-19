//-----------------------------------------------------------------------
// <copyright file="SimSpacewireWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
    using EGSE.Constants;
    using EGSE.Devices;
    using EGSE.Protocols;
    using EGSE.Utilites;

    /// <summary>
    /// Interaction logic for SpaceWireWindow.xaml
    /// </summary>
    public partial class SimSpacewireWindow : Window
    {
        /// <summary>
        /// Интерфейс управления прибором.
        /// </summary>
        private DevBUK _intfEGSE;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SimSpacewireWindow" />.
        /// </summary>
        public SimSpacewireWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Инициализирует интерфейс управления прибором.
        /// </summary>
        /// <param name="intfEGSE">Интерфейс управления прибором.</param>
        public void Init(DevBUK intfEGSE)
        {
            _intfEGSE = intfEGSE;
            DataContext = _intfEGSE;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire1NP1Trans = !_intfEGSE.IsSpaceWire1NP1Trans;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire1NP2Trans = !_intfEGSE.IsSpaceWire1NP2Trans;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire1NP1TransData = !_intfEGSE.IsSpaceWire1NP1TransData;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire1NP2TransData = !_intfEGSE.IsSpaceWire1NP2TransData;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire1IntfOn = !_intfEGSE.IsSpaceWire1IntfOn;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire4IntfOn = !_intfEGSE.IsSpaceWire4IntfOn;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire4TimeMark = !_intfEGSE.IsSpaceWire4TimeMark;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire4EOPSend = !_intfEGSE.IsSpaceWire4EOPSend;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire4AutoSend = !_intfEGSE.IsSpaceWire4AutoSend;
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire4EEPSend = !_intfEGSE.IsSpaceWire4EEPSend;
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if ("false" == (string)(sender as Button).Tag)
            {
                _intfEGSE.IsSpaceWire1RecordSend = true;
            }
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            if ("false" == (string)(sender as Button).Tag)
            {
                return;
            }
        }

    }
}
