//-----------------------------------------------------------------------
// <copyright file="EGSEUtilitesBigBufferManager.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

namespace EGSE.Utilites
{

    /// <summary>
    /// Класс менеджера для большого кольцевого буфера
    /// Представляет собой двумерный массив. Первый индекс которого является указателем на большой массив 
    /// максимальным размером 70 КБ. При чтении и записи изменяются указатели первого индекса двумерного массива.
    /// </summary>
    public class BigBufferManager
    {
        /// <summary>
        /// Размер буфера по-умолчанию
        /// </summary>
        private const uint DEFAULT_BUF_SIZE = 100;

        /// <summary>
        /// Размер единичных массивов (ограничен драйвером FTDI, который не передает больше 65 КБ за раз)
        /// </summary>
        private const uint FTDI_BUF_SIZE = 70000;

        /// <summary>
        /// Получает или задает представление большого кольцевого буфера.
        /// AData[positionIdx][dataIdx]
        /// </summary>
        public byte[][] AData { get; set; }

        /// <summary>
        /// Здесь хранятся длины всех массивов, так как длина второго массива задана константой.
        /// </summary>
        private int[] _aLen;

        /// <summary>
        /// Текущая позиция чтения
        /// </summary>
        private uint _curRPos;

        /// <summary>
        /// Текущая позиция записи
        /// </summary>
        private uint _curWPos;

        /// <summary>
        /// Количество элементов в кольцевом буфере
        /// </summary>
        private int _count;

        /// <summary>
        /// Размер кольцевого буфера (в количестве массивов по 70 КБ)
        /// </summary>
        private uint _bufSize;

        /// <summary>
        /// Последняя позиция чтения
        /// </summary>
        private uint _lastRPos;

        /// <summary>
        /// Последняя позиция записи
        /// </summary>
        private uint _lastWPos;

        /// <summary>
        /// Объект для защиты функций moveNextRead и moveNextWrite
        /// </summary>
        private object thisLock = new object();

        /// <summary>
        /// Размер буфера в байтах, счиается при вызове функций moveNextRead и moveNextWrite
        /// </summary>
        private int _bytesInBuffer;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BigBufferManager" />.
        /// </summary>
        /// <param name="bufSize">Размер буфера</param>
        public BigBufferManager(uint bufSize = DEFAULT_BUF_SIZE)
        {
            _bufSize = bufSize;
            AData = new byte[_bufSize][];
            _aLen = new int[_bufSize];
            for (uint i = 0; i < _bufSize; i++)
            {
                AData[i] = new byte[FTDI_BUF_SIZE];
                _aLen[i] = 0;
            }

            _lastRPos = 0;
            _lastWPos = 0;
            _curWPos = 0;
            _curRPos = 0;
            _count = 0;
            _bytesInBuffer = 0;
        }

        /// <summary>
        /// Перемещает указатель чтения
        /// Обеспечивает защиту от одновременного изменения _count и _bytesInBuffer
        /// Изменяет _bytesInBuffer
        /// </summary>
        public void MoveNextRead()
        {
            lock (this)
            {
                _curRPos = (_curRPos + 1) % _bufSize;
                _count--;
                _bytesInBuffer -= _aLen[_lastRPos];
#if (DEBUG && CONSOLE)
                System.Console.WriteLine("readBuf, count = {0}, bytesAvailable = {1}, RPos = {2}", _count, _bytesInBuffer,_curRPos);
#endif
            }
        }

        /// <summary>
        /// Перемещает указатель записи
        /// Обеспечивает защиту от одновременного изменения _count и _bytesInBuffer
        /// Изменяет _bytesInBuffer и записывает длину буфера в ALen
        /// </summary>
        /// <param name="bufSize">Сколько было записано в текущий буфер</param>
        public void MoveNextWrite(int bufSize)
        {
            lock (this)
            {
                _curWPos = (_curWPos + 1) % _bufSize;
                _count++;
                _aLen[_lastWPos] = bufSize;
                _bytesInBuffer += bufSize;
#if (DEBUG && CONSOLE)
                System.Console.WriteLine("writeBuf, count = {0}, bytesAvailable = {1}, WPos = {2}", _count, _bytesInBuffer, _curWPos);
#endif
            }
        }

        /// <summary>
        /// Получает количество байт в буфере.
        /// </summary>
        public int BytesAvailable
        {
            get
            {
                return _bytesInBuffer;
            }
        }

        /// <summary>
        /// Получает размер последнего буфера для чтения.
        /// </summary>
        public int ReadBufSize
        {
            get
            {
                return _aLen[_lastRPos];
            }
        }

        /// <summary>
        /// Получает текущий буфер для чтения.
        /// Примечание:
        /// Если читать нечего, возвращает null.
        /// </summary>
        public byte[] ReadBuf
        {
            get
            {
                if (_count == 0)
                {
                    return null;
                }

                _lastRPos = _curRPos;
                return AData[_lastRPos];
            }
        }

        /// <summary>
        /// Получает текущий буфер для записи.
        /// Примечание:
        /// Если писать некуда, возвращает null.
        /// </summary>
        public byte[] WriteBuf
        {
            get
            {
                if (_count >= _bufSize)
                {
                    return null;
                }

                _lastWPos = _curWPos;
                return AData[_lastWPos];
            }
        }
    }
}
