//-----------------------------------------------------------------------
// <copyright file="EGSEThreadingProtocol.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

namespace Egse.Threading
{
    using System.IO;
    using System.Threading;
    using Egse.Protocols;

    /// <summary>
    /// Класс потока декодера, читающего из потока USBThread
    /// В классе на данный момент два конструктора, один - для подключения к USB,
    /// другой - для получения данных из Stream
    /// </summary>
    public class ProtocolThread
    {
        /// <summary>
        /// Размер буфера, при достижении которого происходит считывание данных из USB.
        /// </summary>
        private const int ReadBufferSizeInBytes = 1024;

        /// <summary>
        /// декодер, использующийся для декодирования данных 
        /// </summary>
        private ProtocolUSBBase _dec;

        /// <summary>
        /// признак, что нужно перевести декодер в начальное состояние (например, при подключении/отключении устройства)
        /// </summary>
        private bool _resetDecoderFlag;

        /// <summary>
        /// поток входных данных из USB 
        /// </summary>
        private FTDIThread _threadFTDI;

        /// <summary>
        /// Поток входных данных, если читаем из файла.
        /// </summary>
        private Stream _fileStream;

        /// <summary>
        /// максимальный размер буфера, который был доступен для чтения
        /// </summary>
        private uint _maxBufferSize;

        /// <summary>
        ///   сам поток, в котором будет происходить обработка
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// флаг остановки потока 
        /// </summary>
        private volatile bool _terminateFlag;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolThread" />.
        /// Основной конструктор потока декодера. Используется по-умолчанию, если читаем данные из USB.
        /// </summary>
        /// <param name="dec">Декодер, который будем использовать.</param>
        /// <param name="threadFTDI">Поток, из которого получаем данные для декодирования.</param>
        public ProtocolThread(ProtocolUSBBase dec, FTDIThread threadFTDI)
        {
            _dec = dec;
            _resetDecoderFlag = false;
            _maxBufferSize = 0;
            _threadFTDI = threadFTDI;
            _fileStream = null;
            _terminateFlag = false;

            _thread = new Thread(Execution);
            _thread.IsBackground = true;
            _thread.Start();
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolThread" />.
        /// Дополнительный конструктор потока декодера. Используется, если нужно декодировать данные из любого объекта Stream.
        /// </summary>
        /// <param name="dec">Декодер, который будем использовать.</param>
        /// <param name="stream">Stream, откуда будем брать данные для декодирования.</param>
        public ProtocolThread(ProtocolUSBBase dec, Stream stream)
        {
            _dec = dec;
            _fileStream = stream;

            // проверим, что поток может читать
            if (_fileStream.CanRead == false)
            {
                // TODO что тут должно быть?
            }

            _thread = new Thread(ExecutionStream);
            _thread.Start();
        }

        /// <summary>
        /// Получает или задает максимальный размер кольцевого буфера.
        /// </summary>
        public uint MaxBufferSize
        {
            get
            {
                return _maxBufferSize;
            }

            set
            {
                _maxBufferSize = 0;
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
        /// Для безопасного перевода декодара в начальное состояние (например, когда USB подключилось/отключилось)
        /// </summary>
        public void ResetDecoder()
        {
            _resetDecoderFlag = true;
        }

        /// <summary>
        /// Основной поток получения и декодирования данных. 
        /// Примечание:
        /// Вызывается из основного конструктора.
        /// </summary>
        private void Execution()
        {
            uint bytesToRead = 0;
            while (!_terminateFlag)
            {
                // если нужно перевеси декодер в начальное состояние
                if (_resetDecoderFlag)
                {
                    _resetDecoderFlag = false;
                    _dec.Reset();
                }

                // сколько байт можно считать из потока
                bytesToRead = (uint)_threadFTDI.BigBuf.BytesAvailable;

                // будем читать большими порциями
                if (bytesToRead >= ReadBufferSizeInBytes)
                {
                    // для статистики рассчитаем максимальную заполненность буфера, которая была
                    if (bytesToRead > _maxBufferSize)
                    {
                        _maxBufferSize = bytesToRead;
                    }

                    _dec.Decode(_threadFTDI.BigBuf.ReadBuf, _threadFTDI.BigBuf.ReadBufSize);
                    _threadFTDI.BigBuf.MoveNextRead();
                }

                System.Threading.Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Дополнительный поток получения и декодирования данных из Stream.
        /// Примечание:
        /// Останавливается, когда считали 0 байт.
        /// 566 ms (byte to byte)
        /// 144 ms (copyTo)
        /// </summary>
        private void ExecutionStream()
        {
            int bytesReaded = 1;
            byte[] tmpBuf = new byte[ReadBufferSizeInBytes];
#if DEBUG_TIME
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            while (bytesReaded > 0)
            {
                bytesReaded = _fileStream.Read(tmpBuf, 0, ReadBufferSizeInBytes);
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
    }
}
