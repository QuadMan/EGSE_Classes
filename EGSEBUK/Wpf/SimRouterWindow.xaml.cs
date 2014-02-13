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
    using System.Globalization;
    using EGSE.Constants;
    using EGSE.Devices;

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
            DataContext = _intfEGSE;
        }

        /// <summary>
        /// Обновляет состояние UI.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void TimerWork(object sender, EventArgs e)
        {
           /* if (_intfEGSE.IsSpaceWire2IntfOn)
            {
                SpaceWire2IntfOn.Content = "ВКЛ";
                SpaceWire2IntfOn.Background = Brushes.LightGreen;
            }
            else
            {
                SpaceWire2IntfOn.Content = "ВЫКЛ";
                SpaceWire2IntfOn.Background = Brushes.Red;
            }*/
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
        /// Handles the Click event of the SpaceWire2IntfOn control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SpaceWire2IntfOn_Click(object sender, RoutedEventArgs e)
        {
            _intfEGSE.IsSpaceWire2IntfOn = !_intfEGSE.IsSpaceWire2IntfOn;
        }
    }
    
    public class BoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value == true) ? "true" : "false";   
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value == "true") ? true : false;
        }
    }
}
