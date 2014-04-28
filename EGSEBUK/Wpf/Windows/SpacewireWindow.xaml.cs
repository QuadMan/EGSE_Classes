//-----------------------------------------------------------------------
// <copyright file="SpacewireWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
    /// Interaction logic for SimRouterWindow.xaml
    /// </summary>
    public partial class SpacewireWindow : Window
    {
        /// <summary>
        /// Интерфейс управления прибором.
        /// </summary>
        private EgseBukNotify _intfEGSE;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SpacewireWindow" />.
        /// </summary>
        public SpacewireWindow()
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
            _intfEGSE.GotSpacewire2Msg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewireMsg);
            DataContext = _intfEGSE;
            GridSpacewire.DataContext = _intfEGSE.Spacewire2Notify;
            MonitorList.DataContext = new MonitorListViewModel();
        }       

        /// <summary>
        /// Вызывается когда [пришло сообщение по протоколу spacewire].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        public void OnSpacewireMsg(object sender, BaseMsgEventArgs msg)
        {
            new { msg }.CheckNotNull();
            string spacewireMsg = string.Empty;

            spacewireMsg = _intfEGSE.DeviceTime.ToString() + msg.ToString();

            SendToMonitor(spacewireMsg);            
        }

        /// <summary>
        /// Sends to monitor.
        /// </summary>
        /// <param name="txtMsg">The text MSG.</param>
        private void SendToMonitor(string txtMsg)
        {
            if (null != MonitorList && Visibility.Visible == this.Visibility)
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

            if (_intfEGSE.Spacewire2Notify.IsSaveTxtData)
            {
                LogsClass.LogSpacewire2.LogText = txtMsg;
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
    }
}
