//-----------------------------------------------------------------------
// <copyright file="SpacewireWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
        }

        /// <summary>
        /// Вызывается когда [пришло сообщение по протоколу spacewire].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        public void OnSpacewireMsg(object sender, SpacewireSptpMsgEventArgs msg)
        {
            new { msg }.CheckNotNull();
            string spacewireMsg;
            
            if (msg.Data.Length > 30) 
            {
                spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": (" + msg.Data.Length.ToString() + ") [" + msg.SptpInfo.From.ToString() + "-" + msg.SptpInfo.MsgType.ToString() + "->" + msg.SptpInfo.To.ToString() + "] [" + Converter.ByteArrayToHexStr(msg.Data.Take<byte>(10).ToArray()) + "..." + Converter.ByteArrayToHexStr(msg.Data.Skip<byte>(msg.Data.Length - 10).ToArray()) + "]";
            }
            else
            {
                spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": (" + msg.Data.Length.ToString() + ") [" + msg.SptpInfo.From.ToString() + "-" + msg.SptpInfo.MsgType.ToString() + "->" + msg.SptpInfo.To.ToString() + "] [" + Converter.ByteArrayToHexStr(msg.Data) + "]";
            }
            // crc check
            if (IsTkMsg(msg) || IsTmMsg(msg))
            {
                ushort crcInData = msg.ToArray().AsTk().Crc;
                ushort crcGen = msg.ToArray().AsTk().NeededCrc;
                spacewireMsg += (crcGen == crcInData ? " > Crc ok" : " > Crc error, need " + crcGen.ToString("X4"));
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

        internal bool IsTkMsg(SpacewireSptpMsgEventArgs msg)
        {
            try
            {
                SpacewireIcdMsgEventArgs icdMsg = msg.ToArray().AsIcd();
                return icdMsg.IcdInfo.Version == 0 
                    && icdMsg.IcdInfo.Type == SpacewireIcdMsgEventArgs.IcdType.Tk
                    && icdMsg.IcdInfo.Flag == SpacewireIcdMsgEventArgs.IcdFlag.HeaderFill
                    && icdMsg.IcdInfo.Apid == _intfEGSE.Spacewire2Notify.CurApid;
            }
            catch
            {
                return false;
            }
        }

        internal bool IsTmMsg(SpacewireSptpMsgEventArgs msg)
        {
            try
            {
                SpacewireIcdMsgEventArgs icdMsg = msg.ToArray().AsIcd();
                return icdMsg.IcdInfo.Version == 0
                    && icdMsg.IcdInfo.Type == SpacewireIcdMsgEventArgs.IcdType.Tm
                    && icdMsg.IcdInfo.Flag == SpacewireIcdMsgEventArgs.IcdFlag.HeaderFill
                    && icdMsg.IcdInfo.Apid == _intfEGSE.Spacewire2Notify.CurApid;
            }
            catch
            {
                return false;
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
