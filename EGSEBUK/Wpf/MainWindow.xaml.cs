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
    using System.Configuration;
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
    using EGSE.Utilites;
    using System.Collections.Specialized;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Окно "Монитор USB (исходящий)".
        /// </summary>
        private UsbSendsMonitor winUsbSendsMonitor;

        /// <summary>
        /// Окно "Имитатор ВСИ".
        /// </summary>
        private HSIWindow winHSI;

        /// <summary>
        /// Окно "Имитатор БУК (для ВСИ)".
        /// </summary>
        private SimHSIWindow winSimHSI;

        /// <summary>
        /// Окно "Имитатор БУСК".
        /// </summary>
        private SpacewireWindow winSpacewire;

        /// <summary>
        /// Окно "Имитатор БУК (для БУСК)".
        /// </summary>
        private SimSpacewireWindow winSimSpacewire;

        /// <summary>
        /// Окно "Имитатор НП".
        /// </summary>
        private SDWindow winSD;

        /// <summary>
        /// Окно "Имитатор БУК (для НП)".
        /// </summary>
        private SimSDWindow winSimSD;

        /// <summary>
        /// Экземпляр для работы с командами циклограммы.
        /// </summary>
        private CyclogramCommandBuk _bukCycCommands = new CyclogramCommandBuk();

        /// <summary>
        /// Интерфейс работы с устройством устройства.
        /// </summary>
        private EgseBukNotify _intfEGSE = new EgseBukNotify();

        /// <summary>
        /// Инициализация параметров управления
        /// </summary>
        private void InitControlValues()
        {
            _bukCycCommands.BukNotify = _intfEGSE;
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
            if (this.GotSaveAppEvent != null)
            {
                this.GotSaveAppEvent();
            }
        }

        /// <summary>
        /// Загружаем специфичные настройки приложения при загрузке
        /// </summary>
        private void LoadAppSettings()
        {
            if (this.GotLoadAppEvent != null)
            {
                this.GotLoadAppEvent();
            }
        }

        /// <summary>
        /// Метод инициализируеющий дополнительные модули (если это необходимо)
        /// </summary>
        private void InitModules()
        {
            CycloGrid.AddCycCommands(_bukCycCommands.CyclogramCommandsAvailable);
            DataContext = _intfEGSE;
            this.GotLoadAppEvent += new Action(_intfEGSE.LoadApp);
            this.GotSaveAppEvent += new Action(_intfEGSE.SaveApp);
            winHSI = new HSIWindow();
            winHSI.Init(_intfEGSE);
            winSpacewire = new SpacewireWindow();
            winSpacewire.Init(_intfEGSE);
            winSD = new SDWindow();
            winSD.Init(_intfEGSE);
            winSimHSI = new SimHSIWindow();
            winSimHSI.Init(_intfEGSE);
            winSimSpacewire = new SimSpacewireWindow();
            winSimSpacewire.Init(_intfEGSE);
            winSimSD = new SimSDWindow();
            winSimSD.Init(_intfEGSE);
            winUsbSendsMonitor = new UsbSendsMonitor();
            winUsbSendsMonitor.Init(_intfEGSE);
            GridTelemetry.DataContext = _intfEGSE.TelemetryNotify;
            GridShutter.DataContext = _intfEGSE;
            ManualControlSet.DataContext = _intfEGSE;
        }
    }
}
