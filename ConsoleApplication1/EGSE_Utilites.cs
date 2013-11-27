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
**
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EGSE.UTILITES
{
    /// <summary>
    /// Класс менеджера для кольцевого буфера
    /// </summary>
    class AManager
    {
        public const  uint NO_DATA_AVAILABLE = 0xFFFF;
        private const uint FTDI_BUF_SIZE = 70000;
        public byte[][] AData;
        private uint _curRPos;
        private uint _curWPos;
        private uint _count;
        private uint _bufSize;

        private uint _tmpCount;
        private uint _tmpPos;
        private int _bytesInBuffer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufSize"></param>
        public AManager(uint bufSize = 100)
        {
            _bufSize = bufSize;
            AData = new byte[_bufSize][];
            for (uint i = 0; i < _bufSize; i++)
            {
                AData[i] = new byte[FTDI_BUF_SIZE];
            }
            _curWPos = 0;
            _curWPos = 0;
            _count = 0;
        }

        public int getBytesAvailable
        {
            get
            {
                _tmpCount = _count;
                _tmpPos = _curRPos;
                _bytesInBuffer = 0;
                while (_tmpCount > 0)
                {
                    _bytesInBuffer += AData[_tmpPos].Length;
                    _tmpPos = (_tmpPos + 1) % _bufSize;
                }

                return _bytesInBuffer;
            }
        }

        public byte[] getReadBuf
        {
            get
            {
                if (_count > 0)
                {
                    _curRPos = (_curRPos + 1) % _bufSize;
                    _count--;
                    return AData[_curRPos];
                }
                else
                {
                    return null;
                }
            }
        }

        public byte[] getWriteBuf
        {
            get
            {
                if (_count < _bufSize)
                {
                    _curWPos = (_curWPos + 1) % _bufSize;
                    _count++;
                    return AData[_curWPos];
                }
                else
                {
                    return null;
                }
            }
        }

        public uint getReadBufIdx
        {
            get
            {
                if (_count > 0) {
                    _curRPos = (_curRPos + 1) % _bufSize;
                    _count--;
                    return _curRPos;
                }
                else {
                    return NO_DATA_AVAILABLE;
                }
            }
        }

        public uint getWriteBufIdx
        {
            get
            {
                if (_count < _bufSize)
                {
                    _curWPos = (_curWPos + 1) % _bufSize;
                    _count++;
                    return _curWPos;
                }
                else
                {
                    return NO_DATA_AVAILABLE;
                }
            }
        }
    }

    /// <summary>
    /// Класс рассчета значений по калибровочным данным
    /// </summary>
    class Measurements
    {
        public Measurements()
        {

        }

        ~Measurements()
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