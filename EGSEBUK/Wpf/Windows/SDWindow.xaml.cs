﻿//-----------------------------------------------------------------------
// <copyright file="SDWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
    /// Interaction logic for SDWindow.xaml
    /// </summary>
    public partial class SDWindow : Window
    {
        /// <summary>
        /// Интерфейс управления прибором.
        /// </summary>
        private EgseBukNotify _intfEGSE;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SDWindow" />.
        /// </summary>
        public SDWindow()
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
            _intfEGSE.GotSpacewire3Msg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewireMsg);
            GridSD.DataContext = _intfEGSE.Spacewire3Notify;
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

            if (msg is SpacewireEmptyProtoMsgEventArgs)
            {
                SpacewireEmptyProtoMsgEventArgs emptyMsg = msg as SpacewireEmptyProtoMsgEventArgs;
                if (emptyMsg.Data.Length > 30)
                {
                    spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": (" + emptyMsg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(emptyMsg.Data, isSmart: true);
                }
                else
                {
                    spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": (" + emptyMsg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(emptyMsg.Data);
                }
            }
            else if (msg is SpacewireErrorMsgEventArgs)
            {
                SpacewireErrorMsgEventArgs err = msg as SpacewireErrorMsgEventArgs;
                spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": (" + err.Data.Length.ToString() + ") [" + Converter.ByteArrayToHexStr(err.Data) + "] Ошибка: " + err.ErrorMessage();
            }

            if (null != MonitorList && Visibility.Visible == this.Visibility)
            {
                try
                {
                    Extensions.AddToMonitor(MonitorList, spacewireMsg);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }

                if (_intfEGSE.Spacewire3Notify.IsSaveTxtData)
                {
                    LogsClass.LogSpacewire3.LogText = spacewireMsg;
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
    }
}