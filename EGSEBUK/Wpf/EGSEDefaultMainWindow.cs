//-----------------------------------------------------------------------
// <copyright file="EGSEDefaultMainWindow.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Defaults
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;
    using Egse.Constants;
    using Egse.Utilites;

    /// <summary>
    /// Общий класс расширений.
    /// </summary>
    public static class Extensions
    {   
        /// <summary>
        /// Для очистки списка монитора.
        /// </summary>
        /// <param name="list">Экземпляр монитора ListBox.</param>
        public delegate void ClearMonitorDelegate(ListBox list);

        /// <summary>
        /// Для добавления сообщения в список монитора.
        /// </summary>
        /// <param name="list">Экземпляр монитора ListBox.</param>
        /// <param name="msg">Сообщение для добавления.</param>
        public delegate void AddToMonitorDelegate(ListBox list, string msg);

        /// <summary>
        /// Для получения атрибута описания элемента перечисления.
        /// </summary>
        /// <param name="value">Элемент перечисления.</param>
        /// <returns>Описания элемента перечисления.</returns>
        public static string Description(this Enum value)
        {
            Type enumType = value.GetType();
            FieldInfo field = enumType.GetField(value.ToString());
            object[] attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length == 0 ? value.ToString() : ((DescriptionAttribute)attributes[0]).Description;
        }

        /// <summary>
        /// Безопасно очищает список монитора.
        /// </summary>
        /// <param name="list">Экземпляр монитора ListBox.</param>
        public static void ClearMonitor(ListBox list)
        {
            new { list }.CheckNotNull();

            Application.Current.Dispatcher.BeginInvoke(new ClearMonitorDelegate(ClearMonitorInvoke), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { list });
        }

        /// <summary>
        /// Безопасно добавляет сообщение в список монитора.
        /// </summary>
        /// <param name="list">Экземпляр монитора ListBox.</param>
        /// <param name="msg">Сообщение для добавления.</param>
        public static void AddToMonitor(ListBox list, string msg)
        {
            new { list }.CheckNotNull();

            Application.Current.Dispatcher.BeginInvoke(new AddToMonitorDelegate(AddToMonitorInvoke), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { list, msg });
        }
        
        /// <summary>
        /// Получает экземпляр дочернего визуального элемента.
        /// </summary>
        /// <typeparam name="T">Тип экземпляра дочернего визуального элемента.</typeparam>
        /// <param name="referenceVisual">The reference visual.</param>
        /// <returns>Экземпляр дочернего визуального элемента.</returns>
        public static T GetVisualChild<T>(this Visual referenceVisual) where T : Visual
        {
            Visual child = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(referenceVisual); i++)
            {
                child = VisualTreeHelper.GetChild(referenceVisual, i) as Visual;
                if (child != null && child is T)
                {
                    break;
                }
                else if (child != null)
                {
                    child = GetVisualChild<T>(child);
                    if (child != null && child is T)
                    {
                        break;
                    }
                }
            }

            return child as T;
        }

        /// <summary>
        /// Для очистки монитора.
        /// Примечание:
        /// Вызывается в потоке, создавшем экземпляр ListBox.
        /// </summary>
        /// <param name="list">Экземпляр монитора ListBox.</param>
        /// <exception cref="Egse.Defaults.Extensions.NotSupportMonitorListException">Если экземпляр монитора ListBox не инициализировал DataContext.</exception>
        private static void ClearMonitorInvoke(ListBox list)
        {
            if (!(list.DataContext is MonitorListViewModel))
            {
                throw new NotSupportMonitorListException(Resource.Get(@"eNotSupportMonitorList"));
            }

            (list.DataContext as MonitorListViewModel).MonitorListItem.Clear();
        }

        /// <summary>
        /// Для добавления сообщения в список монитора.
        /// Примечание:
        /// Вызывается в потоке, создавшем экземпляр ListBox.
        /// </summary>
        /// <param name="list">Экземпляр монитора ListBox.</param>
        /// <param name="msg">Сообщение для добавления.</param>
        /// <exception cref="Egse.Defaults.Extensions.NotSupportMonitorListException">Если экземпляр монитора ListBox не инициализировал DataContext.</exception>
        private static void AddToMonitorInvoke(ListBox list, string msg)
        {
            if (!(list.DataContext is MonitorListViewModel)) 
            { 
                throw new NotSupportMonitorListException(Resource.Get(@"eNotSupportMonitorList")); 
            } 
            
            (list.DataContext as MonitorListViewModel).MonitorListItem.Add(msg); 
            if ((bool)list.GetValue(ListBoxExtensions.IsScrollingProperty)) 
            { 
                list.ScrollIntoView(msg); 
            } 
        }

        /// <summary>
        /// Возникает при попытке добавить/очистить список мониторов.
        /// </summary>
        internal class NotSupportMonitorListException : ApplicationException
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="NotSupportMonitorListException" />.
            /// </summary>
            /// <param name="message">A message that describes the error.</param>
            public NotSupportMonitorListException(string message)
                : base(message)
            {
            }
        }
    }

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
                        
            InitModules();
            LoadWindows();
            InitControlValues();
            LoadAppSettings();

            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(TimerWork);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            _dispatcherTimer.Start();
            _intfEGSE.Device.Start();
        }

        /// <summary>
        /// Occurs when [got load application event].
        /// </summary>
        public event Action GotLoadAppEvent;

        /// <summary>
        /// Occurs when [got save application event].
        /// </summary>
        public event Action GotSaveAppEvent;

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
            
            if (_intfEGSE.IsConnected)
            {
                ConnectionLabel.Background = Brushes.PaleGreen;
                ConnectionLabel.Content = Global.DeviceName + Resource.Get("stConnected");
            }
            else
            {
                ConnectionLabel.Background = Brushes.Tomato;
                ConnectionLabel.Content = Global.DeviceName + Resource.Get("stDisconnected");

                // инициализируем все экранные формы на значения по-умолчанию при отключении от устройства
                // DefaultScreenInit();
                // hsiWin.Clear
            }
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
    }

    public class ShortToStrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "0x" + ((short)value).ToString("X");            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short val;
            if (short.TryParse(((string)value).TrimStart(new char[] {'0','x'}), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val))
            {
                return val;
            }
            else
            {
                return null;
            }
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
    /// Конвертор EgseTime to string для wpf.
    /// </summary>
    public class EgseTimeToStrConverter : IValueConverter
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
            if (null != value)
            {
                return ((EgseTime)value).ToString();
            }
            else
            {
                return string.Empty;
            }
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
            return null;
        }
    }

    /// <summary>
    /// Конвертор DeviceSpeed to string для wpf.
    /// </summary>
    public class DeviceSpeedToStrConverter : IValueConverter
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
            return string.Format(Resource.Get(@"stDeviceSpeed"), this.ReadableByte((float)value));             
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
            return null;
        }
    }

    /// <summary>
    /// Конвертор DeviceTrafic to string для wpf.
    /// </summary>
    public class DeviceTraficToStrConverter : IValueConverter
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
            return string.Format(Resource.Get(@"stDeviceTrafic"), this.ReadableByte((float)((long)value)));
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
            return null;
        }
    }

    /// <summary>
    /// Конвертор enum to bool для wpf.
    /// </summary>
    public class EnumToBoolConverter : IValueConverter 
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
            if (parameter.Equals(value)) 
            {
                return true; 
            }
            else 
            {
                return false; 
            }
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
            if (((bool)value) == true)
            {
                return parameter;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        } 
    } 

    /// <summary>
    /// Конвертор array to string для wpf.
    /// </summary>
    public class BytesToStrConverter : IValueConverter
    {
        /// <summary>
        /// Количество системных байт в начале массива.
        /// Примечание:
        /// Используется для приведения типа object к byte[].
        /// </summary>
        private const int ObjHeaderSysBytesCount = 27;

        /// <summary>
        /// Количество системных байт в конце массива.
        /// Примечание:
        /// Используется для приведения типа object к byte[].
        /// </summary>
        private const int ObjEnderSysBytesCount = 1;

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
            if (null == value)
            {
                return string.Empty;
            }

            byte[] buf = ObjectToByteArray(value);
            if (0 == buf.Length)
            {
                return " ";
            }

            return Converter.ByteArrayToHexStr(buf);            
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
            return Converter.HexStrToByteArray((string)value);
        }

        /// <summary>
        /// Приведение типа object к byte[].
        /// </summary>
        /// <param name="obj">Переменная типа object.</param>
        /// <returns>Полученный массив.</returns>
        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            byte[] buf;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                (new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()).Serialize(ms, obj);
                buf = ms.ToArray().Take<byte>((int)(ms.Length - ObjEnderSysBytesCount)).ToArray().Skip<byte>(ObjHeaderSysBytesCount).ToArray();
            }

            return buf;
        }
    }

    /// <summary>
    /// Используется для организации вывода в ComboBox элементов перечисления (используя аргументы Description).
    /// </summary>
    public class EnumerationExtension : MarkupExtension
    {
        /// <summary>
        /// Тип перечисления.
        /// </summary>
        private Type _enumType;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EnumerationExtension" />.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <exception cref="System.ArgumentNullException">Ошибка в аргументе.</exception>
        public EnumerationExtension(Type enumType)
        {
            if (null == enumType)
            {
                throw new ArgumentNullException("enumType");
            }

            EnumType = enumType;
        }

        /// <summary>
        /// Получает тип перечисления.
        /// </summary>
        /// <value>
        /// The type of the enum.
        /// </value>
        /// <exception cref="System.ArgumentException">Type must be an Enum.</exception>
        public Type EnumType
        {
            get 
            { 
                return _enumType; 
            }

            private set
            {
                if (value == _enumType)
                {
                    return;
                }

                var enumType = Nullable.GetUnderlyingType(value) ?? value;
                if (false == enumType.IsEnum)
                {
                    throw new ArgumentException("Type must be an Enum.");
                }
                    
                _enumType = value;
            }
        }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var enumValues = Enum.GetValues(EnumType);
            return (from object enumValue in enumValues select new EnumerationMember { Value = enumValue, Description = GetDescription(enumValue) }).ToArray();
        }

        /// <summary>
        /// Получает описание элемента перечисления.
        /// </summary>
        /// <param name="enumValue">Элемент перечисления.</param>
        /// <returns>Описание элемента перечисления.</returns>
        private string GetDescription(object enumValue)
        {
            var descriptionAttribute = EnumType.GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return null != descriptionAttribute ? descriptionAttribute.Description : enumValue.ToString();
        }

        /// <summary>
        /// Организация вывода в список.
        /// </summary>
        public class EnumerationMember
        {
            /// <summary>
            /// Получает или задает описание элемента.
            /// </summary>
            /// <value>
            /// Описание элемента.
            /// </value>
            public string Description { get; set; }

            /// <summary>
            /// Получает или задает значение элемента.
            /// </summary>
            /// <value>
            /// Значение элемента.
            /// </value>
            public object Value { get; set; }
        }
    }

    /// <summary>
    /// Расширение для мониторов ListBox.
    /// </summary>
    public class ListBoxExtensions : DependencyObject
    {
        /// <summary>
        /// Свойство показывает, что необходимо прокручивать монитор.
        /// </summary>
        public static readonly DependencyProperty IsScrollingProperty = DependencyProperty.RegisterAttached("IsScrolling", typeof(bool), typeof(ListBoxExtensions), new UIPropertyMetadata(default(bool)));

        /// <summary>
        /// Свойство показывает, что нужно использовать автоматическое прокручивание.
        /// </summary>
        public static readonly DependencyProperty IsAutoscrollProperty = DependencyProperty.RegisterAttached("IsAutoscroll", typeof(bool), typeof(ListBoxExtensions), new UIPropertyMetadata(default(bool), OnIsAutoscrollChanged));
 
        /// <summary>
        /// Gets the is scrolling.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Значение свойства IsSrolling</returns>
        public static bool GetIsScrolling(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsScrollingProperty);
        }

        /// <summary>
        /// Sets the is scrolling.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">Новое значение свойства IsScrolling</param>
        public static void SetIsScrolling(DependencyObject obj, bool value)
        {
            obj.SetValue(IsScrollingProperty, value);
        }

        /// <summary>
        /// Gets the is autoscroll.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Значение свойства IsAutoscroll</returns>
        public static bool GetIsAutoscroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsAutoscrollProperty);
        }

        /// <summary>
        /// Sets the is autoscroll.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">Новое значение свойства IsAutoscroll</param>
        public static void SetIsAutoscroll(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAutoscrollProperty, value);
        }

        /// <summary>
        /// Called when [is autoscroll changed].
        /// </summary>
        /// <param name="s">Экземпляр ListBox</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.ArgumentException">Если экземпляр связанного объекта не является ListBox</exception>
        public static void OnIsAutoscrollChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            if (!(s is ListBox))
            {
                throw new ArgumentException("s");
            }

            ListBox listBox = s as ListBox;
            
            if ((bool)e.NewValue) 
            {
                listBox.Loaded += (sender, m) =>
                {
                    ScrollViewer scrollViewer = listBox.GetVisualChild<ScrollViewer>();
                    if (scrollViewer != null)
                    {
                        ScrollBar scrollBar = scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer) as ScrollBar;
                        if (scrollBar != null)
                        {
                            s.SetValue(IsScrollingProperty, true);
                            scrollBar.ValueChanged += delegate
                            {
                                s.SetValue(IsScrollingProperty, scrollViewer.VerticalOffset + scrollViewer.ViewportHeight >= listBox.Items.Count - 2);
                            };
                        }
                    }
                };
            }
        }
    }

    /// <summary>
    /// MVVM для мониторов ListBox.
    /// </summary>
    public class MonitorListViewModel
    {
        /// <summary>
        /// The maximum count item in monitor list.
        /// </summary>
        private const int MaxCountItemInMonitorList = 10000;

        /// <summary>
        /// The _monitor list
        /// </summary>
        private ObservableCollection<string> monitorList;

        /// <summary>
        /// Получает список данных/команд монитора spacewire/hsi.
        /// </summary>
        /// <value>
        /// Список данных/команд монитора spacewire/hsi.
        /// </value>
        public ObservableCollection<string> MonitorListItem
        {
            get
            {
                if (null == this.monitorList)
                {
                    this.monitorList = new ObservableCollection<string>();
                }

                if (MaxCountItemInMonitorList < monitorList.Count)
                {
                    this.monitorList.RemoveAt(0);
                }

                return this.monitorList;
            }
        }
    }
}
