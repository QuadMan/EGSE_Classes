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
            if (msg != null)
            {

                string spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": " + Converter.ByteArrayToHexStr(msg.Data.Take<byte>(10).ToArray()) + "..." + Converter.ByteArrayToHexStr(msg.Data.Skip<byte>(msg.Data.Length - 10).ToArray());
                if (null != Monitor && Visibility.Visible == this.Visibility)
                {
                    Monitor.Dispatcher.Invoke(new Action(delegate
                        {
                            Monitor.Items.Add(spacewireMsg);
                            Monitor.ScrollIntoView(spacewireMsg);
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

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsTimeMark = !_intfEGSE.Spacewire2Notify.IsTimeMark;
        }

        /// <summary>
        /// Handles the 1 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsBukTrans = !_intfEGSE.Spacewire2Notify.IsBukTrans;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsBkpTrans = !_intfEGSE.Spacewire2Notify.IsBkpTrans;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsBukKbv = !_intfEGSE.Spacewire2Notify.IsBukKbv;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsBkpKbv = !_intfEGSE.Spacewire2Notify.IsBkpKbv;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsIntfOn = !_intfEGSE.Spacewire2Notify.IsIntfOn;
        }

        ///// <summary>
        ///// Handles the Click event of the Button control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        //private void Button_Click_6(object sender, RoutedEventArgs e)
        //{
        //    _intfEGSE.Spacewire2Notify.IsBukTransData = !_intfEGSE.Spacewire2Notify.IsBukTransData;
        //}

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsBkpTransData = !_intfEGSE.Spacewire2Notify.IsBkpTransData;
        }

        /// <summary>
        /// Handles the 8 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsSendBkp = true;
        }

        /// <summary>
        /// Handles the 9 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsSendBuk = true;
        }

        /// <summary>
        /// Handles the 10 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            _intfEGSE.Spacewire2Notify.IsSendRMAP = true;       
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            //_intfEGSE.Device.CmdSetDeviceLogicAddr();
        }
    }
}
