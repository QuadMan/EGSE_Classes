//-----------------------------------------------------------------------
// <copyright file="HSIWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
            DataContext = _intfEGSE;
            GridHSI.DataContext = _intfEGSE.HSINotify;
        }

        /// <summary>
        /// Вызывается когда [пришло сообщение по протоколу ВСИ].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        public void OnHsiMsg(object sender, HsiMsgEventArgs msg)
        {
            if (msg != null)
            {
                string hsiMsg;
                if (msg.Data.Length > 30)
                {
                    hsiMsg = _intfEGSE.DeviceTime.ToString() + ": (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data.Take<byte>(10).ToArray()) + "..." + Converter.ByteArrayToHexStr(msg.Data.Skip<byte>(msg.Data.Length - 10).ToArray());
                }
                else
                {
                    hsiMsg = _intfEGSE.DeviceTime.ToString() + ": (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data);
                }

                if (null != Monitor && Visibility.Visible == this.Visibility)
                {
                    Monitor.Dispatcher.Invoke(new Action(delegate
                    {
                        Monitor.Items.Add(hsiMsg);
                        Monitor.ScrollIntoView(hsiMsg);
                    }));
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
    }
}
