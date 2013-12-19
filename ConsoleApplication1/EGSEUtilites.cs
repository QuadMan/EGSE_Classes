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
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Resources;

namespace EGSE.Utilites
{
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
        /// <param name="mark">Ключ</param>
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
                    _rm[i++] = new ResourceManager(s.Replace(@".resources", ""), assembly);
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
                        };
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
        /// <returns>Массив байт</returns>
        public static byte[] HexStrToByteArray(string HexStr)
        {
            string[] hexValuesSplit = HexStr.Split(' ');
            byte[] outBuf = new byte[hexValuesSplit.Length];
            int i = 0;
            foreach (String hex in hexValuesSplit)
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
        /// <returns>Результирующая строка</returns>
        public static string ByteArrayToHexStr(byte[] data, string delimeter = " ")
        {
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
                if (mbs > 1000) {
                    return mbs.ToString("0.0")+" мб/сек";
                }
                return kbs.ToString("0.0")+" кб/сек";
            }
            return speedInBytes.ToString("0.0")+" байт/сек";
        }

        /// <summary>
        /// Переводит размер файла в строку (с добавлением размерности)
        /// </summary>
        /// <param name="fileSizeInBytes">Размер файла в байтах</param>
        /// <returns>Строка размера файла</returns>
        public static string FileSizeToStr(UInt64 fileSizeInBytes)
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
                return fileSizeInBytes.ToString() + " байт";
        }

        /// <summary>
        /// Определяет нажатый элемент
        /// Обрабатываются:
        /// Нажатие кнопок
        /// Нажатие чекбоксов
        /// Выбор элемента комбобокса
        /// 
        /// Если элемент не найден, то возвращает null
        /// </summary>
        /// <param name="reaEv"></param>
        /// <returns>строку с описанием нажатого элемента (null, если ничего не найдено)</returns>
        /*
        public static string ElementClicked(MouseEventArgs reaEv)
        {

            string strRes = null;
            string[] strVerb = { "Нажата ", "Снято нажатие: ", "Выбран: " };


            if (reaEv.Source.GetType().Equals(typeof(Button)))
            {
                Button elemSource = reaEv.Source as Button;

                if (elemSource != null)
                    strRes += "Нажата кнопка: \"" + elemSource.Content + "\"";
            }
            else if (reaEv.Source.GetType().Equals(typeof(CheckBox)))
            {
                CheckBox elemSource = reaEv.Source as CheckBox;
                if (elemSource != null)
                {
                    if (!(bool)elemSource.IsChecked)
                        strRes += "Выбран ";//strVerb[0];
                    else
                        strRes += "Снят ";// strVerb[1];
                    strRes += "чекбокс: \"" + elemSource.Content + "\"";
                }
            }
            else if (reaEv.Source.GetType().Equals(typeof(ComboBoxItem)))
            {
                ComboBoxItem elemSource = reaEv.Source as ComboBoxItem;

                if (elemSource != null)
                    strRes += "Выбран комбобокс: \"" + elemSource.Content + "\"";
            }
            return strRes;
        }
        */
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
        public static T make<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                typeof(T));
            handle.Free();
            return stuff;
        }
    }

    /// <summary>
    /// Значение телеметрического параметра, изменения которого необходимо отслеживать (обычно для логгирования изменения состояния - контактные датчики, включени питания и т.д.)
    /// Пример использования:
    /// static void tFunc(int val)
    /// {
    ///     string s = "";
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
        public delegate void onFunctionDelegate(int val);

        /// <summary>
        /// Делагат на изменение параметра
        /// </summary>
        public onFunctionDelegate onNewState;

        /// <summary>
        /// Значение параметра
        /// </summary>
        public int value;

        /// <summary>
        /// Нужно ли проверять параметр на изменение значения
        /// </summary>
        public bool makeTest;

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public TMValue() {
            value = -1;
            makeTest = false;
        }

        /// <summary>
        /// Создаем параметр сразу 
        /// </summary>
        /// <param name="val">Значение</param>
        /// <param name="fun">Функция при изменении параметра, можно передать null</param>
        /// <param name="mkTest">Нужно ли сравнивать старое и новое значение параметра</param>
        public TMValue(int val, onFunctionDelegate fun, bool mkTest)
        {
            value = val;
            onNewState = fun;
            makeTest = mkTest;
        }

        /// <summary>
        /// Присваивание значения
        /// Если необходима проверка значения и определена функция проверки
        /// </summary>
        /// <param name="val">Новое значение</param>
        public void SetVal(int val) {
            bool _changed = true;
            if (makeTest) {
                _changed = value != val;
            }
            if (_changed)
            {
                onNewState(val);
            }

            value = val;
        }
    }

    /// <summary>
    /// Класс, позволяющий отслеживать изменения значений, которые влияют на отображение интерфейса
    /// (параметры имитаторов, значения включения полукомплектов и т.д.) и которые может устанавливать пользователь
    /// Пример работы:
    /// ControlValue HSIParameters;
    /// при получении данных от устройства, делаем HSIParameters.GetValue = value;
    /// при изменении пользователем интерфейса, делаем HSIParameters.SetValue = value;
    /// в таймере, вызываемом 2 раза в секунду, выполняем проверку
    /// if (HSIParameters.TimerTick() == ControlValue.ValueState.vsChanged) {
    /// нужно обновить экран, так как значение, пришедшее от прибора не совпадает с установленным
    /// }
    /// </summary>
    public class ControlValue
    {
        private const int UPDATE_TIMEOUT_TICKS = 3;
        public delegate void setFunctionDelegate(UInt32 value);

        struct CVProperty
        {
            public UIElement cb;
            public UInt16 bitIdx;
            public UInt16 bitLen;
            public setFunctionDelegate func;

            public CVProperty(UIElement _cb, UInt16 _bitIdx, UInt16 _bitLen, setFunctionDelegate _func)
            {
                cb = _cb;
                bitIdx = _bitIdx;
                bitLen = _bitLen;
                func = _func;
            }
        };

        List<CVProperty> cvpl = new List<CVProperty>();

        public enum ValueState { vsUnchanged, vsChanged, vsCounting };
        // старое значение, которое получили
        private int _oldGetValue;
        // значение, которое получаем из USB
        private int _getValue;
        // значение, которое уставливается из интерфейса
        private int _setValue;
        // значение счетчика времени до проверки совпадения GetValue и SetValue
        private int _timerCnt;
        //
        private int _defaultValue;
        //
        private bool _updatingGetState;

        public ControlValue(int defaultValue = 0)
        {
            _oldGetValue = -1;
            _getValue = 0;
            _setValue = 0;
            _timerCnt = 0;
            _updatingGetState = false;
            _defaultValue = defaultValue;
        }

        public bool AddProperty(UIElement _cb, UInt16 _bitIdx, UInt16 _bitLen, setFunctionDelegate _func)
        {
            if (_cb is CheckBox)
            {
                (_cb as CheckBox).Click += _cb_Click;
            }
            else if (_cb is ComboBox)
            {
                (_cb as ComboBox).SelectionChanged += ControlValue_SelectionChanged;
            }
            else return false;

            cvpl.Add(new CVProperty(_cb, _bitIdx, _bitLen,_func));
            return true;
        }

        void ControlValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingGetState) return;
            foreach (CVProperty cv in cvpl)
            {
                if ((cv.cb is ComboBox) && (cv.cb == sender))
                {
                    Int32 mask = 0;
                    for (UInt16 i = 0; i < cv.bitLen; i++)
                    {
                        mask |= (1 << (cv.bitIdx + i));
                    }
                    _setValue &= ~mask;
                    _setValue |= (cv.cb as ComboBox).SelectedIndex << cv.bitIdx;
                    TimerSet(UPDATE_TIMEOUT_TICKS);
                    cv.func((uint)_setValue);
                }
            }            
        }

        void _cb_Click(object sender, RoutedEventArgs e)
        {
            if (_updatingGetState) return;

            foreach (CVProperty cv in cvpl)
            {
                if ((cv.cb is CheckBox) && (cv.cb == sender))
                {
                    if ((bool)(cv.cb as CheckBox).IsChecked)
                    {
                        _setValue |= (1 << cv.bitIdx);
                    }
                    else
                    {
                        _setValue &= ~(1 << cv.bitIdx);
                    }
                    _setValue |= _defaultValue;
                    TimerSet(UPDATE_TIMEOUT_TICKS);            // значение установили, ждем 2 секунды до проверки Get и SetValue
                    cv.func((uint)_setValue);
                }
            }
        }

        /// <summary>
        /// Метод для принудительного вызова через определнное время проверки
        /// установленного и полученного значений
        /// </summary>
        public void RefreshGetValue() {
            _timerCnt = UPDATE_TIMEOUT_TICKS;
        }

        /// <summary>
        /// Значение, полученное из USB
        /// </summary>
        public int GetValue
        {
            get
            {
                return _getValue;
            }
            set
            {
                _oldGetValue = _getValue;
                _getValue = value;
            }
        }

        /// <summary>
        /// Значение, установленное из интерфейса
        /// </summary>
        public int SetValue
        {
            get { return _setValue; }
            set
            {
                _setValue = value;

                TimerSet(UPDATE_TIMEOUT_TICKS);            // значение установили, ждем 2 секунды до проверки Get и SetValue
            }
        }

        /// <summary>
        /// Устаналиваем, сколько "тиков" ждать
        /// </summary>
        /// <param name="timerVal"></param>
        private void TimerSet(int timerVal)
        {
            _timerCnt = timerVal;
        }

        /// <summary>
        /// Один тик таймера (вызывается из внешнего прерывания и может быть любым, хоть 1 секунда, хоть 500 мс)
        /// </summary>
        /// <returns>Функция возвращает результат проверки Set и Get значений, если время истекло, или vsCounting, если счет продолжается</returns>
        public ValueState TimerTick()
        {
            if ((_timerCnt > 0) && (--_timerCnt == 0))          // пришло время для проверки Get и Set Value
            {
                //if (Changed()) { return ValueState.vsChanged; }
                //else { return ValueState.vsUnchanged; }

                if (Changed()) UpdateGetProperties();
            }
            return ValueState.vsCounting;
        }

        private void UpdateGetProperties()
        {
            _updatingGetState = true;
            foreach (CVProperty cv in cvpl)
            {
                if (cv.cb is CheckBox)
                {
                    (cv.cb as CheckBox).IsChecked = ((_getValue & (1 << cv.bitIdx)) > 0);
                }
                if (cv.cb is ComboBox)
                {
                    Int32 mask = 0;
                    for (UInt16 i = 0; i < cv.bitLen; i++)
                    {
                        mask |= (1 << (cv.bitIdx + i));
                    }
                    (cv.cb as ComboBox).SelectedIndex = (_getValue & mask) >> cv.bitIdx;
                }
            }
            _setValue = _getValue;
            _updatingGetState = false;
        }

        /// <summary>
        /// Проверяем значения
        /// </summary>
        /// <returns></returns>
        private bool Changed()
        {
            return (_oldGetValue != _getValue) || (_setValue != _getValue);
        }
    }

    /// <summary>
    /// Класс работы с временем в КИА - позволяет декодировать и преобразовывать в строку заданное время
    /// Необходимо заполнить поле данных времени data (6 байт)
    /// </summary>
    public class EgseTime
    {
        /// <summary>
        /// Данные времени (6 байт)
        /// </summary>
        public byte[] data;

        public uint day;
        public uint hour;
        public uint min;
        public uint sec;
        public uint msec;
        public uint mcsec;
        private StringBuilder sb;

        public EgseTime()
        {
            data = new byte[6];
            day = 0;
            hour = 0;
            min = 0;
            sec = 0;
            msec = 0;
            mcsec = 0;
            sb = new StringBuilder();
        }

        public void Decode() 
        {
            day = ((uint)data[0] << 3) | ((uint)data[1] >> 5);
            hour = ((uint)data[1] & 0x1F);
            min = ((uint)data[2] >> 2);
            sec = ((uint)(data[2] & 3) << 4) | ((uint)data[3] >> 4);
            msec = ((uint)(data[3] & 0xF) << 4) | ((uint)data[4] >> 6);
            mcsec =((uint)(data[4] & 3) << 8) | (uint)data[5];
        }

        public void Encode()
        {
            DateTime now = DateTime.Now;
            data[0] = 0;
            data[1] = (byte)now.Hour;
            data[2] = (byte)(now.Minute << 2);
            data[2] |= (byte)(now.Second >> 4);
            data[3] = (byte)(now.Second << 4);
            data[3] |= (byte)(now.Millisecond >> 6);

            data[4] = (byte)(now.Millisecond << 2);
            data[5] = 0;
        }

        /// <summary>
        /// Преобразуем время в строку
        /// </summary>
        /// <returns>строку в виде DD:HH:MM:SS:MSS:MCS</returns>
        new public string ToString()
        {
            Decode();
            sb.Clear();
            sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}.{3:D3}.{4:D3}", hour, min, sec, msec, mcsec);

            return sb.ToString();
        }
    }

    /// <summary>
    /// Класс менеджера для большого кольцевого буфера
    /// Представляет собой двумерный массив. Первый индекс которого является указателем на большой массив 
    /// максимальным размером 70 КБ. При чтении и записи изменяются указатели первого индекса двумерного массива.
    /// </summary>
    class BigBufferManager
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
        public byte[][] AData;
        /// <summary>
        /// Здесь хранятся длины всех массивов, так как длина второго массива задана константой
        /// </summary>
        private int[] ALen;
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
        private Object thisLock = new Object();
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
            ALen = new int[_bufSize];
            for (uint i = 0; i < _bufSize; i++)
            {
                AData[i] = new byte[FTDI_BUF_SIZE];
                ALen[i] = 0;
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
        public void moveNextRead()
        {
            lock (this)
            {
                _curRPos = (_curRPos + 1) % _bufSize;
                _count--;
                _bytesInBuffer -= ALen[_lastRPos];
//#if DEBUG_TEXT
                System.Console.WriteLine("readBuf, count = {0}, bytesAvailable = {1}, RPos = {2}", _count, _bytesInBuffer,_curRPos);
//#endif
            }
        }

        /// <summary>
        /// Перемещает указатель записи
        /// Обеспечивает защиту от одновременного изменения _count и _bytesInBuffer
        /// Изменяет _bytesInBuffer и записывает длину буфера в ALen
        /// </summary>
        /// <param name="bufSize">Сколько было записано в текущий буфер</param>
        public void moveNextWrite(int bufSize)
        {
            lock (this)
            {
                _curWPos = (_curWPos + 1) % _bufSize;
                _count++;
                ALen[_lastWPos] = bufSize;
                _bytesInBuffer += bufSize;
//#if DEBUG_TEXT
                System.Console.WriteLine("writeBuf, count = {0}, bytesAvailable = {1}, WPos = {2}", _count, _bytesInBuffer, _curWPos);
//#endif
            }
        }

        /// <summary>
        /// Возвращает количество байт в буфере
        /// </summary>
        public int bytesAvailable
        {
            get
            {
                return _bytesInBuffer;
            }
        }

        /// <summary>
        /// Возвращает размер последнего буфера для чтения
        /// </summary>
        public int readBufSize
        {
            get
            {
                return ALen[_lastRPos];
            }
        }

        /// <summary>
        /// Возвращает текущий буфер для чтения
        /// Если читать нечего, возвращает null
        /// </summary>
        public byte[] readBuf
        {
            get
            {
                if (_count > 0)
                {
                    _lastRPos = _curRPos;

                    return AData[_lastRPos];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Возвращает текущий буфер для записи
        /// Если писать некуда, возвращает null
        /// </summary>
        public byte[] writeBuf
        {
            get
            {
                if (_count < _bufSize)
                {
                    _lastWPos = _curWPos;

                    return AData[_lastWPos];
                }
                else
                {
                    return null;
                }
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
        static extern long WritePrivateProfileString(string section, string key, string value, string filePath);
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
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
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(section ?? _exe, key, "", RetVal, 255, _path);
            return RetVal.ToString();
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
    /// Класс позволяет сохранять в ini-файл параметры экземплеров окон
    /// Сохраняет: позицию, размеры, состояние(развернуто/свернуто) окна, видимость
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
        public static bool Save(string param, string value, string section="MAIN")
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
                else return null;
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
                    win.Left = _rect.Left;
                    win.Top = _rect.Top;
                    win.Height = _rect.Size.Height;
                    win.Width = _rect.Size.Width;
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