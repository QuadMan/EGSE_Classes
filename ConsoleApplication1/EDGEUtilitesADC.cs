/*** EDGEUtilitesADC.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль логирования для КИА
**
** Author: Мурзин Святослав
** Project: КИА
** Module: EDGE UTILITES 
** Requires: 
** Comments:
 *
 * ПРИМЕР:
 * 
 * С# ------------------------------------------------------
 * 
 * static void Main(string[] args)
        {
            CalibrationValue[] cv = new CalibrationValue[4];
            CalibrationValues clValues;
            ADC adc = new ADC();
            
 *          //ИНИЦИАЛИЗАЦИЯ СТРУКТУРЫ КАЛИБРОВОЧНЫХ ЗНАЧЕНИЙ
            cv[0].XVal = 10;
            cv[0].YVal = 20;

            cv[1].XVal = 11;
            cv[1].YVal = 21;

            cv[2].XVal = 14;
            cv[2].YVal = 24;

            cv[3].XVal = 16;
            cv[3].YVal = 26;
            
            clValues = new CalibrationValues(cv);
            
 *          //ИНИЦИАЛИЗАЦИЯ КЛАССА ADC
            adc.AddChannel(123, clValues, 1);
            adc.AddChannel(456, clValues, 3);


            adc.AddData(123, 6);
            adc.AddData(123, 6);
            adc.AddData(123, 12);

            adc.AddData(456, 6);
            adc.AddData(456, 6);
            adc.AddData(456, 12);
 
 *          //ВЫВОД ЗНАЧЕНИЙ КАНАЛОВ
            Console.WriteLine("Result: {0} {1}", adc.GetValue(123), adc.GetValue(456));

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
**
** History:
**  0.1.0	(10.12.2013) -	Начальная версия 
 *  0.2.0   (18.12.2013) - Доработал класс CalibrationValues и Channel, чтобы была настройка - считать-ли отрицательные значения правильными при рассчетах
 *                          (выявилась ошибка при расчете значения, превышающего калибровочные данные - видимо, прямая экстраполируется неправильно)
**
*/

using System;
using System.Collections.Generic;

namespace EGSE.Utilites.ADC
{
//*****************************************************************************
//*****************************************************************************
    /// <summary>
    /// Специальное исключение для классов
    /// </summary>
    public class ADCException : ApplicationException
    {
        public ADCException() { }
        public ADCException(string message) : base(message) { }
        public ADCException(string message, Exception ex) : base(message) { }
        // Конструктор для обработки сериализации типа
        protected ADCException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext contex)
            : base(info, contex) { }
    }
//*****************************************************************************
//*****************************************************************************

    /// <summary>
    /// Структура калибровочных значений
    /// </summary>
    public struct CValue
    {
        public float XVal;
        public float YVal;
        public CValue(float x, float y)
        {
            XVal = x;
            YVal = y;
        }
    }
