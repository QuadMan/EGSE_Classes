//-----------------------------------------------------------------------
// <copyright file="EGSEDefaultMainWindow.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.WPF
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
    using EGSE.Utilites;
    using EGSE.Utilites.ADC;
    using EGSE.Constants;
    using EGSE.Defaults;

    /// <summary>
    /// Класс, содержащий "неизменяемые" методы и поля-свойства основного окна.
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer _dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();
            base.Title = BUKConst.ShowCaption;

            InitControlValues();
            loadWindows();
            InitModules();

            LoadAppSettings();

            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(timerWork);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
            _intfEGSE.Device.Start();

        }

        /// <summary>
        /// Загружаем параметры окон из конфигурационного файла
        /// </summary>
        private void loadWindows()
        {
            foreach (Window w in Application.Current.Windows)
            {
                AppSettings.LoadWindow(w);
            }
        }

        /// <summary>
        /// Сохраняем параметры окно в конфигурационном файле
        /// </summary>
        private void saveAllWindows()
        {
            foreach (Window w in Application.Current.Windows)
            {
                AppSettings.SaveWindow(w);
            }
        }

        /// <summary>
        /// Обработка таймера 1 раз в секунду
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerWork(object sender, EventArgs e)
        {
            OnTimerWork();
            // проверяем элементы управления - изменились ли они
            //testControlValuesOnTimeTick();
            // индикация подключения, скорости
            //TimeLabel.Content = _EGSE.ETime.ToString();
            
            /*if (_EGSE.Connected)
            {
                ConnectionLabel.Background = Brushes.LightGreen;
                ConnectionLabel.Content = BUKConst.DeviceName + " подключен";
            }
            else
            {
                ConnectionLabel.Background = Brushes.Red;
                ConnectionLabel.Content = BUKConst.DeviceName + " отключен";

                // инициализируем все экранные формы на значения по-умолчанию при отключении от устройства
                DefaultScreenInit();
                //hsiWin.Cle
            }*/
             
            //SpeedLabel.Content = Converter.SpeedToStr(_EGSE.Device.Speed) + " [" + _EGSE.Device.GlobalBufferSize.ToString() + "]";
        }

        /// <summary>
        /// Вызывается при закрытии приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // сохраняем все настройки приложения
            saveAllWindows();
            SaveAppSettings();
            // закрываем окна и устройства
            closeAll();
            // закрываем лог-файлы
            LogsClass.LogMain.LogText = "Программа завершена";

            Application.Current.Shutdown();
        }


        /// <summary>
        /// Закрываем все окна, кроме основного, так как оно само закрывается
        /// И отключаемся от устройства
        /// </summary>
        private void closeAll()
        {
            //Window mainWin = Window.GetWindow(this);
            
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
        /// При нажатии на кнопку "Выйти"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Кнопка "О программе"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Для отлова нажатия на кнопки-чекбоксы и т.д.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseLoggerEvent(object sender, MouseButtonEventArgs e)
        {
            string logEvent = EventClickToString.ElementClicked(e);
            if (logEvent != null)
            {
                LogsClass.LogOperator.LogText = logEvent;
            }
        }

        /// <summary>
        /// При активации окна проверяем, чтобы дочерние окна были видимы, если установлены чекбоксы соответствущие 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Activated(object sender, EventArgs e)
        {
            //checkWindowsActivation();
        }

    }

    public class TestC : INotifyPropertyChanged
    {
        private bool _isWinOpened;

        public bool IsWinOpened
        {
            get { return _isWinOpened; }
            set
            {
                _isWinOpened = value;
                FirePropertyChangedEvent("IsWinOpened");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
