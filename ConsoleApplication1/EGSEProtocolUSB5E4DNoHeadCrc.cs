//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolUSB5E4DNoHeadCrc.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace Egse.Protocols
{
    using System;
    using System.IO;
    using Egse.Utilites;

    /// <summary>
    /// класс декодера по протоколу 5E4D, без CRC8 заголовка.
    /// </summary>
    public class ProtocolUSB5E4DNoHeadCrc : ProtocolUSBBase
    {
        /// <summary>
        /// Длина заголовка кадра.
        /// </summary>
        private const uint FrameHeaderSize = 5;

        /// <summary>
        /// Максимально допустимый размер кадра.
        /// </summary>
        private const uint MaxFrameLen = 65536;

        /// <summary>
        /// Максимально допустимое количество ошибок декодера.
        /// Остальные ошибки не будут фиксироваться.
        /// </summary>
        private const uint MaxErrorCount = 100;

        /// <summary>
        /// Вспомогательная таблица для быстрого расчета CRC8ATM.
        /// </summary>
        private static byte[] _crc8Table = new byte[256] 
            {
                0x00, 0x91, 0xe3, 0x72, 0x07, 0x96, 0xe4, 0x75, 0x0e, 0x9f, 0xed, 0x7c, 0x09, 0x98, 0xea, 0x7b, 0x1c, 0x8d, 0xff, 0x6e, 0x1b, 0x8a, 0xf8, 
                0x69, 0x12, 0x83, 0xf1, 0x60, 0x15, 0x84, 0xf6, 0x67, 0x38, 0xa9, 0xdb, 0x4a, 0x3f, 0xae, 0xdc, 0x4d, 0x36, 0xa7, 0xd5, 0x44, 0x31, 0xa0, 
                0xd2, 0x43, 0x24, 0xb5, 0xc7, 0x56, 0x23, 0xb2, 0xc0, 0x51, 0x2a, 0xbb, 0xc9, 0x58, 0x2d, 0xbc, 0xce, 0x5f, 0x70, 0xe1, 0x93, 0x02, 0x77, 
                0xe6, 0x94, 0x05, 0x7e, 0xef, 0x9d, 0x0c, 0x79, 0xe8, 0x9a, 0x0b, 0x6c, 0xfd, 0x8f, 0x1e, 0x6b, 0xfa, 0x88, 0x19, 0x62, 0xf3, 0x81, 0x10, 
                0x65, 0xf4, 0x86, 0x17, 0x48, 0xd9, 0xab, 0x3a, 0x4f, 0xde, 0xac, 0x3d, 0x46, 0xd7, 0xa5, 0x34, 0x41, 0xd0, 0xa2, 0x33, 0x54, 0xc5, 0xb7, 
                0x26, 0x53, 0xc2, 0xb0, 0x21, 0x5a, 0xcb, 0xb9, 0x28, 0x5d, 0xcc, 0xbe, 0x2f, 0xe0, 0x71, 0x03, 0x92, 0xe7, 0x76, 0x04, 0x95, 0xee, 0x7f, 
                0x0d, 0x9c, 0xe9, 0x78, 0x0a, 0x9b, 0xfc, 0x6d, 0x1f, 0x8e, 0xfb, 0x6a, 0x18, 0x89, 0xf2, 0x63, 0x11, 0x80, 0xf5, 0x64, 0x16, 0x87, 0xd8, 
                0x49, 0x3b, 0xaa, 0xdf, 0x4e, 0x3c, 0xad, 0xd6, 0x47, 0x35, 0xa4, 0xd1, 0x40, 0x32, 0xa3, 0xc4, 0x55, 0x27, 0xb6, 0xc3, 0x52, 0x20, 0xb1, 
                0xca, 0x5b, 0x29, 0xb8, 0xcd, 0x5c, 0x2e, 0xbf, 0x90, 0x01, 0x73, 0xe2, 0x97, 0x06, 0x74, 0xe5, 0x9e, 0x0f, 0x7d, 0xec, 0x99, 0x08, 0x7a, 
                0xeb, 0x8c, 0x1d, 0x6f, 0xfe, 0x8b, 0x1a, 0x68, 0xf9, 0x82, 0x13, 0x61, 0xf0, 0x85, 0x14, 0x66, 0xf7, 0xa8, 0x39, 0x4b, 0xda, 0xaf, 0x3e, 
                0x4c, 0xdd, 0xa6, 0x37, 0x45, 0xd4, 0xa1, 0x30, 0x42, 0xd3, 0xb4, 0x25, 0x57, 0xc6, 0xb3, 0x22, 0x50, 0xc1, 0xba, 0x2b, 0x59, 0xc8, 0xbd, 
                0x2c, 0x5e, 0xcf
            };

        /// <summary>
        /// Экземпляр события: декодером обнаружено сообщение.
        /// </summary>
        private ProtocolMsgEventArgs _package;

        /// <summary>
        /// Экземпляр события: ошибка в декодере.
        /// </summary>
        private ProtocolErrorEventArgs _errorFrame;

        /// <summary>
        /// Текущее состояние декодера.
        /// </summary>
        private DecoderState _state = DecoderState.s5E;

        /// <summary>
        /// Длина сообщения текущего кадра.
        /// </summary>
        private uint _lenMsg = 0;

        /// <summary>
        /// CRC8 посчитанный на текущем шаге декодера.
        /// </summary>
        private byte _curCRC8 = 0;

        /// <summary>
        /// Текущий байт, обрабатываемый декодером.
        /// </summary>
        private byte _curByte = 0;

        /// <summary>
        /// Позиция текущего байта в буфере.
        /// </summary>
        private uint _posCurByte = 0;

        /// <summary>
        /// Позиция сообщения текущего кадра в буфере.
        /// </summary>
        private uint _posMsg = 0;

        /// <summary>
        /// Флаг обработки первой ошибки.
        /// false - первую ошибку не считаем, иначе считаем.
        /// </summary>
        private bool _finishFrame = false;

        /// <summary>
        /// Текущее кол-во ошибок декодера.
        /// </summary>
        private int _curErrorCount = 0;

        /// <summary>
        /// логер Encoder-а.
        /// </summary>
        private TxtLogger _encLogStream = null;

        /// <summary>
        /// логер Decoder-а.
        /// </summary>
        private FileStream _decLogStream = null;

        /// <summary>
        /// Логирование кодированного потока.
        /// </summary>
        private bool _writeEncLog = false;

        /// <summary>
        /// Логирование декодированного потока.
        /// </summary>
        private bool _writeDecLog = false;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolUSB5E4DNoHeadCrc" />.
        /// </summary>
        public ProtocolUSB5E4DNoHeadCrc()
        {
            _package = new ProtocolMsgEventArgs(MaxFrameLen);
            _errorFrame = new ProtocolErrorEventArgs(MaxFrameLen);
            Reset();
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolUSB5E4DNoHeadCrc" />.
        /// </summary>
        /// <param name="decLogStream">Ссылка на экземпляр логера Decoder-а.</param>
        /// <param name="encLogStream">Ссылка на экземпляр логера Encoder-а.</param>
        /// <param name="decLogEnable">Включить логер Decoder-а</param>
        /// <param name="encLogEnable">Включить логер Encoder-а</param>
        public ProtocolUSB5E4DNoHeadCrc(FileStream decLogStream, TxtLogger encLogStream, bool decLogEnable, bool encLogEnable)
            : this()
        {
            if ((decLogStream != null) && decLogStream.CanRead)
            {
                _decLogStream = decLogStream;
                _writeDecLog = decLogEnable;
            }

            _encLogStream = encLogStream;
            _writeEncLog = encLogEnable;
        }

        /// <summary>
        /// Состояния декодера.
        /// </summary>
        private enum DecoderState
        {
            /// <summary>
            /// 1 байт: 0x5E.
            /// </summary>
            s5E,

            /// <summary>
            /// 2 байт: 0x4D.
            /// </summary>
            s4D,

            /// <summary>
            /// 3 байт: адрес посылки.
            /// </summary>
            sADDR,

            /// <summary>
            /// 4 байт: старший байт размера сообщения.
            /// </summary>
            sNBH,

            /// <summary>
            /// 5 байт: младший байт размера сообщения.
            /// </summary>
            sNBL,

            /// <summary>
            /// 7..N байт: сообщение.
            /// </summary>
            sMSG,

            /// <summary>
            /// N+1 байт: CRC8ATM для всего кадра.
            /// </summary>
            sCRC
        }

        /// <summary>
        /// Получает или задает значение, показывающее, записывать ли кодирующий буфер в файл.
        /// </summary>
        public bool WriteEncLog
        {
            get
            {
                return _writeEncLog;
            }

            set
            {
                _writeEncLog = value;
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, записывать ли декодирующий буфер в файл.
        /// </summary>
        public bool WriteDecLog
        {
            get
            {
                return _writeDecLog;
            }

            set
            {
                _writeDecLog = value;
            }
        }

        /// <summary>
        /// Сброс текущего состояния декодера.
        /// </summary>
        public override void Reset()
        {
            _state = DecoderState.s5E;
            _curCRC8 = 0;
            _lenMsg = 0;
            _posMsg = 0;
        }

        /// <summary>
        /// Циклический декодер буфера данных
        /// </summary>
        /// <param name="buf">буфер данных</param>
        /// <param name="bufSize">размер данных(в байтах)</param>
        public override void Decode(byte[] buf, int bufSize)
        {
            _posCurByte = 0;
            if (_writeDecLog && (_decLogStream != null))
            {
                _decLogStream.Write(buf, 0, bufSize);
            }

            while (_posCurByte < bufSize)
            {
                _curByte = buf[_posCurByte];
                switch (_state)
                {
                    case DecoderState.s5E:
                        if (0x5E != _curByte)
                        {
                            if (_finishFrame)
                            {
                                _finishFrame = false;
                                OnErrorFrame(buf, _posCurByte, bufSize, Resource.Get("eMissing5E"));
                            }

                            Reset();
                        }
                        else
                        {
                            _state = DecoderState.s4D;
                        }

                        break;
                    case DecoderState.s4D:
                        if (0x4D != _curByte)
                        {
                            OnErrorFrame(buf, _posCurByte, bufSize, Resource.Get("eMissing4D"));
                            Reset();
                        }
                        else
                        {
                            _state = DecoderState.sADDR;
                        }

                        break;
                    case DecoderState.sADDR:
                        _package.Addr = _curByte;
                        _state = DecoderState.sNBH;
                        break;
                    case DecoderState.sNBH:
                        _lenMsg = (uint)(_curByte << 8);
                        _state = DecoderState.sNBL;
                        break;
                    case DecoderState.sNBL:
                        _lenMsg += _curByte;
                        _state = DecoderState.sMSG;
                        break;
                    case DecoderState.sMSG:
                        _package.Data[_posMsg++] = _curByte;
                        if (_posMsg == _lenMsg)
                        {
                            _package.DataLen = (int)_posMsg;
                            _state = DecoderState.sCRC;
                        }

                        break;
                    case DecoderState.sCRC:
                        if (_curCRC8 != _curByte)
                        {
                            OnErrorFrame(buf, _posCurByte, bufSize, Resource.Get("eBadCRC"));
                        }
                        else
                        {
                            OnProtocolMsg(this, _package);
                            _finishFrame = true;
                        }

                        Reset();
                        break;
                }

                if (DecoderState.s5E != _state)
                {
                    _curCRC8 = _crc8Table[_curCRC8 ^ _curByte];
                }

                _posCurByte++;
            }
        }

        /// <summary>
        /// Кодируем буфер
        /// </summary>
        /// <param name="addr">0..255 адресный байт</param>
        /// <param name="buf">Буфер данных</param>
        /// <param name="bufOut">Буфер сформированной посылки</param>
        /// <returns>true, если кодирование успешно</returns>
        public override bool Encode(uint addr, byte[] buf, out byte[] bufOut)
        {
            // длина пакета данных = макс.длина(65535) - (длина заголовка(5e 4d addr nbh nbl) + crc)
            if (buf.Length > MaxFrameLen - (FrameHeaderSize + 1))
            {
                bufOut = null;
                OnErrorFrame(buf, 0, buf.Length, Resource.Get("eMaxFrameSize"));
                return false;
            }

            bufOut = new byte[buf.Length + FrameHeaderSize + 1];
            bufOut[0] = 0x5E;
            bufOut[1] = 0x4D;
            try
            {
                bufOut[2] = checked((byte)addr);
            }
            catch (OverflowException)
            {
                OnErrorFrame(buf, 0, buf.Length, Resource.Get("eBadAddr"));
                return false;
            }

            bufOut[3] = unchecked((byte)(buf.Length >> 8));
            bufOut[4] = unchecked((byte)buf.Length);
            Array.Copy(buf, 0, bufOut, 5, buf.Length);
            bufOut[bufOut.GetUpperBound(0)] = 0;
            for (byte i = 0; i < bufOut.GetUpperBound(0); i++)
            {
                bufOut[bufOut.GetUpperBound(0)] = _crc8Table[bufOut[bufOut.GetUpperBound(0)] ^ bufOut[i]];
            }

            if (_writeEncLog && (_encLogStream != null))
            {
                _encLogStream.LogText = Converter.ByteArrayToHexStr(bufOut);
            }

            return true;
        }

        /// <summary>
        /// Формирует событие: ошибка декодера.
        /// </summary>
        /// <param name="buf">Буфер с ошибкой</param>
        /// <param name="pos">Позиция ошибки в буфере</param>
        /// <param name="len">Размер буфера</param>
        /// <param name="msg">Описание ошибки</param>
        private void OnErrorFrame(byte[] buf, uint pos, int len, string msg)
        {
            _curErrorCount++;
            if ((int)MaxErrorCount > _curErrorCount)
            {
                Array.Copy(buf, _errorFrame.Data, len);
                _errorFrame.ErrorPos = pos;
                _errorFrame.Msg = msg;
                OnProtocolError(this, _errorFrame);
            }
        }
    }
}