//*****************************************************************************
//*****************************************************************************

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
        /// Конструктор по умолчанию
        /// </summary>
        public CalibrationValues()
        {
            _listValues = new List<CValue>();
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="cValues">Массив калибровочных данных</param>
        public CalibrationValues(CValue[] cValues)
        {
            _listValues = new List<CValue>();

            foreach (CValue cv in cValues)
            {
                if (_listValues.Contains(cv))
                {
                    ADCException exc = new ADCException("Ошибка: значение " + "(" + cv.XVal + "; " + cv.YVal+")" + " с полем х = " + cv.XVal + " уже существует!");
                    throw exc;
                }
                _listValues.Add(cv);
            }
        }

        /// <summary>
        /// Внутренний метод определяющий способ сортировки для метода Sort()
        /// </summary>
        /// <param name="cv_1">Объект CalibrationValue</param>
        /// <param name="cv_2">Объект CalibrationValue</param>
        /// <returns>Результат сравнения</returns>
        private static int CompareCalibrationValues(CValue cv_1, CValue cv_2)
        {
            
            return cv_1.XVal.CompareTo(cv_2.XVal);
        }

        /// <summary>
        /// Внутренний метод проводящий сортировку при добавлении 
        /// новых калибровочных данных
        /// </summary>
        /// <param name="cValue">Новые калибровочные данные</param>

        private void Sort()
        {
            _listValues.Sort(CompareCalibrationValues);
        }

        /// <summary>
        /// Метод добавляет новые калибровочные данные
        /// </summary>
        /// <param name="cValue">Калибровочные данные</param>
        public void Add(CValue cValue)
        {
            if (_listValues == null)
                _listValues.Add(cValue);
            else
            {
                if (_listValues.Contains(cValue))
                {
                    ADCException exc = new ADCException("Ошибка: значение " + "(" + cValue.XVal + ", " + cValue.YVal + ")" + " с полем х = " + cValue.XVal + " уже существует!");
                    throw exc;
                }
                _listValues.Add(cValue);
            }
        }

        /// <summary>
        /// Метод получения калибровочного значения по калибровочным данным
        /// </summary>
        /// <param name="xValue">Входящее калибровочное значение</param>
        /// <returns>Калибровочное значение YVal</returns>
        public float Get(float xValue, bool NegativeIsOk)
        {
            float value = 0;

            if (_listValues.Count == 1)
            {
                ADCException exc = new ADCException("Ошибка: мало калибровочных данных!");
                throw exc;
            }

            Sort();
            
            if (xValue <= _listValues[0].XVal)
            {
                if ((_listValues[1].XVal - _listValues[0].XVal) == 0)
                {
                    ADCException exc = new ADCException("Ошибка: деление на 0! Значения: " + _listValues[1].XVal);
                    throw exc;
                }
                value = (_listValues[0].YVal - (_listValues[1].YVal - _listValues[0].YVal) * (_listValues[0].XVal - xValue)
                        / (_listValues[1].XVal - _listValues[0].XVal));
                if ((NegativeIsOk == false) && (value < 0)) {
                    value = 0;
                }
                return value;
            }
            else if (xValue >= _listValues[_listValues.Count - 1].XVal)
            {
                if ((_listValues[_listValues.Count - 1].XVal - _listValues[_listValues.Count - 2].XVal) == 0)
                {
                    ADCException exc = new ADCException("Ошибка: деление на 0! Значения: " + _listValues[_listValues.Count - 1].XVal);
                    throw exc;
                }
                value = (_listValues[_listValues.Count - 2].YVal + (_listValues[_listValues.Count - 1].YVal - _listValues[_listValues.Count - 2].YVal) * (xValue - _listValues[_listValues.Count - 2].XVal)
                        / (_listValues[_listValues.Count - 1].XVal - _listValues[_listValues.Count - 2].XVal)); 
                if ((NegativeIsOk == false) && (value < 0)) {
                    value = 0;
                }
                return value;
            }
            else
                for (int i = 0; i < _listValues.Count; i++)
                    if (xValue <= _listValues[i].XVal)
                    {
                        if ((_listValues[i].XVal - _listValues[i - 1].XVal) == 0)
                        {
                            ADCException exc = new ADCException("Ошибка: деление на 0! Значения: " + _listValues[i].XVal);
                            throw exc;
                        }
                        value = (_listValues[i - 1].YVal + (_listValues[i].YVal - _listValues[i - 1].YVal) * (xValue - _listValues[i - 1].XVal)
                                / (_listValues[i].XVal - _listValues[i - 1].XVal)); 
                        if ((NegativeIsOk == false) && (value < 0)) {
                            value = 0;
                        }
                        return value;
                    }
                        
            return 0;
        }
    }
