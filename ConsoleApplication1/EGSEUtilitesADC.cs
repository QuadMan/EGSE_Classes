//-----------------------------------------------------------------------
// <copyright file="EGSEUtilitesADC.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Мурзин Святослав</author>
//-----------------------------------------------------------------------

namespace EGSE.Utilites.ADC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Структура калибровочных значений
    /// </summary>
    public struct CValue
    {
        /// <summary>
        /// Значение по координате X.
        /// </summary>
        private float _x;

        /// <summary>
        /// Значение по координате Y.
        /// </summary>
        private float _y;

        /// <summary>
        /// Инициализирует новый экземпляр структуры <see cref="CValue" />.
        /// </summary>
        /// <param name="x">Значение по координате X.</param>
        /// <param name="y">Значение по координате Y.</param>
        public CValue(float x, float y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Получает или задает значение по координате X.
        /// </summary>
        public float XVal 
        {
            get
            {
                return _x;
            }

            set
            {
                _x = value;
            }
        }

        /// <summary>
        /// Получает или задает значение по координате Y.
        /// </summary>
        public float YVal
        {
            get
            {
                return _y;
            }

            set
            {
                _y = value;
            }
        }
    }

    /// <summary>
    /// Специальное исключение для классов.
    /// </summary>
    public class ADCException : ApplicationException
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ADCException" />.
        /// </summary>
        public ADCException() 
        { 
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ADCException" />.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public ADCException(string message) 
            : base(message) 
        { 
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ADCException" />.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">Внутреннее исключение.</param>
        public ADCException(string message, Exception ex) 
            : base(message) 
        { 
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ADCException" />.
        /// Конструктор для обработки сериализации типа.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="contex">The contex.</param>
        protected ADCException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext contex)
            : base(info, contex) 
        { 
        }
    }

    /// <summary>
    /// Класс рассчета значений по калибровочным данным
    /// </summary>
    public class CalibrationValues
    {
        /// <summary>
        /// Калибровочные данные
        /// </summary>
        private List<CValue> _listValues;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CalibrationValues" />.
        /// </summary>
        public CalibrationValues()
        {
            _listValues = new List<CValue>();
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CalibrationValues" />.
        /// </summary>
        /// <param name="values">Массив калибровочных данных.</param>
        public CalibrationValues(CValue[] values)
        {
            _listValues = new List<CValue>();

            foreach (CValue cv in values)
            {
                if (_listValues.Contains(cv))
                {
                    ADCException exc = new ADCException("Ошибка: значение " + "(" + cv.XVal + "; " + cv.YVal + ")" + " с полем х = " + cv.XVal + " уже существует!");
                    throw exc;
                }

                _listValues.Add(cv);
            }
        }

        /// <summary>
        /// Метод добавляет новые калибровочные данные.
        /// </summary>
        /// <param name="value">Калибровочные данные.</param>
        public void Add(CValue value)
        {
            if (_listValues == null)
            {
                _listValues.Add(value);
            }
            else
            {
                if (_listValues.Contains(value))
                {
                    ADCException exc = new ADCException("Ошибка: значение " + "(" + value.XVal + ", " + value.YVal + ")" + " с полем х = " + value.XVal + " уже существует!");
                    throw exc;
                }

                _listValues.Add(value);
            }
        }

        /// <summary>
        /// Метод получения калибровочного значения по калибровочным данным.
        /// </summary>
        /// <param name="valueX">Входящее калибровочное значение</param>
        /// <param name="negativeIsOk">if set to <c>true</c> [negative is ok].</param>
        /// <returns>
        /// Калибровочное значение YVal
        /// </returns>
        public float Get(float valueX, bool negativeIsOk)
        {
            float value = 0;

            if (_listValues.Count == 1)
            {
                ADCException exc = new ADCException("Ошибка: мало калибровочных данных!");
                throw exc;
            }

            Sort();
            
            if (valueX <= _listValues[0].XVal)
            {
                if ((_listValues[1].XVal - _listValues[0].XVal) == 0)
                {
                    ADCException exc = new ADCException("Ошибка: деление на 0! Значения: " + _listValues[1].XVal);
                    throw exc;
                }

                value = _listValues[0].YVal - (((_listValues[1].YVal - _listValues[0].YVal) * (_listValues[0].XVal - valueX)) / (_listValues[1].XVal - _listValues[0].XVal));
                if ((negativeIsOk == false) && (value < 0)) 
                {
                    value = 0;
                }

                return value;
            }
            else if (valueX >= _listValues[_listValues.Count - 1].XVal)
            {
                if ((_listValues[_listValues.Count - 1].XVal - _listValues[_listValues.Count - 2].XVal) == 0)
                {
                    ADCException exc = new ADCException("Ошибка: деление на 0! Значения: " + _listValues[_listValues.Count - 1].XVal);
                    throw exc;
                }

                value = _listValues[_listValues.Count - 2].YVal + (((_listValues[_listValues.Count - 1].YVal - _listValues[_listValues.Count - 2].YVal) * (valueX - _listValues[_listValues.Count - 2].XVal))
                        / (_listValues[_listValues.Count - 1].XVal - _listValues[_listValues.Count - 2].XVal));
                if ((negativeIsOk == false) && (value < 0))
                {
                    value = 0;
                }

                return value;
            }
            else
            {
                for (int i = 0; i < _listValues.Count; i++)
                {
                    if (valueX <= _listValues[i].XVal)
                    {
                        if ((_listValues[i].XVal - _listValues[i - 1].XVal) == 0)
                        {
                            ADCException exc = new ADCException("Ошибка: деление на 0! Значения: " + _listValues[i].XVal);
                            throw exc;
                        }

                        value = _listValues[i - 1].YVal + (((_listValues[i].YVal - _listValues[i - 1].YVal) * (valueX - _listValues[i - 1].XVal))
                                / (_listValues[i].XVal - _listValues[i - 1].XVal));
                        if ((negativeIsOk == false) && (value < 0))
                        {
                            value = 0;
                        }

                        return value;
                    }
                }
            }
                        
            return 0;
        }

        /// <summary>
        /// Внутренний метод определяющий способ сортировки для метода Sort()
        /// </summary>
        /// <param name="cv_1">Первый объект CalibrationValue для сравнения</param>
        /// <param name="cv_2">Второй объект CalibrationValue для сравнения</param>
        /// <returns>Результат сравнения</returns>
        private static int CompareCalibrationValues(CValue cv_1, CValue cv_2)
        {
            return cv_1.XVal.CompareTo(cv_2.XVal);
        }

        /// <summary>
        /// Внутренний метод проводящий сортировку при добавлении новых калибровочных данных.
        /// </summary>
        private void Sort()
        {
            _listValues.Sort(CompareCalibrationValues);
        }
    }

    /// <summary>
    /// Класс канала для данных АЦП
    /// Обладает возможностью высчитывания среднего значения
    /// Для вывода результата могут использоваться калибровочные данные
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Максимальный уровен усреднения значения.
        /// </summary>
        private const uint MaxAverageLevel = 10;

        /// <summary>
        /// Уникальный ID канала
        /// </summary>
        private uint _uiId;

        /// <summary>
        /// Количество данных АЦП
        /// Значение не превышает MAX_AVERAGE_LEVEL
        /// </summary>
        private uint _uiDataCnt;

        /// <summary>
        /// Текущая позиция в _listDatas для записи новых данных АЦП.
        /// </summary>
        private byte _currentPoint;

        /// <summary>
        /// List данных АЦП
        /// </summary>
        private List<float> _listDatas;

        /// <summary>
        /// Калибровочные значения
        /// </summary>
        private CalibrationValues _clbrtValues;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="Channel" />.
        /// </summary>
        /// <param name="id">Новый ID канала, должен быть уникальным, иначе выробатывается исключение.</param>
        /// <param name="calibration">Набор калибровочные данных для канала, если калибровочных данных нет, передаем null.</param>
        /// <param name="averageLevel">Уровен усреднения значения - от 0 (не усредняем) до 10 (максимально по 10 значениям).</param>
        public Channel(uint id, CalibrationValues calibration, uint averageLevel)
        {
            _uiId = id;

            if (averageLevel > MaxAverageLevel)
            {
                ADCException exc = new ADCException("Ошибка: заданный уровень усреднения " + averageLevel + " не должен превышать " + MaxAverageLevel);
                throw exc; 
            }

            if (averageLevel == 0)
            {
                averageLevel = 1;
            }
                
            _listDatas = new List<float>();
            for (int i = 0; i < averageLevel; i++)
            {
                _listDatas.Add(0);
            }

            _clbrtValues = calibration;

            NegativeValuesIsCorrect = false;
        }

        /// <summary>
        /// Получает или задает значение, показывающее, допустимы ли отрицательных значений.
        /// Примечание:
        /// Отрицательные значение некорректны для данного канала, если при расчетах будут получатся значения меньше 0, выдаем 0.
        /// </summary>
        public bool NegativeValuesIsCorrect { get; set; }

        /// <summary>
        /// Метод определяет является ли передаваемый ID уникальным
        /// </summary>
        /// <param name="id">ID, который надо проверить на уникальность</param>
        /// <returns>true, если ID не уникальный, иначе - false</returns>
        public bool IsThisId(uint id)
        {
            return _uiId == id;
        }

        /// <summary>
        /// Метод добавляет новые данные в канал.
        /// Примечание:
        /// Если канал уже заполнен, то данные переписывают данные канал.
        /// </summary>
        /// <param name="data">Данные для записи в канал.</param>
        public void AddData(float data)
        {
            if (_currentPoint == _listDatas.Count)
            {
                _currentPoint = 0;
            }

            _listDatas[_currentPoint] = data;
            _currentPoint++;
            if (_uiDataCnt != _listDatas.Count)
            {
                _uiDataCnt++;
            }
        }

        /// <summary>
        /// Метод получения значения для канала, с учетом среднего значения и калибровочных данных.
        /// Примечание:
        /// Если в канале только одно значение - генерируется исключение.
        /// </summary>
        /// <returns>Значение для канала.</returns>
        public float GetValue()
        {
            float middle = 0;

            if (_currentPoint == 0)
            {
                ADCException exc = new ADCException("Ошибка: канал с ID " + _uiId + " не имеет данных!");
                throw exc; 
            }
            
            /*
            foreach (float fval in _listDatas)
                fMidle += fval;
             */
            middle = _listDatas.Sum() / _uiDataCnt;

            //// fMidle /= _uiDataCnt;

            if (_clbrtValues == null)
            {
                return middle;
            }

            return _clbrtValues.Get(middle, NegativeValuesIsCorrect); 
        }
    }

    /// <summary>
    /// Класс, поддерживающий получение, преобразование данных АЦП
    /// Обладает возможностью высчитывания среднего значения
    /// Для вывода результата могут использоваться калибровочные данные
    /// static void Main(string[] args)
    ///    {
    ///        CalibrationValue[] cv = new CalibrationValue[4];
    ///        CalibrationValues clValues;
    ///        ADC adc = new ADC();
    ///       //ИНИЦИАЛИЗАЦИЯ СТРУКТУРЫ КАЛИБРОВОЧНЫХ ЗНАЧЕНИЙ
    ///        cv[0].XVal = 10;
    ///        cv[0].YVal = 20;
    ///        cv[1].XVal = 11;
    ///        cv[1].YVal = 21;
    ///        cv[2].XVal = 14;
    ///        cv[2].YVal = 24;
    ///        cv[3].XVal = 16;
    ///        cv[3].YVal = 26;
    ///        clValues = new CalibrationValues(cv);
    ///       //ИНИЦИАЛИЗАЦИЯ КЛАССА ADC
    ///        adc.AddChannel(123, clValues, 1);
    ///        adc.AddChannel(456, clValues, 3);
    ///        adc.AddData(123, 6);
    ///        adc.AddData(123, 6);
    ///        adc.AddData(123, 12);
    ///        adc.AddData(456, 6);
    ///        adc.AddData(456, 6);
    ///        adc.AddData(456, 12);
    ///          //ВЫВОД ЗНАЧЕНИЙ КАНАЛОВ
    ///       Console.WriteLine("Result: {0} {1}", adc.GetValue(123), adc.GetValue(456));
    ///     Console.WriteLine("Press any key to exit.");
    ///        Console.ReadKey();
    ///    }
    /// </summary>
    public class ADC
    {
        /// <summary>
        /// List каналов
        /// </summary>
        private List<Channel> _listChannel;
        
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ADC" />.
        /// </summary>
        public ADC()
        {
            _listChannel = new List<Channel>();
        }

        /// <summary>
        /// Добавляем канал рассчета данных
        /// </summary>
        /// <param name="channelId">Уникальный Id канала. Если такой Id уже есть в классе, генерируем исключение</param>
        /// <param name="calibration">Набор калибровочных значений для данного канала. Если калибровочных значений нет, передаем null</param>
        /// <param name="averageLevel">Уровен усреднения значения - от 0 (не усредняем) до 10 (максимально по 10 значениям)</param>
        public void AddChannel(uint channelId, CalibrationValues calibration, uint averageLevel)
        {
            if (_listChannel.Count != 0)
            {
                foreach (Channel chnl in _listChannel)
                {
                    if (chnl.IsThisId(channelId))
                    {
                        ADCException exc = new ADCException("Ошибка: канал с ID " + channelId + " уже существует!");
                        throw exc;
                    }
                }
            }

            _listChannel.Add(new Channel(channelId, calibration, averageLevel));
        }

        /// <summary>
        /// Добавляем очередное изменение в канал.
        /// </summary>
        /// <param name="channelID">Уникальный ID канала.</param>
        /// <param name="newData">Данные для изменения.</param>
        public void AddData(uint channelID, float newData)
        {
            _listChannel[SearchChannel(channelID)].AddData(newData);
        }

        /// <summary>
        /// Рассчитываем значение для указанного канала.
        /// </summary>
        /// <param name="chanelId">Уникальный ID канала</param>
        /// <returns>Возвращает значение</returns>
        public float GetValue(uint chanelId) 
        {
            return _listChannel[SearchChannel(chanelId)].GetValue();
        }

        /// <summary>
        /// Метод определяет индекс канала в List по заданному ID.
        /// Примечание:
        /// Если заданный ID не существует - генерируется исключение.
        /// </summary>
        /// <param name="channelId">Заданный ID.</param>
        /// <returns>Индекс канала в List.</returns>
        private int SearchChannel(uint channelId)
        {
            for (int i = 0; i < _listChannel.Count; i++)
            {
                if (_listChannel[i].IsThisId(channelId))
                {
                    return i;
                }
            }

            ADCException exc = new ADCException("Ошибка: канал с ID " + channelId + " не найден!");
            throw exc;
        }
    }
}