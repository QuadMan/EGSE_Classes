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
using EGSE.Decoders.USB;
using EGSE.Threading;
using System.Threading;
using System.IO;

namespace EGSE.Threading
{
    /// <summary>
    /// Класс потока декодера, читающего из потока USBThread
    /// В классе на данный момент два конструктора, один - для подключения к USB,
    /// другой - для получения данных из Stream
    /// </summary>
    class DecoderThread
    {
        // размер буфера, при достижении которого происходит считывание данных из источника
        private const int READ_BUF_SIZE_IN_BYTES = 10 * 1024;
        // декодер, использующийся для декодирования данных
        private USBDecoder _dec;
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
        public DecoderThread(USBDecoder dec, FTDIThread fThread)
        {
            _dec = dec;
            _resetDecoderFlag = false;
            _maxCBufSize = 0;
            _fThread = fThread;
            _fStream = null;

            _thread = new Thread(Execution);
            _thread.Start();
        }

        /// <summary>
        /// Дополнительный конструктор потока декодера. Используется, если нужно декодировать данные из любого объекта Stream
        /// </summary>
        /// <param name="dec">Декодер, который будем использовать</param>
        /// <param name="fStream">Stream, откуда будем брать данные для декодирования</param>
        public DecoderThread(USBDecoder dec, Stream fStream)
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
            while (true)
            {
                if (_resetDecoderFlag)                              // если нужно перевеси декодер в начальное состояние
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
                    //_dec.GetData(_ftdi.bigBuf, bytesToRead);     // декодируем буфер
                }
                System.Threading.Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Дополнительный поток получения и декодирования данных из Stream
        /// Останавливается, когда считали 0 байт
        /// </summary>
        private void ExecutionStream()
        {
            int bytesReaded = 0;
            byte[] tmpBuf = new byte[READ_BUF_SIZE_IN_BYTES];
            bool streamFinished = false;
            while (!streamFinished)
            {
                bytesReaded = _fStream.Read(tmpBuf, 0, READ_BUF_SIZE_IN_BYTES);
                if (bytesReaded > 0)
                {
                    _dec.decode(tmpBuf,0,tmpBuf.Length);
                }
                else
                {
                    streamFinished = true;
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
