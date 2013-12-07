/*** EDGE_THREADING_Decoders.cs
**
** (с) 2013 ИКИ РАН
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
**  0.1.0	(26.11.2013) - Начальная версия
**  0.2.0   (27.11.2013) - Добавлен второй конструктор, в котором мы можем получать данных из Stream
*/

using System;
using System.Threading;
using System.IO;
using System.Diagnostics;

using EGSE.Protocols;
using EGSE.Threading;

namespace EGSE.Threading
{
    /// <summary>
    /// Класс потока декодера, читающего из потока USBThread
    /// В классе на данный момент два конструктора, один - для подключения к USB,
    /// другой - для получения данных из Stream
    /// </summary>
    class ProtocolThread
    {
        // размер буфера, при достижении которого происходит считывание данных из источника
        private const int READ_BUF_SIZE_IN_BYTES = 1024;
        // декодер, использующийся для декодирования данных
        private ProtocolUSBBase _dec;
        // признак, что нужно перевести декодер в начальное состояние (например, при подключении/отключении устройства)
        private bool _resetDecoderFlag;
        // поток входных данных из USB
        private FTDIThread _fThread;
        // поток входных данных
        private Stream _fStream;
        // максимальный размер буфера, который был доступен для чтения
        private uint _maxCBufSize;
        // сам поток, в котором будет происходить обработка
        private Thread _thread;
        // флаг остановки потока
        private volatile bool _terminateFlag;

        /// <summary>
        /// Максимальный размер кольцевого буфера 
        /// </summary>
        public uint maxCBufSize
        {
            set
            {
                _maxCBufSize = 0;
            }
            get
            {
                return _maxCBufSize;
            }
        }

        /// <summary>
        /// Основной конструктор потока декодера. Используется по-умолчанию, если читаем данные из USB
        /// </summary>
        /// <param name="dec">Декодер, который будем использовать</param>
        /// <param name="fThread">Поток, из которого получаем данные для декодирования</param>
        public ProtocolThread(ProtocolUSBBase dec, FTDIThread fThread)
        {
            _dec = dec;
            _resetDecoderFlag = false;
            _maxCBufSize = 0;
            _fThread = fThread;
            _fStream = null;
            _terminateFlag = false;

            _thread = new Thread(Execution);
            _thread.Start();
        }

        /// <summary>
        /// Дополнительный конструктор потока декодера. Используется, если нужно декодировать данные из любого объекта Stream
        /// </summary>
        /// <param name="dec">Декодер, который будем использовать</param>
        /// <param name="fStream">Stream, откуда будем брать данные для декодирования</param>
        public ProtocolThread(ProtocolUSBBase dec, Stream fStream)
        {
            _dec = dec;
            _fStream = fStream;

            // проверим, что поток может читать
            if (_fStream.CanRead == false)
            {
                // TODO
            }

            _thread = new Thread(ExecutionStream);
            _thread.Start();
        }

        /// <summary>
        /// Основной поток получения и декодирования данных. Вызывается из основного конструктора
        /// </summary>
        private void Execution()
        {
            uint bytesToRead = 0;
            while (!_terminateFlag)
            {
                if (_resetDecoderFlag)                              // если нужно перевеси декодер в начальное состояние
                {
                    _resetDecoderFlag = false;
                    _dec.Reset();
                }

                bytesToRead = (uint)_fThread.bigBuf.bytesAvailable;       // сколько байт можно считать из потока
                if (bytesToRead >= READ_BUF_SIZE_IN_BYTES)          // будем читать большими порциями
                {
                    if (bytesToRead > _maxCBufSize)                 // для статистики рассчитаем максимальную заполненность буфера, которая была
                    {
                        _maxCBufSize = bytesToRead;
                    }
                    _dec.Decode(_fThread.bigBuf.readBuf, _fThread.bigBuf.readBufSize);        // декодируем буфер
                    _fThread.bigBuf.moveNextRead();
                    //_fThread.bigBuf.bytesAvailable -= (int)bytesToRead;
                }
                System.Threading.Thread.Sleep(1);
            }
        }

        /// <summary>
        /// выставляем флаг остановки потока
        /// </summary>
        public void Finish()
        {
            _terminateFlag = true;
            _thread.Join(1000);
        }

        /// <summary>
        /// Дополнительный поток получения и декодирования данных из Stream
        /// Останавливается, когда считали 0 байт
        /// 566 ms (byte to byte)
        /// 144 ms (copyTo)
        /// </summary>
        private void ExecutionStream()
        {
            int bytesReaded = 1;
            byte[] tmpBuf = new byte[READ_BUF_SIZE_IN_BYTES];
#if DEBUG_TIME
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            while (bytesReaded > 0)
            {
                bytesReaded = _fStream.Read(tmpBuf, 0, READ_BUF_SIZE_IN_BYTES);
                if (bytesReaded > 0)
                {
                    _dec.Decode(tmpBuf, bytesReaded);
                }
                System.Threading.Thread.Sleep(1);
            }
#if DEBUG_TIME
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
#endif
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
