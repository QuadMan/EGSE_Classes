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
    /// Для преобразования сообщений spacewire к сообщениям верхнего уровня.
    /// </summary>
    public static class MsgWorker
    {
        /// <summary>
        /// Преобразует "сырые" данные массива к типу КБВ-кадра.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Экземпляр КБВ-кадра.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        public static SpacewireKbvMsgEventArgs AsKbv(this Array obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Преобразует сообщение ICD-протокола к сообщению КБВ-кадра.
        /// </summary>
        /// <param name="obj">The <see cref="SpacewireIcdMsgEventArgs"/> instance containing the event data.</param>
        /// <returns>Экземпляр КБВ-кадра.</returns>
        public static SpacewireKbvMsgEventArgs AsKbv(this SpacewireIcdMsgEventArgs obj)
        {
            SpacewireKbvMsgEventArgs newObj = new SpacewireKbvMsgEventArgs(new byte[] { }, obj.Time1, obj.Time2, obj.Error);
            newObj.FieldPNormal = obj.Data[0];
            newObj.FieldPExtended = obj.Data[1];
            newObj.Kbv = obj.ConvertToInt(obj.Data.Skip(2).ToArray());
            return newObj;
        }

        /// <summary>
        /// Формирует тип КБВ-кадра, с данными представленными в массиве байт.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Экземпляр КБВ-кадра.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        public static SpacewireKbvMsgEventArgs ToKbv(this Array obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Преобразует "сырые" данные массива к типу ТМ-кадра.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Экземпляр ТМ-кадра.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        public static SpacewireTmMsgEventArgs AsTm(this Array obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Преобразует сообщение ICD-протокола к сообщению ТМ-кадра.
        /// </summary>
        /// <param name="obj">The <see cref="SpacewireIcdMsgEventArgs"/> instance containing the event data.</param>
        /// <returns>Экземпляр ТМ-кадра.</returns>
        public static SpacewireTmMsgEventArgs AsTm(this SpacewireIcdMsgEventArgs obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Формирует тип ТМ-кадра, с данными представленными в массиве байт.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Экземпляр ТМ-кадра.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        public static SpacewireTmMsgEventArgs ToTm(this Array obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Преобразует "сырые" данные массива к типу ТК-кадра.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Экземпляр ТК-кадра.</returns>
        /// <exception cref="System.NotImplementedException">Нет реализации.</exception>
        public static SpacewireTkMsgEventArgs AsTk(this Array obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Преобразует сообщение ICD-протокола к сообщению ТК-кадра.
        /// </summary>
        /// <param name="obj">The <see cref="SpacewireIcdMsgEventArgs"/> instance containing the event data.</param>
        /// <returns>Экземпляр ТК-кадра.</returns>
        public static SpacewireTkMsgEventArgs AsTk(this SpacewireIcdMsgEventArgs obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Формирует тип ТК-кадра, с данными представленными в массиве байт.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="apid">APID устройтсва.</param>
        /// <param name="dict">Словарь контроля последовательности.</param>
        /// <returns>
        /// Экземпляр ТК-кадра.
        /// </returns>
        public static SpacewireTkMsgEventArgs ToTk(this Array obj, byte apid, Dictionary<byte, AutoCounter> dict)
        {
            byte[] buf = new byte[obj.Length];
            Array.Copy(obj, buf, buf.Length);
            return new SpacewireTkMsgEventArgs(apid, dict, buf);
        }

        /// <summary>
        /// Формирует SPTP-сообщение.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="to">Адрес устройства получателя.</param>
        /// <param name="from">Адрес устройства отправителя.</param>
        /// <returns>Экземпляр SPTP-кадра.</returns>
        public static SpacewireSptpMsgEventArgs ToSptp(this Array obj, byte to, byte from)
        {
            byte[] buf = new byte[obj.Length];
            Array.Copy(obj, buf, buf.Length);
            return new SpacewireSptpMsgEventArgs(buf, to, from);
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
                string sectionName = GetPropertyValue(win, Window.TitleProperty);
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
                string sectionName = GetPropertyValue(win, Window.TitleProperty);
                
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
        /// <returns>Результирующая строка, возвращает пустую строку, если data = null.</returns>
        public static string ByteArrayToHexStr(byte[] data, string delimeter = " ")
        {
            if (data == null)
            {
                return string.Empty;
            }

            string hex = BitConverter.ToString(data);
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

    /// <summary>
    /// Класс работы с временем в КИА - позволяет декодировать и преобразовывать в строку заданное время.
    /// Необходимо заполнить поле данных времени data (6 байт).
    /// </summary>
    public class EgseTime
    {
        /// <summary>
        /// Размер кадра "Время" устройства (в байтах).
        /// </summary>
        private const int DefaultTimeSize = 6;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EgseTime" />.
        /// </summary>
        public EgseTime()
        {
            Data = new byte[DefaultTimeSize];
            Day = 0;
            Hour = 0;
            Min = 0;
            Sec = 0;
            Msec = 0;
            Mcsec = 0;
        }

        /// <summary>
        /// Получает или задает данные времени (6 байт).
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Получает параметр: День.
        /// </summary>
        public uint Day { get; private set; }

        /// <summary>
        /// Получает параметр: Час.
        /// </summary>
        public uint Hour { get; private set; }

        /// <summary>
        /// Получает параметр: Минута.
        /// </summary>
        public uint Min { get; private set; }

        /// <summary>
        /// Получает параметр: Секунда.
        /// </summary>
        public uint Sec { get; private set; }

        /// <summary>
        /// Получает параметр: Миллисекунда.
        /// </summary>
        public uint Msec { get; private set; }

        /// <summary>
        /// Получает параметр: Микросекунда.
        /// </summary>
        public uint Mcsec { get; private set; }

        /// <summary>
        /// Декодируем время из буфера в поля
        /// </summary>
        public void Decode()
        {
            Day = ((uint)Data[0] << 3) | ((uint)Data[1] >> 5);
            Hour = (uint)Data[1] & 0x1F;
            Min = (uint)Data[2] >> 2;
            Sec = ((uint)(Data[2] & 3) << 4) | ((uint)Data[3] >> 4);
            Msec = ((uint)(Data[3] & 0xF) << 4) | ((uint)Data[4] >> 6);
            Mcsec = ((uint)(Data[4] & 3) << 8) | (uint)Data[5];
        }

        /// <summary>
        /// Кодируем текущее время в буфер.
        /// </summary>
        public void Encode()
        {
            DateTime now = DateTime.Now;
            Data[0] = 0;
            Data[1] = (byte)now.Hour;
            Data[2] = (byte)(now.Minute << 2);
            Data[2] |= (byte)(now.Second >> 4);
            Data[3] = (byte)(now.Second << 4);
            Data[3] |= (byte)(now.Millisecond >> 6);
            Data[4] = (byte)(now.Millisecond << 2);
            Data[5] = 0;
        }

        /// <summary>
        /// Преобразуем время в строку.
        /// </summary>
        /// <returns>Cтроку в виде DD:HH:MM:SS:MSS:MCS</returns>
        public new string ToString()
        {
            Decode();
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}.{3:D3}.{4:D3}", Hour, Min, Sec, Msec, Mcsec);
            return sb.ToString();
        }
    }

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