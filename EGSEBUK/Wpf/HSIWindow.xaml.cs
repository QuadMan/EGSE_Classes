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
            _intfEGSE.GotHsiCmdMsg += new ProtocolHsi.HsiMsgEventHandler(OnHsiCmdMsg);
            DataContext = _intfEGSE;
            GridHSI.DataContext = _intfEGSE.HsiNotify;
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
                hsiMsg = _intfEGSE.DeviceTime.ToString() + ": [" + msg.Info.Line.ToString() + "] (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data, isSmart: true);
            }
            else
            {
                hsiMsg = _intfEGSE.DeviceTime.ToString() + ": [" + msg.Info.Line.ToString() + "] (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data);
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
                hsiMsg = _intfEGSE.DeviceTime.ToString() + ": [" + msg.Info.Line.ToString() + "] (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data, isSmart: true);
            }
            else
            {
                hsiMsg = _intfEGSE.DeviceTime.ToString() + ": [" + msg.Info.Line.ToString() + "] (" + msg.Data.Length.ToString() + ") " + Converter.ByteArrayToHexStr(msg.Data);
            }

            if (null != Monitor && Visibility.Visible == this.Visibility)
            {
                MonitorCmd.Dispatcher.Invoke(new Action(delegate
                {
                    MonitorCmd.Items.Add(hsiMsg);
                    MonitorCmd.ScrollIntoView(hsiMsg);
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

        /// <summary>
        /// Handles the 1 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MonitorCmd.Items.Clear();
        }
    }
}
