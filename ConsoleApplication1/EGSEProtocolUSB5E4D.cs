/*
 * EDGE_Decoders_USB_5E4D.cs
 * 
 * Copyright(c) 2013 IKI RSSI, laboratory №711
 * 
 * Author: Коробейщиков Иван
 * Project: EDGE
 * Module: EDGE_Decoders_USB_5E4D
 */

using EGSE.Utilites;
using System;
using System.IO;

namespace EGSE.Protocols
{
    /// <summary>
    /// класс декодера по протоколу 5E4D
    /// </summary>
    public class ProtocolUSB5E4D : ProtocolUSBBase
    {
        private enum DecoderState
        {
            s5E,    // 1 байт: 0x5E 
            s4D,    // 2 байт: 0x4D
            sADDR,  // 3 байт: адрес посылки
            sNBH,   // 4 байт: старший байт размера сообщения
            sNBL,   // 5 байт: младший байт размера сообщения
            sCRCH,  // 6 байт: CRC8ATM для заголовка кадра
            sMSG,   // 7..N байт: сообщение          
            sCRC    // N+1 байт: CRC8ATM для всего кадра
        }
        private ProtocolMsgEventArgs _package;
        private ProtocolErrorEventArgs _errorFrame;
        private const uint PROTOCOL_FRAME_SIZE = 6;       
        private const uint MAX_FRAME_LEN = 65536;
        private const uint MAX_ERROR_COUNT = 100;
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
        private DecoderState _state = DecoderState.s5E;
        private uint _lenMsg = 0; // длина сообщения текущего кадра
        private byte _curCRC8 = 0;
        private byte _curByte = 0;
        private uint _posCurByte = 0;
        private uint _posMsg = 0; // позиция сообщения текущего кадра в буфере 
        private bool _isFinishFrame = false; // false - первую ошибку не считаем, иначе считаем
        private int _maxErrorCount = 0;
        private int _curErrorCount = 0;
        private TxtLogger _encLogStream;
        private FileStream _decLogStream;
        
        /// <summary>
        /// Включить лог Encoder-а
        /// </summary>
        public bool IsWriteEncLog = false;
        
        /// <summary>
        /// Включить лог Decoder-а
        /// </summary>
        public bool IsWriteDecLog = false;
        
        /// <summary>
        /// Коснтруктор по-умолчанию
        /// </summary>
        public ProtocolUSB5E4D()
        {
            _package = new ProtocolMsgEventArgs(MAX_FRAME_LEN);
            _errorFrame = new ProtocolErrorEventArgs(MAX_FRAME_LEN);
            _maxErrorCount = (int)MAX_ERROR_COUNT;
            Reset();
        }

        /// <summary>
        /// Конструктор с поддержкой логеров
        /// </summary>
        /// <param name="argDecLogStream"></param>
        /// <param name="fEncStream"></param>
        /// <param name="wDecLog"></param>
        /// <param name="wEncLog"></param>
        public ProtocolUSB5E4D(FileStream argDecLogStream, TxtLogger fEncStream, bool wDecLog, bool wEncLog)
            : this()
        {
            _decLogStream = null;
            _encLogStream = null;
            if ((argDecLogStream != null) && (argDecLogStream.CanRead))
            {
                _decLogStream = argDecLogStream;
                IsWriteDecLog = wDecLog;
            }
            _encLogStream = fEncStream;
            IsWriteEncLog = wEncLog;
        }

        /// <summary>
        /// сброс текущего состояния декодера
        /// </summary>
        override public void Reset()
        {
            _state = DecoderState.s5E;
            _curCRC8 = 0;
            _lenMsg = 0;
            _posMsg = 0;
        }
        
        private void OnErrorFrame(byte[] buf, uint bufPos, int bLen, string msg)
        {
            _curErrorCount++;
            if (_maxErrorCount > _curErrorCount)
            {
                Array.Copy(buf, _errorFrame.Data, bLen);
                _errorFrame.ErrorPos = bufPos;
                _errorFrame.DataLen = (bLen > 256) ? 255 : bLen;
                _errorFrame.Msg = msg;
                OnProtocolError(_errorFrame);
            }
        }
        /// <summary>
        /// Циклический декодер буфера данных
        /// </summary>
        /// <param name="buf">буфер данных</param>
        /// <param name="bufSize">размер данных(в байтах)</param>
        override public void Decode(byte[] buf, int bufSize)
        {
            _posCurByte = 0;
            if (IsWriteDecLog && (_decLogStream != null))
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
                            if (_isFinishFrame)
                            {
                                _isFinishFrame = false;
                                OnErrorFrame(buf, _posCurByte, bufSize, "После сообщения не встретился 0x5E");
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
                            OnErrorFrame(buf, _posCurByte, bufSize, "После 0x5E отсутствует 0x4D");
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
                        _state = DecoderState.sCRCH;
                        break;
                    case DecoderState.sCRCH:
                        _state = DecoderState.sMSG;
                        if (_curCRC8 != _curByte)
                        {
                            OnErrorFrame(buf, _posCurByte, bufSize, "CRC8 заголовка расчитан неверно");
                            Reset();
                        }
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
                            OnErrorFrame(buf, _posCurByte, bufSize, "CRC8 кадра расчитан неверно");
                        }
                        else
                        { 
                            OnProtocolMsg(_package);
                            _isFinishFrame = true;
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
        /// <param name="buf">данные</param>
        /// <param name="bufOut">посылка</param>
        /// <returns>true, если кодирование успешно</returns>
        override public bool Encode(uint addr, byte[] buf, out byte[] bufOut)
        {
            if (buf.Length > MAX_FRAME_LEN - PROTOCOL_FRAME_SIZE)  // длина пакета данных = макс.длина(65535) - длина заголовка(5e 4d addr nbh nbl crch <data> crc)
            {
                bufOut = null;
                OnErrorFrame(buf, 0, buf.Length, "Превышен максимальный размер кадра");
                return false;
            }
            bufOut = new byte[buf.Length + PROTOCOL_FRAME_SIZE + 1];
            bufOut[0] = 0x5E;
            bufOut[1] = 0x4D;             
            try
            {               
               bufOut[2] = checked((byte)addr);
               bufOut[3] = checked((byte)(buf.Length >> 8));
            }
            catch (OverflowException)
            {
                OnErrorFrame(buf, 0, buf.Length, "Ошибка переполнения");
                return false;  
            }            
            bufOut[4] = unchecked((byte)buf.Length);
            bufOut[5] = 0;                
            for (byte i = 0; i < 5; i++)            
            {
                bufOut[5] = _crc8Table[bufOut[5] ^ bufOut[i]];
            }
            Array.Copy(buf, 0, bufOut, 6, buf.Length);
        //    bufOut[6 + buf.Length] = 0;
        //    for (uint i = PROTOCOL_FRAME_SIZE; i < bufOut.Length-1; i++)  
        //    {
        //        bufOut[bufOut.Length-1] = _crc8Table[bufOut[bufOut.Length-1] ^ bufOut[i]];
        //    }
            bufOut[bufOut.GetUpperBound(0)] = 0;
            for (byte i = 0; i < bufOut.GetUpperBound(0) - 1; i++)
            {
                bufOut[bufOut.GetUpperBound(0)] = _crc8Table[bufOut[bufOut.GetUpperBound(0)] ^ bufOut[i]];
            }
            if (IsWriteEncLog && (_encLogStream != null))
            {
                _encLogStream.LogText = Converter.ByteArrayToHexStr(bufOut);
            }
            return true;
        }
    }
}
