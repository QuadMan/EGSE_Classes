//-----------------------------------------------------------------------
// <copyright file="SimRouterWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
    public partial class SimRouterWindow : Window
    {
        /// <summary>
        /// Интерфейс управления прибором.
        /// </summary>
        private DevBUK _intfEGSE;

        /// <summary>
        /// Таймер обновления UI.
        /// </summary>
        private System.Windows.Threading.DispatcherTimer _dispatcherTimer; 

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="SimRouterWindow" />.
        /// </summary>
        public SimRouterWindow()
        {
            InitializeComponent();
            
            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(TimerWork);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
        }

        /// <summary>
        /// Инициализирует интерфейс управления прибором.
        /// </summary>
        /// <param name="intfEGSE">Интерфейс управления прибором.</param>
        public void Init(DevBUK intfEGSE)
        {
            _intfEGSE = intfEGSE;
            _intfEGSE.GotSpacewire2Msg += new ProtocolSpacewire.SpacewireMsgEventHandler(OnSpacewire2Msg);
            DataContext = _intfEGSE;
        }

        /// <summary>
        /// Вызывается когда [пришло сообщение по протоколу spacewire].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The <see cref="SpacewireMsgEventArgs"/> instance containing the event data.</param>
        public void OnSpacewire2Msg(object sender, SpacewireMsgEventArgs msg)
        {
            if (msg != null)
            {
                string spacewireMsg = _intfEGSE.DeviceTime.ToString() + ": " + Converter.ByteArrayToHexStr(msg.Data);
                if (null != Spacewire2Mon && Visibility.Visible == this.Visibility)
                {
                    Spacewire2Mon.Dispatcher.Invoke(new Action(delegate
                        { 
                            Spacewire2Mon.Items.Add(spacewireMsg); 
                            Spacewire2Mon.ScrollIntoView(spacewireMsg); 
                        }));
                }
            }
        }  

        /// <summary>
        /// Обновляет состояние UI.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void TimerWork(object sender, EventArgs e)
        {
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
            _intfEGSE.IsSpaceWire2TimeMark = !_intfEGSE.IsSpaceWire2TimeMark;
        }

        /// <summary>
        /// Handles the 1 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2BukTrans = !_intfEGSE.IsSpaceWire2BukTrans;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2BkpTrans = !_intfEGSE.IsSpaceWire2BkpTrans;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2BukKbv = !_intfEGSE.IsSpaceWire2BukKbv;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2BkpKbv = !_intfEGSE.IsSpaceWire2BkpKbv;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2IntfOn = !_intfEGSE.IsSpaceWire2IntfOn;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2BukTransData = !_intfEGSE.IsSpaceWire2BukTransData;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2BkpTransData = !_intfEGSE.IsSpaceWire2BkpTransData;
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2RecordSendBkp = true;
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2RecordSendBuk = true;
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2RecordSendRMAP = true;       
        }
    }
}
