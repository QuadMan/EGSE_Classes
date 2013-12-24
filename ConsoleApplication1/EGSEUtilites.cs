/*** EDGE_Utilites.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль дополнительных утилит для КИА
**
** Author: Семенов Александр, Мурзин Святослав
** Project: КИА
** Module: EDGE UTILITES
** Requires: 
** Comments:
 * ==================================================
 * StopWatch для высокоточного замера времени
** ==================================================
 *Работа с битовыми полями в структуре:
 *[StructLayout(LayoutKind.Sequential)]
  public struct Rgb16 {
        private readonly UInt16 raw;
        public byte R{get{return (byte)((raw>>0)&0x1F);}}
        public byte G{get{return (byte)((raw>>5)&0x3F);}}
        public byte B{get{return (byte)((raw>>11)&0x1F);}}

        public Rgb16(byte r, byte g, byte b)
        {
          Contract.Requires(r<0x20);
          Contract.Requires(g<0x40);
          Contract.Requires(b<0x20);
          raw=r|g<<5|b<<11;
        }
    }
** History:
**  0.1.0	(26.11.2013) -	Начальная версия
 *  0.2.0   (01.12.2013) - Ввел новые классы TMValue, EgseTime, ADC
 *                       - комментарии, рефакторинг
 *                       TODO: в bigBufferManager ввести признак переполнения буфера!
 *  0.2.1   (06.12.2013) - в класс EgseTime добавлена возможность кодировки времени
 *                        коррекция декодера времени
 *  0.2.2   (10.12.2013) - добавил функцию конвертации HEX-строки в массив байт
**
 *
 * TODO: в классе BigBuff попробовать уйти от lock(this) в сторону InterlockedIncrement
*/

