/*** EDGEUtilitesADCcs
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
**
** History:
**  0.1.0	
**
*/

using System;
using System.Collections.Generic;

namespace EGSE.Utilites
{
    /// <summary>
    /// Структура калибровочной кривой
    /// </summary>
    struct CalibrationValue
    {
        uint XVal;
        uint YVal;
    }

    /// <summary>
    /// Класс рассчета значений по калибровочным данным
    /// 
    /// ВАЖНО! При добавлении новых пар значений, нужно проводить сортировку!!!!!!!!!!!!!!
    /// </summary>
    class CalibrationValues
    {
        List<CalibrationValue> values;

        public CalibrationValues()
        {

        }

        public CalibrationValues(CalibrationValue[] cValues)
        {

        }

        public void Add(CalibrationValue cValue)
        {

        }

        public float Get(uint xValue)
        {
            return 1;
        }
    }

    /// <summary>
    /// Класс, поддерживающий получение, преобразование данных АЦП
    /// Обладает возможностью высчитывания среднего значения
    /// Для вывода результата могут использоваться калибровочные данные
    /// </summary>
    class ADC
    {
        const uint MAX_AVERAGE_LEVEL = 10;
        private uint _chCount;
        private float[][] _adcVal;
        private CalibrationValues[] _cValues;
        private byte[] _curDataPtr;

        public ADC()
        {
            _chCount = 0;
        }

        /// <summary>
        /// добавляем канал рассчета данных
        /// </summary>
        /// <param name="chId">Уникальный Id канала. Если такой Id уже есть в классе, генерируем исключение</param>
        /// <param name="calibration">Набор калибровочных значений для данного канала. Если калибровочных значений нет, передаем null</param>
        /// <param name="averageLevel">Если необходимо, указываем уровен усреднения значения - от 0 (не усредняем) до 10 (максимально по 10 значениям)</param>
        public void AddChannel(uint chId, CalibrationValues calibration, uint averageLevel)
        {
            if (averageLevel == 0)
            {
                averageLevel = 1;
            }
            _adcVal = new float[++_chCount][];
            _adcVal[_chCount - 1] = new float[averageLevel];
            _cValues[_chCount - 1] = calibration;
            _curDataPtr[_chCount - 1] = 0;
        }

        /// <summary>
        /// Добавляем очередное изменение в канал
        /// </summary>
        /// <param name="chId">Уникальный Id канала</param>
        /// <param name="Data">Данные</param>
        public void AddData(uint chId, uint Data)
        {
            float newData = Data;
            if (_cValues[chId] != null)
            {
                newData = 10;
            }
            _adcVal[chId][_curDataPtr[chId]] = newData;
            _curDataPtr[chId]++;
        }

        /// <summary>
        /// Рассчитываем значение для указанного канала
        /// </summary>
        /// <param name="chId">Уникальный Id канала</param>
        /// <returns></returns>
        public float GetValue(uint chId) 
        {
            return 1;
        }
    }
}