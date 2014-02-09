//-----------------------------------------------------------------------
// <copyright file="EGSEDefaultMainWindow.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Defaults
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading;
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
    using EGSE.Utilites;
    using EGSE.Utilites.ADC;

    /// <summary>
    /// Класс, содержащий "неизменяемые" методы и поля-свойства основного окна.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Таймер обновления UI.
        /// </summary>
        private System.Windows.Threading.DispatcherTimer _dispatcherTimer;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainWindow" />.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Title = BUKConst.ShowCaption;

            InitControlValues();
            LoadWindows();
            InitModules();

            LoadAppSettings();

            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(TimerWork);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
            _intfEGSE.Device.Start();
        }

        /// <summary>
        /// Загружаем параметры окон из конфигурационного файла
        /// </summary>
        private void LoadWindows()
        {
            foreach (Window w in Application.Current.Windows)
            {
                AppSettings.LoadWindow(w);
            }
        }

        /// <summary>
        /// Сохраняем параметры окно в конфигурационном файле
        /// </summary>
        private void SaveAllWindows()
        {
            foreach (Window w in Application.Current.Windows)
            {
                AppSettings.SaveWindow(w);
            }
        }

        /// <summary>
        /// Обработка таймера 1 раз в секунду.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TimerWork(object sender, EventArgs e)
        {
            OnTimerWork();

            // проверяем элементы управления - изменились ли они
            // testControlValuesOnTimeTick();
            //// индикация подключения, скорости
            ////TimeLabel.Content = _EGSE.ETime.ToString();
            
            if (_intfEGSE.Connected)
            {
                ConnectionLabel.Background = Brushes.LightGreen;
                ConnectionLabel.Content = BUKConst.DeviceName + Resource.Get("stConnected");
            }
            else
            {
                ConnectionLabel.Background = Brushes.Red;
                ConnectionLabel.Content = BUKConst.DeviceName + Resource.Get("stDisconnected");

                // инициализируем все экранные формы на значения по-умолчанию при отключении от устройства
                // DefaultScreenInit();
                // hsiWin.Clear
            }
             
            // SpeedLabel.Content = Converter.SpeedToStr(_EGSE.Device.Speed) + " [" + _EGSE.Device.GlobalBufferSize.ToString() + "]";
        }

        /// <summary>
        /// Вызывается при закрытии приложения.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // сохраняем все настройки приложения
            SaveAllWindows();
            SaveAppSettings();

            // закрываем окна и устройства
            CloseAll();

            // закрываем лог-файлы
            LogsClass.LogMain.LogText = "Программа завершена";

            Application.Current.Shutdown();
        }

        /// <summary>
        /// Закрываем все окна, кроме основного, так как оно само закрывается
        /// И отключаемся от устройства
        /// </summary>
        private void CloseAll()
        {
            //// Window mainWin = Window.GetWindow(this);
            
            foreach (Window w in Application.Current.Windows)
            {
                if (w != Application.Current.MainWindow)
                {
                    w.Close();
                }
            }
            
            _intfEGSE.Device.FinishAll();
        }

        /// <summary>
        /// Handles the Click event of the CloseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the AboutButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Mouses the logger event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void MouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = EventClickToString.ElementClicked(e);
            if (logEvent != null)
            {
                LogsClass.LogOperator.LogText = logEvent;
            }
        }

        /// <summary>
        /// Handles the Activated event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Window_Activated(object sender, EventArgs e)
        {
            // checkWindowsActivation();
        }
    }

    ////public class TestC : INotifyPropertyChanged
    ////{
    ////    private bool _isWinOpened;

    ////    public bool IsWinOpened
    ////    {
    ////        get 
    ////        { 
    ////            return _isWinOpened; 
    ////        }

    ////        set
    ////        {
    ////            _isWinOpened = value;
    ////            FirePropertyChangedEvent("IsWinOpened");
    ////        }
    ////    }

    ////    public event PropertyChangedEventHandler PropertyChanged;

    ////    private void FirePropertyChangedEvent(string propertyName)
    ////    {
    ////        if (PropertyChanged != null)
    ////        {
    ////            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    ////        }
    ////    }
    ////}
}
