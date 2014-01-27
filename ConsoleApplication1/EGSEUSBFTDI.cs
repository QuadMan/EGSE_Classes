/*** EDGE_USB_FTDI.cs
**
** (с) 2013 ИКИ РАН
 *
 * Модуль обертка для драйвера FTDI
**
** Author: Семенов Александр
** Project: КИА
** Module: EDGE USB FTDI
** Requires: 
** Comments:
**
** History:
**  0.1.0	(26.11.2013) -	Начальная версия
**
*/
namespace EGSE.USB
{
    using System;
    using FTD2XXNET;

    /// <summary>
    /// Настройка параметров USB устройства
    /// </summary>
    public struct USBCfg
    {
        /// <summary>
        /// сколько поток чтения данных из USB может "спать"
        /// </summary>
        public int Sleep;

        /// <summary>
        /// размер буфера чтения USB устройства
        /// </summary>
        public uint ReadBufferSize;
        
        /// <summary>
        /// размер буфера записи USB устройства
        /// </summary>
        public uint WriteBufferSize;
        
        /// <summary>
        /// Величина таймаута чтения USB устройства
        /// </summary>
        public uint ReadTimeOut;
        
        /// <summary>
        /// Величина таймаута записи USB устройства
        /// </summary>
        public uint WriteTimeOut;
        
        /// <summary>
        /// Значение latency
        /// </summary>
        public uint Latency;

        /// <summary>
        /// Конструктор конфигурации USB
        /// </summary>
        /// <param name="sleep">Сколько поток должен спать</param>
        public USBCfg(int sleep = 10)
        {
            Sleep = sleep;
            ReadBufferSize = 65535;
            WriteBufferSize = 1000;
            ReadTimeOut = 10;
            WriteTimeOut = 10;
            Latency = 2;
        }
    }

    /// <summary>
    /// Класс-обертка основных функций библиотеки FTD2XX
    /// </summary>
    public class USBFTDI
    {
        /// <summary>
        /// Максимальный размер буфера FTDI
        /// </summary>
        private const uint MAX_FTDI_IN_BUF_IN_BYTES = 65 * 1024;

        /// <summary>
        /// Бзовый класс FTD2XX
        /// </summary>
        private FTDICustom _ftdi;

        /// <summary>
        /// Серийный номер
        /// </summary>
        private string _serialNumber;

        /// <summary>
        /// Конфигурация USB
        /// </summary>
        private USBCfg _cfg;

        /// <summary>
        /// Используется для расчетамаксимального заполнения буфера USB FTDI
        /// </summary>
        private int _maxBufferSize;

        /// <summary>
        /// Открыто устройство или нет
        /// </summary>
        private bool _isOpen;
        
        /// <summary>
        /// Максимальное значение считанного буфера FTDI
        /// Можно определить (косвенно) было ли переполнение буфера (если значение равно 65535)
        /// </summary>
        public int MaxBufferSize
        {
            get
            {
                return _maxBufferSize;
            }

            set
            {
                _maxBufferSize = value;
            }
        }

        /// <summary>
        /// Открыто ли устройство
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="serial">Серийный номер</param>
        /// <param name="cfg">Конфигурация</param>
        public USBFTDI(string serial, USBCfg cfg)
        {
            _ftdi = new FTDICustom();
            _serialNumber = serial;
            _cfg = cfg;
            _maxBufferSize = 0;
            _isOpen = false;
        }

        /// <summary>
        /// Открываем устройство
        /// </summary>
        /// <returns>Статус открытия устройства</returns>
        public FTDICustom.FT_STATUS Open()
        {
            FTDICustom.FT_STATUS res = _ftdi.OpenBySerialNumber(_serialNumber);
            _isOpen = res == FTDICustom.FT_STATUS.FT_OK;

            if (_isOpen)
            {
                // TODO: SetConfigParameters
            }

            return res;
        }

        /// <summary>
        /// Закрываем устройство
        /// </summary>
        /// <returns>Статус выполнения операции</returns>
        public FTDICustom.FT_STATUS Close()
        {
            FTDICustom.FT_STATUS res = _ftdi.Close();
            _isOpen = false;

            return res;
        }

        /// <summary>
        /// получаем, сколько байт доступно в буфере FTDI
        /// </summary>
        /// <param name="bytesAvailable">Сколько байт доступно</param>
        /// <returns>Статус выполнения операции</returns>
        public FTDICustom.FT_STATUS GetBytesAvailable(ref int bytesAvailable)
        {
            FTDICustom.FT_STATUS res = _ftdi.GetRxBytesAvailable(ref bytesAvailable);
            if (res != FTDICustom.FT_STATUS.FT_OK) 
            {
                _isOpen = false;
            }

            return res;
        }

        /// <summary>
        /// Чтение буфера данных из FTDI
        /// </summary>
        /// <param name="inBuf">куда читаем</param>
        /// <param name="bytesToRead">сколько хотим прочитать</param>
        /// <param name="bytesReaded">сколько реально прочитали</param>
        /// <returns>Статус операции</returns>
        public FTDICustom.FT_STATUS ReadBuf(byte[] inBuf, int bytesToRead, ref int bytesReaded)
        {
            bytesReaded = 0;
            if (inBuf == null)
            {
                return FTDICustom.FT_STATUS.FT_OK;
            }

            if (_ftdi.Read(inBuf, bytesToRead, ref bytesReaded) != FTDICustom.FT_STATUS.FT_OK)
            {
                _isOpen = false;
                return FTDICustom.FT_STATUS.FT_IO_ERROR;
            }
            else
            {
                return FTDICustom.FT_STATUS.FT_OK;
            }
        }

        /// <summary>
        /// Читаем все, что есть в буфере FTDI
        /// </summary>
        /// <param name="inBuf">куда читать</param>
        /// <param name="bytesReaded">сколько считали</param>
        /// <returns>Статус операции</returns>
        public FTDICustom.FT_STATUS ReadAll(byte[] inBuf, ref int bytesReaded)
        {
            int bytesToRead = 0;
            FTDICustom.FT_STATUS res = _ftdi.GetRxBytesAvailable(ref bytesToRead);
            if ((res == FTDICustom.FT_STATUS.FT_OK) && (bytesToRead > 0))
            {
                res = _ftdi.Read(inBuf, bytesToRead, ref bytesReaded);
                if (bytesReaded > _maxBufferSize)
                {
                    _maxBufferSize = bytesReaded;
                }
            }

            _isOpen = res == FTDICustom.FT_STATUS.FT_OK;

            return res;
        }

        /// <summary>
        /// Записываем буфер данных в USB
        /// </summary>
        /// <param name="buf">Буфер</param>
        /// <param name="bytesWritten">Сколько байт записалось</param>
        /// <returns>Статус операции</returns>
        public FTDICustom.FT_STATUS WriteBuf(ref byte[] buf, out uint bytesWritten)
        {
            bytesWritten = 0;
            FTDICustom.FT_STATUS res = _ftdi.Write(buf, buf.Length, ref bytesWritten);
            _isOpen = res == FTDICustom.FT_STATUS.FT_OK;

            return FTDICustom.FT_STATUS.FT_OK;
        }
    }
}