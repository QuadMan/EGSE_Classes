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
        public int[] ALen;
        private uint _curRPos;
        private uint _curWPos;
        private int _count;
        private uint _bufSize;
        private uint _lastRPos;
        private Object thisLock = new Object();
        private uint _lastWPos;

        private int _bytesInBuffer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufSize"></param>
        public AManager(uint bufSize = 100)
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
        }

        public void moveNextRead()
        {
            lock (this)
            {
                _count--;
                _curRPos = (_curRPos + 1) % _bufSize;
                _bytesInBuffer -= ALen[_lastRPos];

                System.Console.WriteLine("readBuf, count = {0}, bytesAvailable = {1}, RPos = {2}", _count, _bytesInBuffer,_curRPos);
            }
        }

        public void moveNextWrite(int bufSize)
        {
            lock (this)
            {
                _curWPos = (_curWPos + 1) % _bufSize;
                _count++;
                ALen[_lastWPos] = bufSize;
                _bytesInBuffer += bufSize;

                System.Console.WriteLine("writeBuf, count = {0}, bytesAvailable = {1}, WPos = {2}", _count, _bytesInBuffer, _curWPos);
            }
        }

        public int bytesAvailable
        {
            get
            {
                return _bytesInBuffer;
            }
        }

        public int readBufSize
        {
            get
            {
                return ALen[_lastRPos];
            }
        }

        public int count
        {
            get
            {
                return _count;
            }
        }

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