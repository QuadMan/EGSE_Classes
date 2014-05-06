//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolUSB7C6E.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

// TODO нужно написать юнит-тест для декодера

namespace Egse.Protocols 
{
    using System;
    using System.IO;
    using System.Linq;
    using Egse.Utilites;

    /// <summary>
    /// Класс USB-протокола формата: 7C6E...
    /// </summary>
    public class ProtocolUSB7C6E : ProtocolUSBBase
    {
        /// <summary>
        /// Максимальное количество ошибок декодера, при котором перестанет вызываться делегат.
        /// </summary>
        private const uint MaxErrorsCount = 100;

        /// <summary>
        /// Максимальный размер данных в одной посылке.
        /// Примечание:
        /// Размер по протоколу.
        /// </summary>
        private const int DecoderMaxDataLength = 256;

        /// <summary>
        /// Текущее состояние декодера.
        /// </summary>
        private int _step = 0;

        /// <summary>
        /// Текущий байт при декодировании посылки.
        /// </summary>
        private byte _bt = 0;
        
        /// <summary>
        /// Сколько ошибок протокола зафиксировал декодер.
        /// </summary>
        private uint _errorsCount = 0;

        /// <summary>
        /// Сообщение, формируется при окончании декодирования кадра.
        /// </summary>
        private ProtocolMsgEventArgs _protoMsg;

        /// <summary>
        /// Сообщение об ошибке в протоколе.
        /// </summary>
        private ProtocolErrorEventArgs _protoErrMsg;

        /// <summary>
        /// Внутренняя переменная.
        /// Примечание:
        /// Вычисляем размер буфера для копирования.
        /// </summary>
        private int _dt = 0;

        /// <summary>
        /// Внутренняя переменная.
        /// Примечание:
        /// Позиция последнего скопированного байта.
        /// </summary>
        private int _bufI = 0;

        /// <summary>
        /// Внутренняя переменная.
        /// Примечание:
        /// Длина формируемого сообщения.
        /// </summary>
        private int _msgLen = 0;

        /// <summary>
        /// Внутренняя переменная.
        /// Примечание:
        /// Флаг обнаружения ошибки "байт-мусор" между кадрами. 
        /// </summary>
        private bool _firstMsg = true;

        /// <summary>
        /// Экземпляр класса, отвечающий за логирование кодируемых данных.
        /// </summary>
        private TxtLogger _encodeStream;

        /// <summary>
        /// Экземпляр класса, отвечающий за логирование декодируемых данных.
        /// </summary>
        private FileStream _decoderStream;

        /// <summary>
        /// Запись бинарного лога данных USB.
        /// Примичание:
        /// При вызове соответствующего конструктора.
        /// </summary>
        private bool _writeEncLog = false;

