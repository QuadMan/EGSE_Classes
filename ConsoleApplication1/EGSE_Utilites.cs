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
using System.Threading;

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
        private int _count;
        private uint _bufSize;
        private uint _oldPos;
        //private Object thisLock = new Object();

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

        public int bytesAvailable
        {
            get
            {
                return _bytesInBuffer;
            }

            set
            {
                Interlocked.Exchange(ref _bytesInBuffer, value);
            }
        }

        public byte[] getReadBuf
        {
            get
            {
                if (_count > 0)
                {
                    _oldPos = _curRPos;
                    Interlocked.Decrement(ref _count);//--;
                    _curRPos = (_curRPos + 1) % _bufSize;
                    System.Console.WriteLine("readBuf, count = {0}, bytesAvailable = {1}", _count, _bytesInBuffer);
                    return AData[_oldPos];
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
                    _oldPos = _curWPos;
                    _curWPos = (_curWPos + 1) % _bufSize;
                    Interlocked.Increment(ref _count);//++;
                    System.Console.WriteLine("writeBuf, count = {0}, bytesAvailable = {1}", _count, _bytesInBuffer);
                    return AData[_oldPos];
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