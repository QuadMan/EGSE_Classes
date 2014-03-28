//-----------------------------------------------------------------------
// <copyright file="EGSEUtilites.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Мурзин Святослав</author>
//-----------------------------------------------------------------------

// TODO в bigBufferManager ввести признак переполнения буфера!
// TODO в классе BigBuff попробовать уйти от lock(this) в сторону InterlockedIncrement
namespace EGSE.Utilites
{    
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Windows;
    using EGSE.Protocols;

    /// <summary>
    /// Предоставляет проверку контроля CRC-16.
    /// </summary>
    public static class Crc16
    {
        // старший и младший байты должны быть поменяны местами

        /// <summary>
        /// CRC16 таблица.
        /// </summary>
        private static readonly ushort[] Crc16Table = new ushort[]
            {
                0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
                0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
                0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
                0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
                0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485,
                0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
                0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4,
                0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
                0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
                0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
                0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12,
                0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
                0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41,
                0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
                0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
                0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
                0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F,
                0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
                0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E,
                0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
                0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
                0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
                0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C,
                0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
                0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB,
                0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
                0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
                0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
                0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9,
                0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
                0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8,
                0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0
            };

        /// <summary>
        /// Получает расчитанный CRC, для указанных данных.
        /// </summary>
        /// <param name="bytes">Массив данных.</param>
        /// <param name="len">Сколько байт учавствует в расчете CRC.</param>
        /// <param name="start">Индекс байта, с которого нужно считать CRC.</param>
        /// <param name="crc">CRC с которого надо начать (для "кусочного" подсчета).</param>
        /// <returns>Значение CRC.</returns>
        public static ushort Get(byte[] bytes, int len, int start = 0, ushort crc = 0xFFFF)
        {
            for (var i = start; i < len; i++)
            {
                crc = (ushort)((crc << 8) ^ Crc16Table[(crc >> 8) ^ bytes[i]]);
            }

            return crc;
        }
    }

    /// <summary>
    /// Для преобразования сообщений spacewire к сообщениям верхнего уровня.
    /// </summary>
    public static class MsgWorker
    {
        // приведение к типу сообщения от "сырых" массивов данных   
    
