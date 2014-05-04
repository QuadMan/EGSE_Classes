//-----------------------------------------------------------------------
// <copyright file="HsiWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
    /// Interaction logic for HsiWindow.xaml
    /// </summary>
    public partial class HsiWindow : Window
    {
        /// <summary>
        /// Интерфейс управления прибором.
        /// </summary>
        private EgseBukNotify _intfEGSE;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="HsiWindow" />.
        /// </summary>
        public HsiWindow()
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
            GridHsi.DataContext = _intfEGSE.HsiNotify;
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

            SendToMonitor(hsiMsg);
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

            SendToMonitorCmd(hsiMsg);
        }

        /// <summary>
        /// Sends to monitor.
        /// </summary>
        /// <param name="txtMsg">The text MSG.</param>
        private void SendToMonitor(string txtMsg)
        {
            if (null != MonitorList)
            {
                try
                {
                    Extensions.AddToMonitor(MonitorList, txtMsg);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }

            if (_intfEGSE.HsiNotify.IsSaveTxtData)
            {
                LogsClass.LogHsi.LogText = txtMsg;
            }            
        }

        /// <summary>
        /// Sends to monitor command.
        /// </summary>
        /// <param name="txtMsg">The text MSG.</param>
        private void SendToMonitorCmd(string txtMsg)
        {
            if (null != MonitorListCmd)
            {
                try
                {
                    Extensions.AddToMonitor(MonitorListCmd, txtMsg);
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

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (true == (bool)e.NewValue)
            {
                MonitorList.SetValue(ListBoxExtensions.IsScrollingProperty, true);
            }

            if (true == (bool)e.NewValue)
            {
                MonitorListCmd.SetValue(ListBoxExtensions.IsScrollingProperty, true);
            }            
        }
    }
}
