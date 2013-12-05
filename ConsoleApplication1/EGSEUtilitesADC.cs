/*** EDGEUtilitesADCcs
**
** (�) 2013 ��� ���
 *
 * ������ ����������� ��� ���
**
** Author: ������ ���������
** Project: ���
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
    /// ��������� ������������� ������
    /// </summary>
    struct CalibrationValue
    {
        uint XVal;
        uint YVal;
    }

    /// <summary>
    /// ����� �������� �������� �� ������������� ������
    /// 
    /// �����! ��� ���������� ����� ��� ��������, ����� ��������� ����������!!!!!!!!!!!!!!
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
    /// �����, �������������� ���������, �������������� ������ ���
    /// �������� ������������ ������������ �������� ��������
    /// ��� ������ ���������� ����� �������������� ������������� ������
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
        /// ��������� ����� �������� ������
        /// </summary>
        /// <param name="chId">���������� Id ������. ���� ����� Id ��� ���� � ������, ���������� ����������</param>
        /// <param name="calibration">����� ������������� �������� ��� ������� ������. ���� ������������� �������� ���, �������� null</param>
        /// <param name="averageLevel">���� ����������, ��������� ������ ���������� �������� - �� 0 (�� ���������) �� 10 (����������� �� 10 ���������)</param>
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
        /// ��������� ��������� ��������� � �����
        /// </summary>
        /// <param name="chId">���������� Id ������</param>
        /// <param name="Data">������</param>
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
        /// ������������ �������� ��� ���������� ������
        /// </summary>
        /// <param name="chId">���������� Id ������</param>
        /// <returns></returns>
        public float GetValue(uint chId) 
        {
            return 1;
        }
    }
}