//*****************************************************************************
//*****************************************************************************

    /// <summary>
    /// Класс канала для данных АЦП
    /// Обладает возможностью высчитывания среднего значения
    /// Для вывода результата могут использоваться калибровочные данные
    /// </summary>
    class Channel
    {
        /// <summary>
        /// Максимальный уровен усреднения значения
        /// </summary>
        const uint MAX_AVERAGE_LEVEL = 10;

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
        /// Текущая позиция в _listDatas для записи новых данных АЦП
        /// </summary>
        private byte _btCurPoint;

        /// <summary>
        /// List данных АЦП
        /// </summary>
        private List<float> _listDatas;

        /// <summary>
        /// Калибровочные значения
        /// </summary>
        private CalibrationValues _clbrtValues;

        /// <summary>
        /// Отрицательные значение некорректны для данного канала, если при расчетах будут получатся
        /// значения меньше 0, выдаем 0
        /// </summary>
        public bool NegativeValuesIsCorrect;

        /// <summary>
        /// Конструктор для класса Channel
        /// </summary>
        /// <param name="id">Новый ID канала
        /// Должен быть уникальным, иначе выробатывается исключение</param>
        /// <param name="calibration">Набор калибровочные данных для канала
        /// Если калибровочных данных нет, передаем null</param>
        /// <param name="averageLevel">Уровен усреднения значения - от 0 (не усредняем) до 10 (максимально по 10 значениям)</param>
        public Channel(uint id, CalibrationValues calibration, uint averageLevel)
        {
            _uiId = id;

            if (averageLevel > MAX_AVERAGE_LEVEL)
            {
                ADCException exc = new ADCException("Ошибка: заданный уровень усреднения " + averageLevel + " не должен превышать " + MAX_AVERAGE_LEVEL);
                throw exc; 
            }
            if (averageLevel == 0)
                averageLevel = 1;
                
            _listDatas = new List<float>();
            for (int i = 0; i < averageLevel; i++)
                _listDatas.Add(0);
            _clbrtValues = calibration;

            NegativeValuesIsCorrect = false;
        }

        /// <summary>
        /// Метод определяет является ли передаваемый ID уникальным
        /// </summary>
        /// <param name="id">ID, который надо проверить на уникальность</param>
        /// <returns>true, если ID не уникальный, иначе - false</returns>
        public bool IsThisId(uint id)
        {
            return (_uiId == id);
        }

        /// <summary>
        /// Метод добавляет новые данные в канал
        /// Если канал уже заполнен, то данные переписывают данные канал
        /// </summary>
        /// <param name="fData">Данные для записи в канал</param>
        public void AddData(float fData)
        {
                if (_btCurPoint == _listDatas.Count)
                    _btCurPoint = 0;
                _listDatas[_btCurPoint] = fData;
                _btCurPoint++;
                if (_uiDataCnt != _listDatas.Count)
                    _uiDataCnt++;
        }

        /// <summary>
        /// Метод получения значения для канала, с учетом среднего значения и
        /// калибровочных данных.
        /// Если в канале только одно значение - генерируется исключение
        /// </summary>
        /// <returns>Значение для канала</returns>
        public float GetValue()
        {
            float fMidle = 0;

            if (_btCurPoint == 0)
            {
                ADCException exc = new ADCException("Ошибка: канал с ID " + _uiId + " не имеет данных!");
                throw exc; 
            }
            
            foreach (float fval in _listDatas)
                fMidle += fval;

            fMidle /= _uiDataCnt;

            if (_clbrtValues == null)
                return fMidle;
            return _clbrtValues.Get(fMidle,NegativeValuesIsCorrect); 
        }
    }
//*****************************************************************************
//*****************************************************************************

    /// <summary>
    /// Класс, поддерживающий получение, преобразование данных АЦП
    /// Обладает возможностью высчитывания среднего значения
    /// Для вывода результата могут использоваться калибровочные данные
    /// </summary>
    public class ADC
    {
        /// <summary>
        /// List каналов
        /// </summary>
        private List<Channel> _listChannel;
        
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public ADC()
        {
            _listChannel = new List<Channel>();
        }

        /// <summary>
        /// Добавляем канал рассчета данных
        /// </summary>
        /// <param name="chId">Уникальный Id канала. Если такой Id уже есть в классе, генерируем исключение</param>
        /// <param name="calibration">Набор калибровочных значений для данного канала. Если калибровочных значений нет, передаем null</param>
        /// <param name="averageLevel">Уровен усреднения значения - от 0 (не усредняем) до 10 (максимально по 10 значениям)</param>
        public void AddChannel(uint chId, CalibrationValues calibration, uint averageLevel)
        {
            if (_listChannel.Count != 0)
                foreach (Channel chnl in _listChannel)
                    if (chnl.IsThisId(chId))
                    {
                        ADCException exc = new ADCException("Ошибка: канал с ID " + chId + " уже существует!");
                        throw exc; 
                    }
            _listChannel.Add(new Channel(chId, calibration, averageLevel));
        }

        /// <summary>
        /// Метод определяет индекс канала в List по заданному ID
        /// Если заданный ID не существует - генерируется исключение
        /// </summary>
        /// <param name="chId">Заданный ID</param>
        /// <returns>Индекс канала в List</returns>
        private int SearchChannel(uint chId)
        {
            for (int i = 0; i < _listChannel.Count; i++)
                if (_listChannel[i].IsThisId(chId))
                    return i;

            ADCException exc = new ADCException("Ошибка: канал с ID " + chId + " не найден!");
            throw exc; 
        }

        /// <summary>
        /// Добавляем очередное изменение в канал
        /// </summary>
        /// <param name="chId">Уникальный ID канала</param>
        /// <param name="Data">Данные</param>
        public void AddData(uint chId, float newData)
        {
            _listChannel[SearchChannel(chId)].AddData(newData);
        }

        /// <summary>
        /// Рассчитываем значение для указанного канала
        /// </summary>
        /// <param name="chId">Уникальный ID канала</param>
        /// <returns></returns>
        public float GetValue(uint chId) 
        {
            return _listChannel[SearchChannel(chId)].GetValue();
        }
    }
}