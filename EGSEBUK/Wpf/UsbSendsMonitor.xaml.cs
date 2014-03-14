//-----------------------------------------------------------------------
// <copyright file="UsbSendsMonitor.xaml.cs" company="IKI RSSI, laboratory №711">
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
    using System.IO;

    /// <summary>
    /// Используется для мониторирования исходящего трафика по USB.
    /// </summary>
    public partial class UsbSendsMonitor : Window
    {
        /// <summary>
        /// Интерфейс управления прибором.
        /// </summary>
        private EgseBukNotify _intfEGSE;

        public UsbSendsMonitor()
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
            GridUsbSendsMonitor.DataContext = _intfEGSE.UITestNotify;
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
            LastOperationMonitor.Items.Clear();
        }

        internal void PopulateList(string filePath)
        {
            List<string> lines = new List<string>();
            if (!File.Exists(filePath))
            {
                Monitor.Items.Add(@"Файл не существует!");
                return;
            }
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default)) 
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Monitor.Items.Add(line);
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            List<string> last = Monitor.Items.Cast<string>().ToList();                                         
            Monitor.Items.Clear();
            PopulateList(_intfEGSE.UITestNotify.UsbLogFile);
            List<string> now = Monitor.Items.Cast<string>().ToList();
            var differenceQuery = now.Except(last);
            LastOperationMonitor.Items.Clear();
            foreach (var s in differenceQuery)
            {
                LastOperationMonitor.Items.Add(s);
            }
        }
    }
}
