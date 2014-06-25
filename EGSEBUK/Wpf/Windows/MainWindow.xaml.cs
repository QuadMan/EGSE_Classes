//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Defaults
{
    using System;
    using System.Linq;
    using System.Windows;
    using Egse.Cyclogram.Command;
    using Egse.Devices;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The win tele KVV
        /// </summary>
        private TeleKvvWindow winTeleKvv;

        /// <summary>
        /// The win tele buk
        /// </summary>
        private TeleBukWindow winTeleBuk;

        /// <summary>
        /// The win control buk
        /// </summary>
        private ControlBukWindow winControlBuk;

        /// <summary>
        /// Окно "Монитор USB (исходящий)".
        /// </summary>
        private UsbSendsMonitor winUsbSendsMonitor;

        /// <summary>
        /// Окно "Имитатор ВСИ".
        /// </summary>
        private HsiWindow winHsi;

        /// <summary>
        /// Окно "Имитатор БУК (для ВСИ)".
        /// </summary>
        private SimHsiWindow winSimHsi;

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
            this.GotLoadAppEvent += new Action(_intfEGSE.LoadAppEvent);
            this.GotSaveAppEvent += new Action(_intfEGSE.SaveAppEvent);
            winControlBuk = new ControlBukWindow();
            winControlBuk.Init(_intfEGSE);
            winTeleKvv = new TeleKvvWindow();
            winTeleKvv.Init(_intfEGSE);   
            winTeleBuk = new TeleBukWindow();
            winTeleBuk.Init(_intfEGSE);            
            winHsi = new HsiWindow();
            winHsi.Init(_intfEGSE);
            winSpacewire = new SpacewireWindow();
            winSpacewire.Init(_intfEGSE);
            winSD = new SDWindow();
            winSD.Init(_intfEGSE);
            winSimHsi = new SimHsiWindow();
            winSimHsi.Init(_intfEGSE);
            winSimSpacewire = new SimSpacewireWindow();
            winSimSpacewire.Init(_intfEGSE);
            winSimSD = new SimSDWindow();
            winSimSD.Init(_intfEGSE);
            winUsbSendsMonitor = new UsbSendsMonitor();
            winUsbSendsMonitor.Init(_intfEGSE);
            GridTelemetry.DataContext = _intfEGSE.TelemetryNotify;
            GridShutter.DataContext = _intfEGSE;
            ManualControlSet.DataContext = _intfEGSE;
            AutoControlSet.DataContext = _intfEGSE;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AboutBoxSimple aboutBox = new AboutBoxSimple(this);
            aboutBox.ShowDialog();
        }

        private void Label_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RTMonTele.Visibility = Visibility.Hidden == RTMonTele.Visibility ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