namespace EGSE.Utilites
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;

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

            string[] hexValuesSplit = HexStr.Split(' ');
            byte[] outBuf = new byte[hexValuesSplit.Length];
            int i = 0;
            foreach (string hex in hexValuesSplit)
            {
                outBuf[i++] = (byte)Convert.ToInt32(hex, 16);
            }

            return outBuf;
        }

        /// <summary>
        /// Строка шестнадцатеричных чисел в массив байт
        /// </summary>
        /// <param name="HexStr">Массив HEX чисел (в виде строк)</param>
        /// <returns>Массив байт или null, если массив hexValues пуст</returns>
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
    /// Статический класс позволяет накладывать байтовый поток на структуру
    /// Пример: объявляем структуру
    ///         [StructLayout(LayoutKind.Sequential,Pack = 1)]
    ///         struct testS
    ///         {
    ///                 byte header;
    ///                 int size;
    ///                 byte flag;
    ///         }
    ///         и вызываем функцию 
    ///         testS ts = ByteArrayToStructure.make<testS>(byteBuf);
    /// </summary>
    public static class ByteArrayToStructure
    {
        /// <summary>
        /// Преобразуем массив байт в структуру
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T Make<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return stuff;
        }
    }

    /// <summary>
    /// Значение телеметрического параметра, изменения которого необходимо отслеживать (обычно для логгирования изменения состояния - контактные датчики, включени питания и т.д.)
    /// Пример использования:
    /// static void tFunc(int val)
    /// {
    ///     string s = String.Empty;
    ///     if ((val and 1) == 1) s += "ПК1 ВКЛ ";
    ///     if ((val and 2) == 2) s += "ПК2 ВКЛ ";
    ///     System.Console.WriteLine(s);
    /// }
    /// TMValue val1 = new TMValue(0, tFunc, true);
    /// val1.SetVal(3);
    /// </summary>
    public class TMValue
    {
        /// <summary>
        /// Описание делегата функции, которую нужно вызвать при изменении параметра
        /// </summary>
        /// <param name="val"></param>
        public delegate void ChangeValueEventHandler(int val);

        /// <summary>
        /// Делагат на изменение параметра
        /// </summary>
        public ChangeValueEventHandler ChangeValueEvent;

        /// <summary>
        /// Значение параметра
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Нужно ли проверять параметр на изменение значения
        /// </summary>
        public bool MakeTest { get; set; }

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public TMValue()
        {
            Value = -1;
            MakeTest = false;
        }

        /// <summary>
        /// Создаем параметр сразу 
        /// </summary>
        /// <param name="val">Значение</param>
        /// <param name="fun">Функция при изменении параметра, можно передать null</param>
        /// <param name="makeTest">Нужно ли сравнивать старое и новое значение параметра</param>
        public TMValue(int val, ChangeValueEventHandler fun, bool makeTest)
        {
            Value = val;
            ChangeValueEvent = fun;
            MakeTest = makeTest;
        }

        /// <summary>
        /// Присваивание значения
        /// Если необходима проверка значения и определена функция проверки
        /// </summary>
        /// <param name="val">Новое значение</param>
        public void SetVal(int val)
        {
            bool _changed = true;
            if (MakeTest)
            {
                _changed = Value != val;
            }

            if (_changed)
            {
                ChangeValueEvent(val);
            }

            Value = val;
        }
    }

    /// <summary>
    /// Класс работы с временем в КИА - позволяет декодировать и преобразовывать в строку заданное время
    /// Необходимо заполнить поле данных времени data (6 байт)
    /// </summary>
    public class EgseTime
    {
        private const int DEFAULT_TIME_SIZE_BYTES = 6;

        /// <summary>
        /// Данные времени (6 байт)
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// День
        /// </summary>
        public uint Day { get; private set; }

        /// <summary>
        /// Час
        /// </summary>
        public uint Hour { get; private set; }

        /// <summary>
        /// Минута
        /// </summary>
        public uint Min { get; private set; }

        /// <summary>
        /// Секунда
        /// </summary>
        public uint Sec { get; private set; }

        /// <summary>
        /// Миллисекунда
        /// </summary>
        public uint Msec { get; private set; }

        /// <summary>
        /// Микросекунда
        /// </summary>
        public uint Mcsec { get; private set; }

        // строка со временем
        private StringBuilder sb;

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public EgseTime()
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
        /// Кодируем ТЕКУШЕЕ время в буфер
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
    /// Класс менеджера для большого кольцевого буфера
    /// Представляет собой двумерный массив. Первый индекс которого является указателем на большой массив 
    /// максимальным размером 70 КБ. При чтении и записи изменяются указатели первого индекса двумерного массива.
    /// </summary>
    public class BigBufferManager
    {
        /// <summary>
        /// Размер буфера по-умолчанию
        /// </summary>
        private const uint DEFAULT_BUF_SIZE = 100;

        /// <summary>
        /// Размер единичных массивов (ограничен драйвером FTDI, который не передает больше 65 КБ за раз)
        /// </summary>
        private const uint FTDI_BUF_SIZE = 70000;

        /// <summary>
        /// Представление большого кольцевого буфера
        /// AData[positionIdx][dataIdx]
        /// </summary>
        public byte[][] AData { get; set; }

        /// <summary>
        /// Здесь хранятся длины всех массивов, так как длина второго массива задана константой
        /// </summary>
        private int[] _aLen;

        /// <summary>
        /// Текущая позиция чтения
        /// </summary>
        private uint _curRPos;

        /// <summary>
        /// Текущая позиция записи
        /// </summary>
        private uint _curWPos;

        /// <summary>
        /// Количество элементов в кольцевом буфере
        /// </summary>
        private int _count;

        /// <summary>
        /// Размер кольцевого буфера (в количестве массивов по 70 КБ)
        /// </summary>
        private uint _bufSize;

        /// <summary>
        /// Последняя позиция чтения
        /// </summary>
        private uint _lastRPos;

        /// <summary>
        /// Последняя позиция записи
        /// </summary>
        private uint _lastWPos;

        /// <summary>
        /// Объект для защиты функций moveNextRead и moveNextWrite
        /// </summary>
        private object thisLock = new object();

        /// <summary>
        /// Размер буфера в байтах, счиается при вызове функций moveNextRead и moveNextWrite
        /// </summary>
        private int _bytesInBuffer;

        /// <summary>
        /// Конструктор большого буфера
        /// </summary>
        /// <param name="bufSize">Размер буфера</param>
        public BigBufferManager(uint bufSize = DEFAULT_BUF_SIZE)
        {
            _bufSize = bufSize;
            AData = new byte[_bufSize][];
            _aLen = new int[_bufSize];
            for (uint i = 0; i < _bufSize; i++)
            {
                AData[i] = new byte[FTDI_BUF_SIZE];
                _aLen[i] = 0;
            }

            _lastRPos = 0;
            _lastWPos = 0;
            _curWPos = 0;
            _curRPos = 0;
            _count = 0;
            _bytesInBuffer = 0;
        }

        /// <summary>
        /// Перемещает указатель чтения
        /// Обеспечивает защиту от одновременного изменения _count и _bytesInBuffer
        /// Изменяет _bytesInBuffer
        /// </summary>
        public void MoveNextRead()
        {
            lock (this)
            {
                _curRPos = (_curRPos + 1) % _bufSize;
                _count--;
                _bytesInBuffer -= _aLen[_lastRPos];
#if DEBUG_TEXT
                System.Console.WriteLine("readBuf, count = {0}, bytesAvailable = {1}, RPos = {2}", _count, _bytesInBuffer,_curRPos);
#endif
            }
        }

        /// <summary>
        /// Перемещает указатель записи
        /// Обеспечивает защиту от одновременного изменения _count и _bytesInBuffer
        /// Изменяет _bytesInBuffer и записывает длину буфера в ALen
        /// </summary>
        /// <param name="bufSize">Сколько было записано в текущий буфер</param>
        public void MoveNextWrite(int bufSize)
        {
            lock (this)
            {
                _curWPos = (_curWPos + 1) % _bufSize;
                _count++;
                _aLen[_lastWPos] = bufSize;
                _bytesInBuffer += bufSize;
#if DEBUG_TEXT
                System.Console.WriteLine("writeBuf, count = {0}, bytesAvailable = {1}, WPos = {2}", _count, _bytesInBuffer, _curWPos);
#endif
            }
        }

        /// <summary>
        /// Возвращает количество байт в буфере
        /// </summary>
        public int BytesAvailable
        {
            get
            {
                return _bytesInBuffer;
            }
        }

        /// <summary>
        /// Возвращает размер последнего буфера для чтения
        /// </summary>
        public int ReadBufSize
        {
            get
            {
                return _aLen[_lastRPos];
            }
        }

        /// <summary>
        /// Возвращает текущий буфер для чтения
        /// Если читать нечего, возвращает null
        /// </summary>
        public byte[] ReadBuf
        {
            get
            {
                if (_count == 0)
                {
                    return null;
                }

                _lastRPos = _curRPos;
                return AData[_lastRPos];
            }
        }

        /// <summary>
        /// Возвращает текущий буфер для записи
        /// Если писать некуда, возвращает null
        /// </summary>
        public byte[] WriteBuf
        {
            get
            {
                if (_count >= _bufSize)
                {
                    return null;
                }

                _lastWPos = _curWPos;
                return AData[_lastWPos];
            }
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
        /// <summary>
        /// Записываем параметр param в секцию section, значением value
        /// </summary>
        /// <param name="param">Параметр приложения</param>
        /// <param name="value">Значение параметра</param>
        /// <param name="section">секция, по-умолчанию, MAIN</param>
        /// <returns></returns>
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
        /// <param name="section">Секция</param>
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
                _ini.Write("Bounds", Convert.ToString(new Rect(new System.Windows.Point(win.Left, win.Top), win.RenderSize)).Replace(";", ","), win.Title);
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
                    _ini.Write("Bounds", Convert.ToString(new Rect(new System.Windows.Point(win.Left, win.Top), win.RenderSize)).Replace(";", ","), win.Title);
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