        /// <summary>
        /// Запись бинарного лога данных USB.
        /// </summary>
        private bool _writeDecLog = false;
            
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolUSB7C6E" />.
        /// </summary>
        public ProtocolUSB7C6E()
        {
            _protoMsg = new ProtocolMsgEventArgs(DecoderMaxDataLength);
            _protoErrMsg = new ProtocolErrorEventArgs(70000);        // FIXIT: убрать константу
            _firstMsg = true;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolUSB7C6E" />.
        /// </summary>
        /// <param name="decodeStream">Поток файла, куда пишем decoder</param>
        /// <param name="encodeStream">Текслогер для encoder-а</param>
        /// <param name="decLog">if set to <c>true</c> [decoder log].</param>
        /// <param name="encLog">if set to <c>true</c> [encoder log].</param>
        public ProtocolUSB7C6E(FileStream decodeStream, TxtLogger encodeStream, bool decLog, bool encLog) : this()
        {
            _decoderStream = null;
            _encodeStream = null;

            _writeDecLog = false;

            if ((decodeStream != null) && decodeStream.CanRead)
            {
                _decoderStream = decodeStream;
                _writeDecLog = decLog;
            }

            _encodeStream = encodeStream;
                
            _writeEncLog = encLog;
        }

        /// <summary>
        /// Получает количество ошибок декодера.
        /// </summary>
        public uint ErrorsCount
        {
            get
            {
                return _errorsCount;
            }
        }

        /// <summary>
        /// Вызывается при инициализации декодера.
        /// </summary>
        public override void Reset()
        {
            _step = 0;
            _bt = 0;
            _bufI = 0;
            _msgLen = 0;
            _errorsCount = 0;
            _firstMsg = true;
        }

        /// <summary>
        /// Декодируем весь буфер.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        public override void Decode(byte[] buffer, int bufferSize)
        {
            int i = 0;

            if (_writeDecLog && (_decoderStream != null))
            {
                _decoderStream.Write(buffer, 0, bufferSize);
            }

            while (i < bufferSize)
            { 
                _bt = buffer[i];
                switch (_step) 
                {
                    case 0: if (_bt != 0x7C) 
                    {
                            if (!_firstMsg) 
                            {
                                // если после предыдущего сообщения сразу не встречается 0x7C - считаем ошибкой
                                _firstMsg = true;
                                _errorsCount++;
                                if (_errorsCount < MaxErrorsCount)
                                {
                                    MakeErrorMsg(buffer, i, bufferSize); // сформируем сообщение об ошибке и передадим его делегату
                                }
                            }

                            _step = -1;
                        }

                        break;
                    case 1: if (_bt != 0x6E) 
                    {
                            _step = 0;
                            _errorsCount++;
                            if (_errorsCount < MaxErrorsCount)
                            {
                                MakeErrorMsg(buffer, i, bufferSize); // сформируем сообщение об ошибке и передадим его делегату
                            }

                            _step = -1;
                        }

                        break;
                    case 2: _protoMsg.Addr = _bt;
                        break;
                    case 3: if (_bt == 0) 
                    {
                            _msgLen = (int)DecoderMaxDataLength;
                        }
                        else {
                            _msgLen = _bt;
                        }

                        _protoMsg.DataLen = _msgLen;
                        break;
                    default:
                        if (bufferSize - i >= _msgLen) 
                        {
                            // в текущем буфере есть вся наша посылка, просто копируем из буфера в сообщение
                            Array.Copy(buffer, i, _protoMsg.Data, _bufI, _msgLen);
                            OnProtocolMsg(this, _protoMsg);
                            _firstMsg = false;
                            _step = -1;
                            _bufI = 0;
                            i += _msgLen - 1; // ?_tmpMsg.len;
                        }
                        else // копируем только часть, до конца буфера 
                        {
                            _dt = bufferSize - i;
                            Array.Copy(buffer, i, _protoMsg.Data, _bufI, _dt);
                            _bufI += _dt;
                            _msgLen -= _dt;
                            i += _dt;
                        }
                           
                        break;
                }

                _step++;
                i++;
            }
        }

        /// <summary>
        /// Кодируем сообщение в соответствии с протоколом.
        /// Примечание:
        /// Можем закодировать только сообщение длиной не больше 256 байт.
        /// Адрес должен быть не больше 255.
        /// </summary>
        /// <param name="addr">Адрес, по которому нужно передать данные.</param>
        /// <param name="buf">Данные (максимум 256 байт).</param>
        /// <param name="bufOut">Выходной буфер.</param>
        /// <returns>True - если выполнено успешно.</returns>
        public override bool Encode(uint addr, byte[] buf, out byte[] bufOut)
        {
            bufOut = new byte[] {};
            int cntPacks = 1;

            if (256 < buf.Length)
            {
                cntPacks = buf.Length == (buf.Length / DecoderMaxDataLength) * DecoderMaxDataLength ? buf.Length / DecoderMaxDataLength : (buf.Length / DecoderMaxDataLength) + 1;
            }

            for (int i = 0; i < cntPacks; i++)
            {
                bufOut = Combine(bufOut, GetPack((byte)addr, buf.Skip(i * DecoderMaxDataLength).Take(DecoderMaxDataLength).ToArray()));
            }

            if (0xFFFF < bufOut.Length)
            {
                throw new MaxBufferFtdiException(Resource.Get(@"eMaxBufferFtdi"));
            }

            if (_writeEncLog && (_encodeStream != null)) 
            {
                _encodeStream.LogText = Converter.ByteArrayToHexStr(bufOut);
            }

            return true;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
        private byte[] GetPack(byte addr, byte[] buf)
        {
            byte[] bufOut;
            bufOut = new byte[buf.Length + 4];
            bufOut[0] = 0x7C;
            bufOut[1] = 0x6E;
            bufOut[2] = addr;
            bufOut[3] = (buf.Length == 256) ? (byte)0 : (byte)buf.Length; 
            Array.Copy(buf, 0, bufOut, 4, buf.Length);
            return bufOut;
        }

        /// <summary>
        /// Формируем сообщение об ошибке и отпарвляем его в делегат.
        /// </summary>
        /// <param name="buf">Буфер, содержащий ошибку.</param>
        /// <param name="bufPos">Позиция в буфере, где обнаружена ошибка.</param>
        /// <param name="bufferLen">Длина буфера.</param>
        private void MakeErrorMsg(byte[] buf, int bufPos, int bufferLen)
        {
            Array.Copy(buf, _protoErrMsg.Data, bufferLen);
            _protoErrMsg.ErrorPos = (uint)bufPos;
            _protoErrMsg.Msg = Resource.Get(@"eUnknownData"); 
            OnProtocolError(this, _protoErrMsg);
        }
    }
}