        /// <summary>
        /// Декодировать как spacewire icd сообщение.
        /// </summary>
        /// <param name="obj">"сырые" данные послыки.</param>
        /// <returns>Icd spacewire-сообщение.</returns>
        public static SpacewireIcdMsgEventArgs AsIcd(this byte[] obj)
        {
            return new SpacewireIcdMsgEventArgs(obj, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Декодировать как spacewire кбв сообщение.
        /// </summary>
        /// <param name="obj">"сырые" данные послыки.</param>
        /// <returns>КБВ spacewire-сообщение.</returns>
        public static SpacewireObtMsgEventArgs AsKbv(this byte[] obj)
        {
            return new SpacewireObtMsgEventArgs(obj, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Декодировать как spacewire сообщение телеметрии.
        /// </summary>
        /// <param name="obj">"сырые" данные послыки.</param>
        /// <returns>spacewire-сообщение телеметрии.</returns>
        public static SpacewireTmMsgEventArgs AsTm(this byte[] obj)
        {
            return new SpacewireTmMsgEventArgs(obj, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Декодировать как spacewire сообщение телекоманды.
        /// </summary>
        /// <param name="obj">"сырые" данные послыки.</param>
        /// <returns>spacewire-сообщение телекоманды.</returns>
        public static SpacewireTkMsgEventArgs AsTk(this byte[] obj)
        {
            return new SpacewireTkMsgEventArgs(obj, 0x00, 0x00, 0x00);
        }

        /// <summary>
        /// Декодировать как spacewire sptp сообщение.
        /// </summary>
        /// <param name="obj">"сырые" данные послыки.</param>
        /// <returns>sptp spacewire-сообщение.</returns>
        public static SpacewireSptpMsgEventArgs AsSptp(this byte[] obj)
        {
            return new SpacewireTkMsgEventArgs(obj, 0x00, 0x00, 0x00);
        }

        // создание новых сообщений

        /// <summary>
        /// Сформировать spacewire-сообщение sptp протокола.
        /// </summary>
        /// <param name="obj">"сырые" данные послыки.</param>
        /// <param name="to">Адрес прибора назначения.</param>
        /// <param name="from">Адрес прибора инициатора.</param>
        /// <returns>sptp spacewire-сообщение.</returns>
        public static SpacewireSptpMsgEventArgs ToSptp(this byte[] obj, byte to, byte from)
        {
            return SpacewireSptpMsgEventArgs.GetNew(obj, to, from);
        }

        /// <summary>
        /// Сформировать spacewire телекоманду.
        /// </summary>
        /// <param name="obj">"сырые" данные послыки.</param>
        /// <param name="to">Адрес прибора назначения.</param>
        /// <param name="from">Адрес прибора инициатора.</param>
        /// <param name="apid">APID прибора назначения.</param>
        /// <returns>
        /// spacewire-сообщение телекоманды.
        /// </returns>
        public static SpacewireTkMsgEventArgs ToTk(this byte[] obj, byte to, byte from, short apid)
        {
            return SpacewireTkMsgEventArgs.GetNew(obj, to, from, apid);
        }
    }

    /// <summary>
    /// Для проверки входных аргументов на null.
    /// Использовать:
    /// internal void SomeName(int arg1, bool arg2, byte[] arg3)
    /// {
    ///     new { arg1, arg2, arg3 }.CheckNotNull();
    /// }
    /// </summary>
    public static class NullCheckers
    {
        /// <summary>
        /// Проверяет аргумент на null.
        /// </summary>
        /// <typeparam name="T">Тип аргумента.</typeparam>
        /// <param name="container">Список аргументов.</param>
        /// <exception cref="System.ArgumentNullException">Если пытаются проверить пустой список.</exception>
        public static void CheckNotNull<T>(this T container) where T : class
        {
            if (container == null)
            {
                throw new ArgumentNullException(@"container");
            }

            NullChecker<T>.Check(container);
        }

        /// <summary>
        /// Формирует список аргументов, использую рефлексию получает имена аргументов, проверяет на null.
        /// </summary>
        /// <typeparam name="T">Класс аргумента.</typeparam>
        private static class NullChecker<T> where T : class
        {
            /// <summary>
            /// Список аргументов, подлежащих проверки на null.
            /// </summary>
            private static readonly List<Func<T, bool>> Checkers;

            /// <summary>
            /// Список наименований аргументов.
            /// </summary>
            private static readonly List<string> Names;

            /// <summary>
            /// Инициализирует статические поля класса <see cref="NullChecker{T}" />.
            /// </summary>
            /// <exception cref="System.ArgumentException">Если аргумент является свойством класса.</exception>
            static NullChecker()
            {
                Checkers = new List<Func<T, bool>>();
                Names = new List<string>();
                foreach (string name in typeof(T).GetConstructors()[0].GetParameters().Select(p => p.Name))
                {
                    Names.Add(name);
                    PropertyInfo property = typeof(T).GetProperty(name);
                    if (property.PropertyType.IsValueType)
                    {
                        throw new ArgumentException(string.Format(@"Свойство {0} является типом значения.", property));
                    }

                    ParameterExpression param = System.Linq.Expressions.Expression.Parameter(typeof(T), @"container");
                    System.Linq.Expressions.Expression propertyAccess = System.Linq.Expressions.Expression.Property(param, property);
                    System.Linq.Expressions.Expression nullValue = System.Linq.Expressions.Expression.Constant(null, property.PropertyType);
                    System.Linq.Expressions.Expression equality = System.Linq.Expressions.Expression.Equal(propertyAccess, nullValue);
                    var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equality, param);
                    Checkers.Add(lambda.Compile());
                }
            }

            /// <summary>
            /// Вызывает проверку на список аргументов.
            /// </summary>
            /// <param name="item">Список аргументов.</param>
            /// <exception cref="System.ArgumentNullException">Если аргумент равен null.</exception>
            internal static void Check(T item)
            {
                for (int i = 0; i < Checkers.Count; i++)
                {
                    if (Checkers[i](item))
                    {
                        throw new ArgumentNullException(Names[i]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Класс сохранения настроек приложения в Ini файл.
    /// Сохраняет и отдельные свойства и настройки окон: позицию, размеры, состояние(развернуто/свернуто) окна, видимость.
    /// </summary>
    public static class AppSettings
    {
        /// <summary>
        /// Максимальное количество элементов в ini-файле.
        /// </summary>
        private const int MaxItemsCount = 100;

        /// <summary>
        /// Записываем параметр param в секцию section, значением value.
        /// </summary>
        /// <param name="param">Параметр приложения.</param>
        /// <param name="value">Значение параметра.</param>
        /// <param name="section">Секция, по-умолчанию, MAIN.</param>
        /// <returns>True - если все хорошо.</returns>
        public static bool Save(string param, string value, string section = "MAIN")
        {
            try
            {
                IniFile _ini = new IniFile();
                _ini.Write(param, value, section);
                return true;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Загружаем параметр из файла
        /// </summary>
        /// <param name="param">Название параметра</param>
        /// <param name="section">Секция параметра</param>
        /// <returns>Строка-значение параметра, null - если параметр не найден</returns>
        public static string Load(string param, string section = "MAIN")
        {
            try
            {
                IniFile _ini = new IniFile();
                if (_ini.IsKeyExists(param, section))
                {
                    return _ini.Read(param, section);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Метод сохраняет в INI файл список.
        /// </summary>
        /// <param name="strList">Список для сохранения</param>
        /// <param name="section">Секция, в которую пишем</param>
        /// <returns>True - если сохранено</returns>
        public static bool SaveList(List<string> strList, string section)
        {
            if ((strList == null) || (strList.Count == 0))
            {
                return false;
            }

            try
            {
                IniFile _ini = new IniFile();
                int i = 0;
                foreach (string s in strList)
                {
                    _ini.Write("Item" + i.ToString(), s, section);
                    if (i++ == MaxItemsCount)
                    {
                        break;
                    }
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Загружаем элементы из секции в список.
        /// Отбираем элементы ItemN, где N должен быть от 0 до MAX_ITEMS_COUNT-1.
        /// </summary>
        /// <param name="strList">Список, в который загружаем элементы.</param>
        /// <param name="section">Секция, из которой загружаем элементы списка.</param>
        /// <returns>True - если выполнено успешно.</returns>
        public static bool LoadList(List<string> strList, string section)
        {
            if (strList == null)
            {
                return false;
            }

            try
            {
                IniFile _ini = new IniFile();
                int i = 0;
                string str = string.Empty;
                strList.Clear();
                while (i < MaxItemsCount)
                {
                    str = "Item" + i.ToString();
                    if (_ini.IsKeyExists(str, section))
                    {
                        strList.Add(_ini.Read(str, section));
                    }
                    else
                    {
                        break;
                    }

                    i++;
                }
            }
            catch
            {
                throw;
            }

            return true;
        }
        
        /// <summary>
        /// Сохраняем параметры окна.
        /// </summary>
        /// <param name="win">Экземпляр окна.</param>
        /// <returns>true, если функция выполнена успешно.</returns>
        public static bool SaveWindow(Window win)
        {
            try
            {
                IniFile _ini = new IniFile();
                ////string sectionName = GetPropertyValue(win, Window.TitleProperty);
                string sectionName = win.GetType().ToString();
                _ini.Write(@"Visibility", GetPropertyValue(win, Window.VisibilityProperty), sectionName);
                _ini.Write(@"WindowState", GetPropertyValue(win, Window.WindowStateProperty), sectionName);
                _ini.Write(@"Left", GetPropertyValue(win, Window.LeftProperty), sectionName);                
                _ini.Write(@"Top", GetPropertyValue(win, Window.TopProperty), sectionName);
                _ini.Write(@"Width", GetPropertyValue(win, Window.WidthProperty), sectionName);
                _ini.Write(@"Height", GetPropertyValue(win, Window.HeightProperty), sectionName);   
                return true;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Загружаем сохраненные параметры окна.
        /// </summary>
        /// <param name="win">Экземпляр окна.</param>
        /// <returns>true, если функция выполнена успешно.</returns>
        public static bool LoadWindow(Window win)
        {
            try
            {
                IniFile _ini = new IniFile();
                ////string sectionName = GetPropertyValue(win, Window.TitleProperty);
                string sectionName = win.GetType().ToString();
                if (_ini.IsKeyExists(@"Visibility", sectionName))
                {
                    SetPropertyValue(win, Window.VisibilityProperty, _ini.Read(@"Visibility", sectionName));
                }
                else
                {
                    _ini.Write(@"Visibility", GetPropertyValue(win, Window.VisibilityProperty), sectionName);
                }

                if (_ini.IsKeyExists(@"WindowState", sectionName))
                {
                    SetPropertyValue(win, Window.WindowStateProperty, _ini.Read(@"WindowState", sectionName));
                }
                else
                {
                    _ini.Write(@"WindowState", GetPropertyValue(win, Window.WindowStateProperty), sectionName);
                }

                if (_ini.IsKeyExists(@"Left", sectionName))
                {
                    SetPropertyValue(win, Window.LeftProperty, _ini.Read(@"Left", sectionName));
                }
                else
                {
                    _ini.Write(@"Left", GetPropertyValue(win, Window.LeftProperty), sectionName);
                }

                if (_ini.IsKeyExists(@"Top", sectionName))
                {
                    SetPropertyValue(win, Window.TopProperty, _ini.Read(@"Top", sectionName));
                }
                else
                {
                    _ini.Write(@"Top", GetPropertyValue(win, Window.TopProperty), sectionName);
                }

                if (_ini.IsKeyExists(@"Width", sectionName))
                {
                    SetPropertyValue(win, Window.WidthProperty, _ini.Read(@"Width", sectionName));
                }
                else
                {
                    _ini.Write(@"Width", GetPropertyValue(win, Window.WidthProperty), sectionName);
                }

                if (_ini.IsKeyExists(@"Height", sectionName))
                {
                    SetPropertyValue(win, Window.HeightProperty, _ini.Read(@"Height", sectionName));
                }
                else
                {
                    _ini.Write(@"Height", GetPropertyValue(win, Window.HeightProperty), sectionName);
                }   

                return true;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Получает значение связанного свойства окна.
        /// </summary>
        /// <param name="win">Экземпляр окна.</param>
        /// <param name="dp">Связанное свойство.</param>
        /// <returns>Текущее значение.</returns>
        private static string GetPropertyValue(Window win, DependencyProperty dp)
        {
            System.Windows.Data.Binding bindObj = System.Windows.Data.BindingOperations.GetBinding(win, dp);
            if (null != bindObj)
            {
                return Convert.ToString(win.DataContext.GetType().GetProperty(bindObj.Path.Path).GetValue(win.DataContext, null));
            }
            else
            {
                return Convert.ToString(win.GetType().GetProperty(dp.Name).GetValue(win, null));
            }
        }

        /// <summary>
        /// Задает значение связанного свойства окна.
        /// </summary>
        /// <param name="win">Экземпляр окна.</param>
        /// <param name="dp">Связанное свойство.</param>
        /// <param name="value">Новое значение.</param>
        private static void SetPropertyValue(Window win, DependencyProperty dp, string value)
        {
            try
            {
                System.ComponentModel.TypeConverter convertor = System.ComponentModel.TypeDescriptor.GetConverter(dp.PropertyType);
                System.Windows.Data.Binding bindObj = System.Windows.Data.BindingOperations.GetBinding(win, dp);
                if (null != bindObj)
                {
                    if (null != bindObj.Converter)
                    {
                        PropertyInfo prop = win.DataContext.GetType().GetProperty(bindObj.Path.Path);
                        object obj = System.ComponentModel.TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromString(value);
                        win.DataContext.GetType().GetProperty(bindObj.Path.Path).SetValue(win.DataContext, obj, null);
                    }
                    else
                    {
                        win.DataContext.GetType().GetProperty(bindObj.Path.Path).SetValue(win.DataContext, convertor.ConvertFromString(value), null);
                    }
                }
                else
                {
                    win.GetType().GetProperty(dp.Name).SetValue(win, convertor.ConvertFromString(value), null);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }

    /// <summary>
    /// Класс для работы со строковыми ресусрами.
    /// (Если define DEBUG, ищет дубликаты в ресурсах)
    /// </summary>
    public static class Resource
    {
        /// <summary>
        /// Формируется при первомм обращении к классу.
        /// </summary>
        private static ResourceManager[] _rm;

        /// <summary>
        /// Возвращает строку из ресурса, соответствующую заданому ключу.
        /// </summary>
        /// <param name="mark">Ключ строки</param>
        /// <returns>Значение строки в ресурсах</returns>
        public static string Get(string mark)
        {
            if (null == _rm)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                _rm = new ResourceManager[assembly.GetManifestResourceNames().Length];
                int i = 0;
                foreach (string s in assembly.GetManifestResourceNames())
                {
                    _rm[i++] = new ResourceManager(s.Replace(@".resources", string.Empty), assembly);
                }
            }
#if (DEBUG)
            string firstFindStr = null;
#endif
            foreach (ResourceManager rm in _rm)
            {
                try
                {
                    string findStr = rm.GetString(mark);
                    if ((null != findStr) && (findStr.Length > 0))
                    {
#if (DEBUG)
                        if (null == firstFindStr)
                        {
                            firstFindStr = findStr;
                        }
                        else
                        {
                            throw new Exception(@"В ресурсах обнаружен дубликат, ключ: " + mark + @", значение 1: " + firstFindStr + @", значение 2: " + findStr);
                        }
#else
                        return findStr;
#endif
                    }
                }
                catch (MissingManifestResourceException)
                {
                    continue;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
#if (DEBUG)
            if (null != firstFindStr)
            {
                return firstFindStr;
            }
#endif
            return @"Ресурс не найден";
        }
    }

    /// <summary>
    /// Класс конвертации различных величин
    /// </summary>
    public static class Converter
    {
        public static EgseTime AsEgseTime(this byte[] obj)
        {
            return MarshalTo<EgseTime>(obj);
        }

        /// <summary>
        /// Обращение байт в беззнаковом целом.
        /// </summary>
        /// <param name="value">Беззнаковое целое, в котором необходимо обратить байты.</param>
        /// <returns>Беззнаковое целое, с обращенными байтами.</returns>
        public static uint ReverseBytes(this uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        /// <summary>
        /// Строка шестнадцатеричных чисел в массив байт.
        /// </summary>
        /// <param name="hexStr">Строка HEX чисел, разделенных пробелами.</param>
        /// <returns>Массив байт или null, если HexStr пустая.</returns>
        public static byte[] HexStrToByteArray(string hexStr)
        {
            if (string.Empty == hexStr)
            {
                return null;
            }

            string[] hexValuesSplit = hexStr.Split(new string[] { " ", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            byte[] outBuf = new byte[hexValuesSplit.Length];
            int i = 0;
            foreach (string hex in hexValuesSplit)
            {
                if (!byte.TryParse(hex, NumberStyles.HexNumber, new CultureInfo("en-US"), out outBuf[i++]))
                {
                    return new byte[] { };
                }

                // outBuf[i++] = (byte)Convert.ToInt32(hex, 16);
            }

            return outBuf;
        }

        /// <summary>
        /// Строка шестнадцатеричных чисел в массив байт.
        /// </summary>
        /// <param name="hexValues">Массив HEX чисел (в виде строк)</param>
        /// <returns>
        /// Массив байт или null, если массив hexValues пуст
        /// </returns>
        public static byte[] HexStrToByteArray(string[] hexValues)
        {
            if ((hexValues == null) || (hexValues.Length == 0))
            {
                return null;
            }

            byte[] outBuf = new byte[hexValues.Length];
            int i = 0;
            foreach (string hex in hexValues)
            {
                outBuf[i++] = (byte)Convert.ToInt32(hex, 16);
            }

            return outBuf;
        }

        /// <summary>
        /// Функция преобразует массив байт в строку.
        /// Байты в строке будут разделяться строкой, заданной в delimeter.
        /// </summary>
        /// <param name="data">Массив байт.</param>
        /// <param name="delimeter">Разделитель между байтами в строке.</param>
        /// <param name="isSmart">если установлено <c>true</c> [попытается сократить строку].</param>
        /// <returns>
        /// Результирующая строка, возвращает пустую строку, если data = null.
        /// </returns>
        public static string ByteArrayToHexStr(byte[] data, string delimeter = " ", bool isSmart = false)
        {
            if ((data == null) || (0 >= data.Length))
            {
                return string.Empty;
            }

            string hex;

            if (isSmart && (20 < data.Length))
            {
                hex = BitConverter.ToString(data.Take(10).ToArray()) + "..." + BitConverter.ToString(data.Skip(data.Length - 10).ToArray());
            }
            else
            {
                hex = BitConverter.ToString(data);
            }

            hex = hex.Replace("-", delimeter);
            return hex;
        }

        /// <summary>
        /// Переводит скорость в строку (с добавлением размерности)
        /// </summary>
        /// <param name="speedInBytes">Скорость в байтах</param>
        /// <returns>Срока скорости</returns>
        public static string SpeedToStr(float speedInBytes)
        {
            float kbs = speedInBytes / 1024;
            if (kbs > 1000)
            {
                float mbs = kbs / 1024;
                if (mbs > 1000)
                {
                    return mbs.ToString("0.0") + " мб/сек";
                }

                return kbs.ToString("0.0") + " кб/сек";
            }

            return speedInBytes.ToString("0.0") + " байт/сек";
        }

        /// <summary>
        /// Переводит размер файла в строку (с добавлением размерности)
        /// </summary>
        /// <param name="fileSizeInBytes">Размер файла в байтах</param>
        /// <returns>Строка размера файла</returns>
        public static string FileSizeToStr(ulong fileSizeInBytes)
        {
            float kb = fileSizeInBytes / 1024;
            float mb = kb / 1024;
            float gb = mb / 1024;
            if (gb > 1)
            {
                return gb.ToString("0.0") + " ГБайт";
            }
            else if (mb > 1)
            {
                return mb.ToString("0.0") + " MБайт";
            }
            else if (kb > 1)
            {
                return kb.ToString("0.0") + " КБайт";
            }
            else
            {
                return fileSizeInBytes.ToString() + " байт";
            }
        }

        /// <summary>
        /// Преобразует структуру к массиву байт.
        /// </summary>
        /// <typeparam name="T1">Тип структуры необходимой для преобразования.</typeparam>
        /// <param name="data">Структура для преобразования в массив байты.</param>
        /// <param name="dynData">Если есть данные в сообщение, они сохранятся тут.</param>
        /// <returns>Образованный массив байт.</returns>
        internal static byte[] MarshalFrom<T1>(T1 data, ref byte[] dynData)
        {
            int rawSize = Marshal.SizeOf(typeof(T1));
            IntPtr buf = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(data, buf, true);
            byte[] rawData = new byte[rawSize + (null != dynData ? dynData.Length : 0)];
            Marshal.Copy(buf, rawData, 0, rawSize);
            Marshal.FreeHGlobal(buf);
            if (null != dynData)
            {
                Array.Copy(dynData, 0, rawData, rawSize, dynData.Length);
            }

            return rawData;
        }

        internal static T1 MarshalTo<T1>(byte[] data)
        {
            byte[] buf;
            return MarshalTo<T1>(data, out buf);
        }

        /// <summary>
        /// Преобразует байты к конкретной структуре.
        /// </summary>
        /// <typeparam name="T1">Тип необходимой структуры.</typeparam>
        /// <param name="data">Массив байт для преобразования.</param>
        /// <param name="dynData">Если есть динамические данные они сохранятся тут.</param>
        /// <returns>Полученная структура после преобразования.</returns>
        internal static T1 MarshalTo<T1>(byte[] data, out byte[] dynData)
        {
            GCHandle pinnedInfo = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var structure = Marshal.PtrToStructure(pinnedInfo.AddrOfPinnedObject(), typeof(T1));
                IntPtr ptr = new IntPtr(pinnedInfo.AddrOfPinnedObject().ToInt32() + Marshal.SizeOf(typeof(T1)));
                if (0 < data.Length - Marshal.SizeOf(typeof(T1)))
                {
                    dynData = new byte[data.Length - Marshal.SizeOf(typeof(T1))];
                    Marshal.Copy(ptr, dynData, 0, dynData.Length);
                }
                else
                {
                    dynData = new byte[] { };
                }

                return (T1)structure;
            }
            finally
            {
                if (pinnedInfo.IsAllocated)
                {
                    pinnedInfo.Free();
                }
            }
        }
    }

    /// <summary>
    /// Статический класс позволяет накладывать байтовый поток на структуру.
    /// </summary>
    public static class ByteArrayToStructure
    {
        /// Пример: объявляем структуру
        ///         [StructLayout(LayoutKind.Sequential,Pack = 1)]
        ///         struct testS
        ///         {
        ///                 byte header;
        ///                 int size;
        ///                 byte flag;
        ///         }
        ///         и вызываем функцию 
        ///         testS ts = ByteArrayToStructure.make\testS/(byteBuf);
        /// <summary>
        /// Преобразуем массив байт в структуру
        /// </summary>
        /// <typeparam name="T">Необходимя структура</typeparam>
        /// <param name="bytes">Байты для преобразования</param>
        /// <returns>Полученная структура</returns>
        public static T Make<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return stuff;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EgseTime
    {
        private byte b0;
        private byte b1;
        private byte b2;
        private byte b3;
        private byte b4;
        private byte b5;

        public int Day 
        { 
            get
            {
                return (b0 << 3) | (b1 >> 5);
            }            
        }

        public int Hour 
        { 
            get
            {
                return b1 & 0x1F;
            }            
        }

        public int Minute 
        { 
            get
            {
                return b2 >> 2;
            }            
        }

        public int Second 
        { 
            get
            {
                return ((b2 & 0x03) << 4) | (b3 >> 4);
            }            
        }

        public int Millisecond 
        { 
            get
            {
                return ((b3 & 0xF) << 4) | (b4 >> 6);
            }            
        }

        public int Microsecond 
        { 
            get
            {
                return ((b4 & 0x03) << 8) | b5;
            }            
        }

        public override string ToString()
        {
            return string.Format("{0:D2}#{1:D2}:{2:D2}:{3:D2}.{4:D3}.{5:D3}", Day, Hour, Minute, Second, Millisecond, Microsecond);
        }

        public static EgseTime Now()
        {          
            DateTime now = DateTime.Now;
            byte[] buf = new byte[6] { 0, (byte)now.Hour, (byte)((byte)(now.Minute << 2) | (byte)(now.Second >> 4)), (byte)((byte)(now.Second << 4) | (byte)(now.Millisecond >> 6)), (byte)(now.Millisecond << 2), 0 };
            return buf.AsEgseTime();
        }

        public byte[] ToArray()
        {
            return new byte[6] { b0, b1, b2, b3, b4, b5 };
        }
    }

    ///// <summary>
    ///// Класс работы с временем в КИА - позволяет декодировать и преобразовывать в строку заданное время.
    ///// Необходимо заполнить поле данных времени data (6 байт).
    ///// </summary>
    //public class EgseTime
    //{
    //    /// <summary>
    //    /// Размер кадра "Время" устройства (в байтах).
    //    /// </summary>
    //    private const int DefaultTimeSize = 6;

    //    /// <summary>
    //    /// Инициализирует новый экземпляр класса <see cref="EgseTime" />.
    //    /// </summary>
    //    public EgseTime()
    //    {
    //        Data = new byte[DefaultTimeSize];
    //        Day = 0;
    //        Hour = 0;
    //        Min = 0;
    //        Sec = 0;
    //        Msec = 0;
    //        Mcsec = 0;
    //    }

    //    /// <summary>
    //    /// Получает или задает данные времени (6 байт).
    //    /// </summary>
    //    public byte[] Data { get; set; }

    //    /// <summary>
    //    /// Получает параметр: День.
    //    /// </summary>
    //    public uint Day { get; private set; }

    //    /// <summary>
    //    /// Получает параметр: Час.
    //    /// </summary>
    //    public uint Hour { get; private set; }

    //    /// <summary>
    //    /// Получает параметр: Минута.
    //    /// </summary>
    //    public uint Min { get; private set; }

    //    /// <summary>
    //    /// Получает параметр: Секунда.
    //    /// </summary>
    //    public uint Sec { get; private set; }

    //    /// <summary>
    //    /// Получает параметр: Миллисекунда.
    //    /// </summary>
    //    public uint Msec { get; private set; }

    //    /// <summary>
    //    /// Получает параметр: Микросекунда.
    //    /// </summary>
    //    public uint Mcsec { get; private set; }

    //    /// <summary>
    //    /// Декодируем время из буфера в поля
    //    /// </summary>
    //    public void Decode()
    //    {
    //        Day = ((uint)Data[0] << 3) | ((uint)Data[1] >> 5);
    //        Hour = (uint)Data[1] & 0x1F;
    //        Min = (uint)Data[2] >> 2;
    //        Sec = ((uint)(Data[2] & 3) << 4) | ((uint)Data[3] >> 4);
    //        Msec = ((uint)(Data[3] & 0xF) << 4) | ((uint)Data[4] >> 6);
    //        Mcsec = ((uint)(Data[4] & 3) << 8) | (uint)Data[5];
    //    }

    //    /// <summary>
    //    /// Кодируем текущее время в буфер.
    //    /// </summary>
    //    public void Encode()
    //    {
    //        DateTime now = DateTime.Now;
    //        Data[0] = 0;
    //        Data[1] = (byte)now.Hour;
    //        Data[2] = (byte)(now.Minute << 2);
    //        Data[2] |= (byte)(now.Second >> 4);
    //        Data[3] = (byte)(now.Second << 4);
    //        Data[3] |= (byte)(now.Millisecond >> 6);
    //        Data[4] = (byte)(now.Millisecond << 2);
    //        Data[5] = 0;
    //    }

    //    /// <summary>
    //    /// Преобразуем время в строку.
    //    /// </summary>
    //    /// <returns>Cтроку в виде DD:HH:MM:SS:MSS:MCS</returns>
    //    public new string ToString()
    //    {
    //        Decode();
    //        StringBuilder sb = new StringBuilder();
    //        sb.Clear();
    //        sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}.{3:D3}.{4:D3}", Hour, Min, Sec, Msec, Mcsec);
    //        return sb.ToString();
    //    }
    //}

    /// <summary>
    /// Класс работы с ini-файлом
    /// </summary>
    public class IniFile
    {
        /// <summary>
        /// Путь к ini-файлу.
        /// </summary>
        private string _path;

        /// <summary>
        /// Имя exe-файла (Название группы параметра по-умолчанию).
        /// </summary>
        private string _exe = Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="IniFile" />.
        /// Конструктор конкретного ini-файла.
        /// </summary>
        /// <param name="iniPath">Полный путь к ini-файлу.</param>
        public IniFile(string iniPath = null)
        {
            _path = new FileInfo(iniPath ?? _exe + ".ini").FullName.ToString();
        }

        /// <summary>
        /// Считать параметр
        /// </summary>
        /// <param name="key">Название параметра</param>
        /// <param name="section">Название группы параметра</param>
        /// <returns>Значение параметра</returns>
        public string Read(string key, string section = null)
        {
            var retVal = new StringBuilder(255);
            SafeNativeMethods.GetPrivateProfileString(section ?? _exe, key, string.Empty, retVal, 255, _path);
            return retVal.ToString();
        }

        /// <summary>
        /// Записать параметр
        /// </summary>
        /// <param name="key">Название параметра</param>
        /// <param name="value">Значение параметра</param>
        /// <param name="section">Название группы параметра</param>
        public void Write(string key, string value, string section = null)
        {
            SafeNativeMethods.WritePrivateProfileString(section ?? _exe, key, value, _path);
        }

        /// <summary>
        /// Удалить параметр
        /// </summary>
        /// <param name="key">Название параметра</param>
        /// <param name="section">Название группы параметра</param>
        public void DeleteKey(string key, string section = null)
        {
            Write(key, null, section ?? _exe);
        }

        /// <summary>
        /// Удалить группу параметров
        /// </summary>
        /// <param name="section">Название группы параметра</param>
        public void DeleteSection(string section = null)
        {
            Write(null, null, section ?? _exe);
        }

        /// <summary>
        /// Проверка на существование параметра
        /// </summary>
        /// <param name="key">Название параметра</param>
        /// <param name="section">Название группы параметра</param>
        /// <returns>Результат проверки</returns>
        public bool IsKeyExists(string key, string section = null)
        {
            return Read(key, section).Length > 0;
        }

        /// <summary>
        /// Класс предназначен для методов, которые являются безопасными для всех, кто их вызывает.
        /// </summary>
        [SuppressUnmanagedCodeSecurityAttribute]
        internal static class SafeNativeMethods
        {
            [DllImport("kernel32", CharSet = CharSet.Ansi, BestFitMapping = false)]
            public static extern int WritePrivateProfileString(string section, string key, string value, string filePath);

            [DllImport("kernel32", CharSet = CharSet.Ansi, BestFitMapping = false)]
            public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        }
    }

    /// <summary>
    /// Для поддержки счетчика последовательности протокола ICD.
    /// </summary>
    public class AutoCounter
    {
        /// <summary>
        /// Счетчик (автоинкременируемый)
        /// </summary>
        private short _counter = 0;

        /// <summary>
        /// Получает [значение счетчика].
        /// Примечание:
        /// После считывания автоматически увеличивается на единицу.
        /// </summary>
        /// <value>
        /// [значение счетчика].
        /// </value>
        public short Counter
        {
            get
            {
                return _counter++;
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="AutoCounter"/> to <see cref="System.Int16"/>.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator short(AutoCounter obj)
        {
            return obj.Counter;
        }
    }

    /// <summary>
    /// Генерация случайного набора данных.
    /// </summary>
    public class RandomBufferGenerator
    {
        /// <summary>
        /// Экземпляр класса формирования псевдо-случайных чисел.
        /// </summary>
        private readonly Random _random = new Random();
       
        /// <summary>
        /// "зерно" для формирования случайных байт.
        /// </summary>
        private readonly byte[] _seedBuffer;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RandomBufferGenerator" />.
        /// </summary>
        /// <param name="bufferSize">Размер буфера для генерации случайных байт.</param>
        public RandomBufferGenerator(int bufferSize)
        {
            _seedBuffer = new byte[bufferSize];

            _random.NextBytes(_seedBuffer);
        }

        /// <summary>
        /// Формирует массив случайных байт.
        /// </summary>
        /// <param name="size">Размер массива случайных байт.</param>
        /// <returns>Массив случайных байт.</returns>
        public byte[] GenerateBufferFromSeed(int size)
        {
            int randomWindow = _random.Next(0, size);

            byte[] buffer = new byte[size];

            Buffer.BlockCopy(_seedBuffer, randomWindow, buffer, 0, size - randomWindow);
            Buffer.BlockCopy(_seedBuffer, 0, buffer, size - randomWindow, randomWindow);

            return buffer;
        }
    }

    /// <summary>
    /// Класс обработки команд для wpf в сценарии mvvm.
    /// </summary>
    public class RelayCommand : System.Windows.Input.ICommand
    {
        /// <summary>
        /// Делегат для выполнения команды.
        /// </summary>
        private readonly Action<object> _execute;

        /// <summary>
        /// Делегат для проверки выполняемости команды (т.е. можно ли выполнить команду).
        /// </summary>
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RelayCommand" />.
        /// </summary>
        /// <param name="execute">Метод для выполнения команды.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RelayCommand" />.
        /// </summary>
        /// <param name="execute">Метод для выполнения команды.</param>
        /// <param name="canExecute">Метод для проверки выполняемости команды.</param>
        /// <exception cref="System.ArgumentNullException">Отсутствует метод для команды.</exception>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Determines whether this instance can execute the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Можно ли выполнить команду сейчас.</returns>
        public bool CanExecute(object parameters)
        {
            return _canExecute == null ? true : _canExecute(parameters);
        }

        /// <summary>
        /// Executes the specified parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public void Execute(object parameters)
        {
            _execute(parameters);
        }
    }
}