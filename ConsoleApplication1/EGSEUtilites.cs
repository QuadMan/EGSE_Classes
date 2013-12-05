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
 * StopWatch для высокоточного замера времени
**
** History:
**  0.1.0	(26.11.2013) -	Начальная версия
 *  0.2.0   (01.12.2013) - Ввел новые классы TMValue, EgseTime, ADC
 *                       - комментарии, рефакторинг
 *                       TODO: в bigBufferManager ввести признак переполнения буфера!
**
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using EGSE.Protocols;

namespace EGSE.Utilites
{
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
        /// Функция, которую нужно вызвать при изменении параметра
        /// </summary>
        public onFunctionDelegate function;
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
            function = fun;
            makeTest = mkTest;
        }

        /// <summary>
        /// Присваивание значения
        /// Если необходима проверка значения и определена функция проверки
        /// </summary>
        /// <param name="val">Новое значение</param>
        public void SetVal(int val) {
            if ((makeTest) && (function != null) && (value != val)) {
                function(val);
            }
            value = val;
        }
    }

    /// <summary>
    /// Класс работы с временем в КИА - позволяет декодировать и преобразовывать в строку заданное время
    /// Необходимо заполнить поле данных времени data (6 байт)
    /// </summary>
    class EgseTime
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
            sec = ((uint)data[2] << 4) | ((uint)data[3] >> 4);
            msec = ((uint)data[3] << 4) | ((uint)data[4] >> 6);
            mcsec =((uint)data[4] << 8) | (uint)data[5];
        }

        /// <summary>
        /// Преобразуем время в строку
        /// </summary>
        /// <returns>строку в виде DD:HH:MM:SS:MSS:MCS</returns>
        new public string ToString()
        {
            Decode();
            sb.Clear();
            sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}:{3:D3}:{4:D3}:", hour, min, sec, msec, mcsec);

            return sb.ToString();
        }
    }

    /// <summary>
    /// Класс, поддерживающий получение, преобразование данных АЦП
    /// Обладает возможностью высчитывания среднего значения
    /// Для вывода результата могут использоваться калибровочные данные
    /// </summary>
    class ADC
    {
        private const uint AVERAGE_LEVEL = 5;
        private float[][] adcVal;
        private Measure[] measures;

        public ADC(uint adcCount)
        {
            adcVal = new float[adcCount][];
            for (uint i = 0; i < adcCount; i++)
            {
                adcVal[i] = new float[AVERAGE_LEVEL];
                measures[i] = null;
            }
        }

        public void AddVal(uint idx, float val)
        {

        }

        public float Average(float[] vals) {
            return 0;
        }

        public float GetVal(uint idx)
        {
            if (measures[idx] != null)
            {
                float resultVal = 0;
                //try {
                measures[idx].getData(Average(adcVal[idx]), ref resultVal);
                //catch ()
                //}
                return resultVal;

            }
            else return Average(adcVal[idx]);
        }

        public void setCalibration(uint idx, Measure measure) {

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
    /// Класс рассчета значений по калибровочным данным
    /// </summary>
    class Measure
    {
        public Measure()
        {

        }

        ~Measure()
        {

        }

        public bool loadFromFile(string fName)
        {
            return true;
        }

        public bool addData(float x, float y)
        {
            return true;
        }

        public bool getData(float val, ref float res)
        {
            res = 0;
            return true;
        }
    }
}