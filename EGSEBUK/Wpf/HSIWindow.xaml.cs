//-----------------------------------------------------------------------
// <copyright file="HSIWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
    /// Interaction logic for HSIWindow.xaml
    /// </summary>
    public partial class HSIWindow : Window
    {
        /// <summary>
        /// Интерфейс управления прибором.
        /// </summary>
        private EgseBukNotify _intfEGSE;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="HSIWindow" />.
        /// </summary>
        public HSIWindow()
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
            _intfEGSE.GotHsiMsg += new ProtocolHsi.HsiMsgEventHandler(OnHsiMsg);
            _intfEGSE.GotHsiCmdMsg += new ProtocolHsi.HsiMsgEventHandler(OnHsiCmdMsg);
            DataContext = _intfEGSE;
            GridHSI.DataContext = _intfEGSE.HsiNotify;
            MonitorList.DataContext = new MonitorListViewModel();
            MonitorListCmd.DataContext = new MonitorListViewModel();    
        }

        /// <summary>
        /// Вызывается когда [пришло сообщение по протоколу ВСИ].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        public void OnHsiMsg(object sender, HsiMsgEventArgs msg)
        {
            new { msg }.CheckNotNull(); 
            string hsiMsg;
            if (msg.Data.Length > 30)
            {
                hsiMsg = _intfEGSE.DeviceTime.ToString() + ": [" + msg.Info.Line.Description() + "] (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data, isSmart: true);
            }
            else
            {
                hsiMsg = _intfEGSE.DeviceTime.ToString() + ": [" + msg.Info.Line.Description() + "] (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data);
            }

            if (null != MonitorList && Visibility.Visible == this.Visibility)
            {
                try
                {
                    Extensions.AddToMonitor(MonitorList, hsiMsg);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
        }

        /// <summary>
        /// Вызывается когда [пришло УКС по протоколу ВСИ].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        public void OnHsiCmdMsg(object sender, HsiMsgEventArgs msg)
        {
            new { msg }.CheckNotNull(); 
            string hsiMsg;
            if (msg.Data.Length > 30)
            {
                hsiMsg = _intfEGSE.DeviceTime.ToString() + ": [" + msg.Info.Line.Description() + "] (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data, isSmart: true);
            }
            else
            {
                hsiMsg = _intfEGSE.DeviceTime.ToString() + ": [" + msg.Info.Line.Description() + "] (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data);
            }

            if (null != MonitorListCmd && Visibility.Visible == this.Visibility)
            {
                try
                {
                    Extensions.AddToMonitor(MonitorListCmd, hsiMsg);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }        
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

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Extensions.ClearMonitor(MonitorList);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        /// <summary>
        /// Handles the 1 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Extensions.ClearMonitor(MonitorListCmd);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }
    }
}
