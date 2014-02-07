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
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Resources;

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
        /// Строка шестнадцатеричных чисел в массив байт
        /// </summary>
        /// <param name="HexStr">Строка HEX чисел, разделенных пробелами</param>
        /// <returns>Массив байт или null, если HexStr пустая </returns>
        public static byte[] HexStrToByteArray(string HexStr)
        {
            if (HexStr == string.Empty)
            {
                return null;
            }

            string[] hexValuesSplit = HexStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] outBuf = new byte[hexValuesSplit.Length];
            int i = 0;
            foreach (string hex in hexValuesSplit)
            {
                outBuf[i++] = (byte)Convert.ToInt32(hex, 16);
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
        /// Байты в строке будут разделяться строкой, заданной в delimeter
        /// </summary>
        /// <param name="data">Массив байт</param>
        /// <param name="delimeter">Разделитель между байтами в строке</param>
        /// <returns>Результирующая строка, возвращает пустую строку, если data = null</returns>
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
    public class EGSETime
    {
        private const int DEFAULT_TIME_SIZE_BYTES = 6;

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

        // строка со временем
        private StringBuilder sb;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EGSETime" />.
        /// </summary>
        public EGSETime()
        {
            Data = new byte[DEFAULT_TIME_SIZE_BYTES];
            Day = 0;
            Hour = 0;
            Min = 0;
            Sec = 0;
            Msec = 0;
            Mcsec = 0;
            sb = new StringBuilder();
        }

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
        /// Преобразуем время в строку
        /// </summary>
        /// <returns>строку в виде DD:HH:MM:SS:MSS:MCS</returns>
        new public string ToString()
        {
            Decode();
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
        /// Путь к ini-файлу
        /// </summary>
        private string _path;

        /// <summary>
        /// Имя exe-файла (Название группы параметра по-умолчанию) 
        /// </summary>
        private string _exe = Assembly.GetExecutingAssembly().GetName().Name;
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="IniFile" />.
        /// Конструктор конкретного ini-файла.
        /// </summary>
        /// <param name="iniPath">Полный путь к ini-файлу</param>
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
            GetPrivateProfileString(section ?? _exe, key, string.Empty, retVal, 255, _path);
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
            WritePrivateProfileString(section ?? _exe, key, value, _path);
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
    }

    /// <summary>
    /// Класс сохранения настроек приложения в Ini файл
    /// Сохраняет и отдельные свойства и настройки окон: позицию, размеры, состояние(развернуто/свернуто) окна, видимость
    /// </summary>
    public static class AppSettings
    {
        private const int MAX_ITEMS_COUNT = 100;

        /// <summary>
        /// Записываем параметр param в секцию section, значением value.
        /// </summary>
        /// <param name="param">Параметр приложения</param>
        /// <param name="value">Значение параметра</param>
        /// <param name="section">секция, по-умолчанию, MAIN</param>
        /// <returns>True - если все хорошо</returns>
        public static bool Save(string param, string value, string section = "MAIN")
        {
            try
            {
                IniFile _ini = new IniFile();
                _ini.Write(param, value, section);
                return true;
            }
            catch (Exception e)
            {
                throw e;
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
            catch (Exception e)
            {
                throw e;
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
            if ((strList == null) || (strList.Count == 0)) {
                return false;
            }

            try
            {
                IniFile _ini = new IniFile();
                int i = 0;
                foreach (string s in strList)
                {
                    _ini.Write("Item" + i.ToString(), s, section);
                    if (i++ == MAX_ITEMS_COUNT)
                    {
                        break;
                    }
                }
                
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Загружаем элементы из секции в список.
        /// Отбираем элементы ItemN, где N должен быть от 0 до MAX_ITEMS_COUNT-1.
        /// </summary>
        /// <param name="strList">Список, в который загружаем элементы</param>
        /// <param name="section">Секция, из которой загружаем элементы списка</param>
        /// <returns>True - если выполнено успешно</returns>
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
                while (i < MAX_ITEMS_COUNT)
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
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        /// <summary>
        /// Сохраняем параметры окна
        /// </summary>
        /// <param name="win">Экземпляр окна</param>
        /// <returns>true, если функция выполнена успешно</returns>
        public static bool SaveWindow(Window win)
        {
            try
            {
                IniFile _ini = new IniFile();
                _ini.Write("Visibility", Convert.ToString(win.Visibility), win.Title);
                _ini.Write("WindowState", Convert.ToString(win.WindowState), win.Title);
                _ini.Write("Bounds", Convert.ToString(new Rect(new System.Windows.Point(win.Left, win.Top), win.RenderSize), new System.Globalization.CultureInfo("en-US")), win.Title);
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Загружаем сохраненные параметры окна
        /// </summary>
        /// <param name="win">Экземпляр окна</param>
        /// <returns>true, если функция выполнена успешно</returns>
        public static bool LoadWindow(Window win)
        {
            try
            {
                IniFile _ini = new IniFile();
                if (_ini.IsKeyExists("Bounds", win.Title))
                {
                    System.ComponentModel.TypeConverter _conv =
                        System.ComponentModel.TypeDescriptor.GetConverter(typeof(Rect));
                    Rect _rect = (Rect)_conv.ConvertFromString(_ini.Read("Bounds", win.Title));

                    // сделал проверки на NaN и 0, так как для окон, которые не открывались значения (Left,Top,RenderSize) не рассчитываются системой, нужно придумать другой способ сохранения
                    if (_rect.Left != double.NaN)
                    {
                        win.Left = _rect.Left;
                    }

                    if (_rect.Top != double.NaN)
                    {
                        win.Top = _rect.Top;
                    }

                    if (_rect.Size.Height != 0)
                    {
                        win.Height = _rect.Size.Height;
                    }

                    if (_rect.Size.Width != 0)
                    {
                        win.Width = _rect.Size.Width;
                    }
                }
                else
                {
                    _ini.Write("Bounds", Convert.ToString(new Rect(new System.Windows.Point(win.Left, win.Top), win.RenderSize), new System.Globalization.CultureInfo("en-US")), win.Title);
                }

                if (_ini.IsKeyExists("WindowState", win.Title))
                {
                    System.ComponentModel.TypeConverter _conv2 =
                        System.ComponentModel.TypeDescriptor.GetConverter(typeof(WindowState));
                    win.WindowState = (WindowState)_conv2.ConvertFromString(_ini.Read("WindowState", win.Title));
                }
                else
                {
                    _ini.Write("WindowState", Convert.ToString(win.WindowState), win.Title);
                }

                if (_ini.IsKeyExists("Visibility", win.Title))
                {
                    System.ComponentModel.TypeConverter _conv3 =
                        System.ComponentModel.TypeDescriptor.GetConverter(typeof(Visibility));
                    win.Visibility = (Visibility)_conv3.ConvertFromString(_ini.Read("Visibility", win.Title));
                }
                else
                {
                    _ini.Write("Visibility", Convert.ToString(win.Visibility), win.Title);
                }

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}