//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using EGSE.Defaults;
    using EGSE.Utilites;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Mouses the logger event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        public void MouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = EventClickToString.ElementClicked(e);
            if (logEvent != null)
            {
                LogsClass.LogOperator.LogText = logEvent;
            }
        }
    }
}
