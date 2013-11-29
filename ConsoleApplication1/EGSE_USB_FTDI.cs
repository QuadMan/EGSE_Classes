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

using System;
using FTD2XX_NET;

namespace EGSE.USB
{
    /// <summary>
    /// Настройка параметров USB устройства
    /// </summary>
    public struct USBCfg
    {
        /// <summary>
        /// сколько поток чтения данных из USB может "спать"
        /// </summary>
        public int sleep;
        /// <summary>
        /// размер буфера чтения USB устройства
        /// </summary>
        public uint rBufSz;
        /// <summary>
        /// размер буфера записи USB устройства
        /// </summary>
        public uint wBufSz;
        /// <summary>
        /// Величина таймаута чтения USB устройства
        /// </summary>
        public uint rTO;
        /// <summary>
        /// Величина таймаута записи USB устройства
        /// </summary>
        public uint wTO;
        /// <summary>
        /// Значение latency
        /// </summary>
        public uint latency;

        public USBCfg(int sleep)
        {
            this.sleep = 10;
            rBufSz = 65535;
            wBufSz = 1000;
            rTO = 10;
            wTO = 10;
            latency = 2;
        }
    }

    class FTDI
    {
        private const uint MAX_FTDI_IN_BUF_IN_BYTES = 65 * 1024;
        private FTDICustom _ftdi;
        private string _sNum;
        private USBCfg _cfg;
        private int _maxBufSize;
        private bool _isOpen;
        public byte[] _inBuf;           //! переделать под свойство get

        /// <summary>
        /// Максимальное значение считанного буфера FTDI
        /// Можно определить (косвенно) было ли переполнение буфера (если значение равно 65535)
        /// </summary>
        public int maxBufSize
        {
            set
            {
                _maxBufSize = value;
            }
            get
            {
                return _maxBufSize;
            }
        }

        /// <summary>
        /// Открыто ли устройство
        /// </summary>
        public bool isOpen
        {
            get
            {
                return _isOpen;
            }
        }

        public FTDI(string Serial, USBCfg cfg)
        {
            _ftdi = new FTDICustom();
            _sNum = Serial;
            _cfg = cfg;
            _maxBufSize = 0;
            _isOpen = false;
            _inBuf = new byte[MAX_FTDI_IN_BUF_IN_BYTES];
        }

        public FTDICustom.FT_STATUS Open()
        {
            FTDICustom.FT_STATUS res = _ftdi.OpenBySerialNumber(_sNum);
            _isOpen = (res == FTDICustom.FT_STATUS.FT_OK);

            if (_isOpen)
            {
                //!SetConfigParameters
            }

            return res;
        }

        public FTDICustom.FT_STATUS Close()
        {
            FTDICustom.FT_STATUS res = _ftdi.Close();
            _isOpen = false;

            return res;
        }

        public FTDICustom.FT_STATUS GetBytesAvailable(ref int bytesAvailable)
        {
            return _ftdi.GetRxBytesAvailable(ref bytesAvailable);
        }

        public FTDICustom.FT_STATUS ReadBuf(byte[] inBuf, int bytesToRead, ref int bytesReaded)
        {
            bytesReaded = 0;
            if (inBuf == null) return FTDICustom.FT_STATUS.FT_OK;

            return _ftdi.Read(inBuf, bytesToRead, ref bytesReaded);
        }

        public FTDICustom.FT_STATUS ReadAll(byte[] inBuf, ref int bytesReaded)
        {
            int bytesToRead = 0;
            FTDICustom.FT_STATUS res = _ftdi.GetRxBytesAvailable(ref bytesToRead);
            if ((res == FTDICustom.FT_STATUS.FT_OK) && (bytesToRead > 0))
            {
                res = _ftdi.Read(inBuf, bytesToRead, ref bytesReaded);
                if (bytesReaded > _maxBufSize)
                {
                    _maxBufSize = bytesReaded;
                }
            }
            _isOpen = (res == FTDICustom.FT_STATUS.FT_OK);

            return res;
        }

        public FTDICustom.FT_STATUS WriteBuf(ref byte[] buf, out uint bytesWritten)
        {
            bytesWritten = 0;
            FTDICustom.FT_STATUS res = _ftdi.Write(buf, buf.Length, ref bytesWritten);
            _isOpen = (res != FTDICustom.FT_STATUS.FT_OK);

            return FTDICustom.FT_STATUS.FT_OK;
        }
    }
}

