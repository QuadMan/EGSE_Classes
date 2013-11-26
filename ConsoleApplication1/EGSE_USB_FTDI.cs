/*** EDGE_USB_FTDI.cs
**
** (с) 2013 Семенов Александр, ИКИ РАН
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
    public struct USBCfg
    {
        public int sleep;
        public uint rBufSz;
        public uint wBufSz;
        public uint rTO;
        public uint wTO;
        public uint latency;
    }

    class FTDI
    {
        private const uint MAX_FTDI_IN_BUF_IN_BYTES = 65 * 1024;
        private FTDICustom _ftdi;
        private string _sNum;
        private USBCfg _cfg;
        private uint _maxBufSize;
        private bool _isOpen;
        public byte[] _inBuf;           //! переделать под свойство get

        public uint maxBufSize
        {
            get
            {
                return _maxBufSize;
            }
        }

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

        public FTDICustom.FT_STATUS ReadAll(ref uint bytesReaded)
        {
            uint bytesToRead = 0;
            FTDICustom.FT_STATUS res = _ftdi.GetRxBytesAvailable(ref bytesToRead);
            if ((res == FTDICustom.FT_STATUS.FT_OK) && (bytesToRead > 0))
            {
                res = _ftdi.Read(_inBuf, bytesToRead, ref bytesReaded);
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

