/*** EDGE_THREADING_Decoders.cs
**
** (с) 2013 Семенов Александр, ИКИ РАН
 *
 * Модуль потокового декодера данных
**
** Author: Семенов Александр
** Project: КИА
** Module: EDGE THREADING DECODERS
** Requires: 
** Comments:
**
** History:
**  0.1.0	(26.11.2013) -	Начальная версия
**
*/

using System;
using EGSE.Decoders;
using EGSE.Threading;
using System.Threading;

namespace EGSE.Threading
{
    /// <summary>
    /// Класс потока декодера, читающего из потока USBThread
    /// </summary>
    class DecoderThread
    {
        private const uint READ_BUF_SIZE_IN_BYTES = 10 * 1024;

        private Decoder _dec;
        private bool _resetDecoderFlag;
        private FTDIThread _fThread;
        private Thread _thread;
        private uint _maxCBufSize;

        /// <summary>
        /// Максимальный размер кольцевого буфера 
        /// </summary>
        public uint maxCBufSize
        {
            get
            {
                return _maxCBufSize;
            }
        }

        /// <summary>
        /// Сбрасываем максимальное значение размера кольцевого буфера, ждем новое
        /// </summary>
        public void resetCBufMaxSize()
        {
            _maxCBufSize = 0;
        }

        public DecoderThread(Decoder dec, FTDIThread fThread)
        {
            _dec = dec;
            _resetDecoderFlag = false;
            _maxCBufSize = 0;
            _fThread = fThread;

            _thread = new Thread(Execution);
            _thread.Start();
        }

        private void Execution()
        {
            uint bytesToRead = 0;
            while (true)
            {
                if (_resetDecoderFlag)                              // если переводим декодер в начальное состояние
                {
                    _resetDecoderFlag = false;
                    _dec.reset();
                }

                bytesToRead = _fThread.dataBytesAvailable;          // сколько байт можно считать из потока
                if (bytesToRead >= READ_BUF_SIZE_IN_BYTES)          // будем читать большими порциями
                {
                    if (bytesToRead > _maxCBufSize)                 // для статистики рассчитаем максимальную заполненность буфера, которая была
                    {
                        _maxCBufSize = bytesToRead;
                    }
                    //!_dec.GetData(_ftdi.bigBuf, bytesToRead);     // декодируем буфер
                }
                System.Threading.Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Для безопасного перевода декодара в начальное состояние (например, когда USB подключилось/отключилось)
        /// </summary>
        public void resetDecoder()
        {
            _resetDecoderFlag = true;
        }
    }
}
