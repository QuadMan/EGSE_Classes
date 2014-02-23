//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="IKI RSSI, laboratory №711">
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
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using EGSE.Constants;
    using EGSE.Cyclogram.Command;
    using EGSE.Devices;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*private HSIWindow winHSI = new HSIWindow();
        private SpaceWireWindow winSW = new SpaceWireWindow();
        private SDWindow winSD = new SDWindow();*/

        /// <summary>
        /// Окно "имитатор БМ-4"
        /// </summary>
        private SimRouterWindow winSimRouter;

        /// <summary>
        /// Окно "имитаторы spacewire"
        /// </summary>
        private SimSpacewireWindow winSimSpacewire;

        /// <summary>
        /// Экземпляр для работы с командами циклограммы.
        /// </summary>
        private CyclogramCommandBUK _bukCycCommands = new CyclogramCommandBUK();

        /// <summary>
        /// Интерфейс работы с устройством устройства.
        /// </summary>
        private EgseBukNotify _intfEGSE = new EgseBukNotify();

        /// <summary>
        /// Инициализация параметров управления
        /// </summary>
        private void InitControlValues()
        {
            _bukCycCommands.BUK = _intfEGSE;
        }

        /// <summary>
        /// Для синхронизации параметров управления.
        /// </summary>
        private void OnTimerWork()
        {
            _intfEGSE.TickAllControlsValues();
        }
       
        /// <summary>
        /// Сохраняем специфические настройки приложения
        /// В данном случае - видимость панели телеметрии
        /// </summary>
        private void SaveAppSettings()
        {
           // AppSettings.Save("PowerLabel", Convert.ToString(TMGrid.Visibility));
           // AppSettings.SaveList(hsiWin.UksSendedList, "UksItems");
        }

        /// <summary>
        /// Загружаем специфичные настройки приложения при загрузке
        /// </summary>
        private void LoadAppSettings()
        {
            /*
            // управляем отображением телеметрической информацией
            string powerLabelVisible = AppSettings.Load("PowerLabel");
            if (powerLabelVisible != null)
            {
                switch (powerLabelVisible)
                {
                    case "Visible":
                        TMGrid.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "Hidden":
                        TMGrid.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    default:
                        TMGrid.Visibility = System.Windows.Visibility.Visible;
                        break;
                }
            }

            AppSettings.LoadList(hsiWin.UksSendedList, "UksItems");
            hsiWin.UksStrText.ItemsSource = hsiWin.UksSendedList.ToArray<string>();*/
        }

        /// <summary>
        /// Метод инициализируеющий дополнительные модули (если это необходимо)
        /// </summary>
        private void InitModules()
        {
            CycloGrid.AddCycCommands(_bukCycCommands.CyclogramCommandsAvailable);
            winSimRouter = new SimRouterWindow();
            winSimRouter.Init(_intfEGSE);
            winSimSpacewire = new SimSpacewireWindow();            
            winSimSpacewire.Init(_intfEGSE);
            DataContext = _intfEGSE;
            GridTelemetry.DataContext = _intfEGSE.TelemetryNotify;
        }

        /// <summary>
        /// Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _intfEGSE.ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk1, Convert.ToInt32(!_intfEGSE.Tele.PowerBusk1));
        }

        /// <summary>
        /// Handles the 1 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _intfEGSE.ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBusk2, Convert.ToInt32(!_intfEGSE.Tele.PowerBusk2));
        }

        /// <summary>
        /// Handles the 2 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _intfEGSE.ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund1, Convert.ToInt32(!_intfEGSE.Tele.PowerBund1));
        }

        /// <summary>
        /// Handles the 3 event of the Button_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _intfEGSE.ControlValuesList[Global.Telemetry].SetProperty(Global.Telemetry.PowerBund2, Convert.ToInt32(!_intfEGSE.Tele.PowerBund2));
        }
    }
}
