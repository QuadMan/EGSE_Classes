//-----------------------------------------------------------------------
// <copyright file="SDWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
    using EGSE.Protocols;
    using EGSE.Utilites;

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
        }

        /// <summary>
        /// Вызывается когда [пришло сообщение по протоколу spacewire].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        public void OnSpacewireMsg(object sender, MsgBase msg)
        {
            new { msg }.CheckNotNull();
            string spacewireMsg = string.Empty;

            if (msg is SpacewireSptpMsgEventArgs)
            {
                SpacewireSptpMsgEventArgs sptpMsg = msg as SpacewireSptpMsgEventArgs;
                if (sptpMsg.Data.Length > 30)
                {
                    spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": (" + sptpMsg.Data.Length.ToString() + ") " + sptpMsg.SptpInfo.From.ToString() + "-" + sptpMsg.SptpInfo.MsgType.ToString() + "->" + sptpMsg.SptpInfo.To.ToString() + " " + Converter.ByteArrayToHexStr(sptpMsg.Data.Take<byte>(10).ToArray()) + "..." + Converter.ByteArrayToHexStr(sptpMsg.Data.Skip<byte>(sptpMsg.Data.Length - 10).ToArray());
                }
                else
                {
                    spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": (" + sptpMsg.Data.Length.ToString() + ") " + sptpMsg.SptpInfo.From.ToString() + "-" + sptpMsg.SptpInfo.MsgType.ToString() + "->" + sptpMsg.SptpInfo.To.ToString() + " " + Converter.ByteArrayToHexStr(sptpMsg.Data);
                }

            }
            else if (msg is SpacewireErrorMsgEventArgs)
            {
                SpacewireErrorMsgEventArgs err = msg as SpacewireErrorMsgEventArgs;
                spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": (" + err.Data.Length.ToString() + ") [" + Converter.ByteArrayToHexStr(err.Data) + "] Ошибка: " + err.ErrorMessage();
            }

            if (null != Monitor && Visibility.Visible == this.Visibility)
            {
                Monitor.Dispatcher.Invoke(new Action(delegate
                {
                    Monitor.Items.Add(spacewireMsg);
                    Monitor.ScrollIntoView(spacewireMsg);
                }));
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
            Monitor.Items.Clear();
        }
    }
}
