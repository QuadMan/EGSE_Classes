﻿//-----------------------------------------------------------------------
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
    using System.Globalization;
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
            Application.Current.MainWindow = this;
            Title = Global.ShowCaption;            

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
        /// Загружаем параметры окон из конфигурационного файла.
        /// </summary>
        private void LoadWindows()
        {
            foreach (Window w in Application.Current.Windows)
            {
                AppSettings.LoadWindow(w);
            }
        }

        /// <summary>
        /// Сохраняем параметры окно в конфигурационном файле.
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
                ConnectionLabel.Content = Global.DeviceName + Resource.Get("stConnected");
            }
            else
            {
                ConnectionLabel.Background = Brushes.Red;
                ConnectionLabel.Content = Global.DeviceName + Resource.Get("stDisconnected");

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
        /// Закрываем все окна, кроме основного, так как оно само закрывается.
        /// И отключаемся от устройства.
        /// </summary>
        private void CloseAll()
        {
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

    /// <summary>
    /// Конвертор bool to int для wpf.
    /// </summary>
    public class BoolToIntConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value == true) ? "true" : "false";
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value == "true") ? true : false;
        }
    }
    /// <summary>
    /// Конвертор string to array для wpf.
    /// </summary>
    public class StrToBytesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
            {
                return string.Empty;
            }            
            byte[] source = ObjectToByteArray(value);
            byte[] dest = new byte[source.Length - 28];
            Array.Copy(source, 27, dest, 0, source.Length - 28);
            return Converter.ByteArrayToHexStr(dest);            
        }
        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value)
            {
                return new byte[] { };
            }
            return Converter.HexStrToByteArray((string)value); 
        }
    }
}
