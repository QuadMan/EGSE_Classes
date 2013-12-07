/*** EDGE_Decoders_USB_7C6E.cs
**
** (с) 2013 ИКИ РАН
 *
** Класс декодера типа 7C 6E ADDR SIZE DATA
** ADDR - байт
** SIZE - байт (0 - 256 байт, 1..255)
** DATA - байтовый поток данных
 *
** При работе декодера ведется учет следующих ошибок:
**    1) после очередного распознанного сообщения, 0x7C не встретился
**    2) после 0x7C не встретился 0x6C
**
** Author: Семенов Александр
** Project: КИА
** Module: EDGE Decoders USB 7C6E
** Requires: 
** Comments:
 * - нужно написать юнит-тест для декодера
**
** History:
**  0.1.0	(29.11.2013) - Начальная версия
 *  0.2.0   (29.11.2013) - 
**
*/

using System;
using System.IO;
using System.Collections.Generic;

using EGSE.Protocols;

namespace EGSE.Protocols 
{
    /// <summary>
    /// </summary>
        public class ProtocolUSB7C6E : ProtocolUSBBase
        {
            // максимальное количество ошибок декдера, при котором мы перестаем вызывать делегата
            private const uint MAX_ERRORS_COUNT = 100;
            // максимальный размер данных в одной посылке, который доступен по протоколу
            private const uint DECODER_MAX_DATA_LEN = 256;

            // текущее состояние декодера
            private int _iStep = 0;
            // текущий байт при декодировании посылки
            private byte _bt = 0;
            // сколько ошибок протокола зафиксировал декодер
            private uint _errorsCount = 0;
            // сообщение, которое мы собираем и при окончании, пересылаем дальше в делегат
            private ProtocolMsgEventArgs _tmpMsg;
            // сообщение об ошибке в протоколе
            private ProtocolErrorEventArgs _tmpErrMsg;
            // внутренние переменные
            private int _dt = 0;
            private int _bufI = 0;
            private int _msgLen = 0;
            private bool _firstMsg = true;

            private TextWriter _fEncStream;
            private FileStream _fDecStream;
            // писать бинарный лог данных USB (при вызове соответствующего конструктора)
            public bool writeEncLog = false;
            public bool writeDecLog = false;
            
            /// <summary>
            /// Создаем декодер
            /// </summary>
            public ProtocolUSB7C6E()
            {
                _tmpMsg = new ProtocolMsgEventArgs(DECODER_MAX_DATA_LEN);
                _tmpErrMsg = new ProtocolErrorEventArgs(70000);        // FIXIT: убрать константу
                Reset();
            }

            /// <summary>
            /// Конструктор, позволяющий писать бинарный лог данных из USB
            /// </summary>
            /// <param name="fStream">Поток файла, куда пишем</param>
            /// <param name="writeBinLog">Флаг - писать сразу или нет</param>
            public ProtocolUSB7C6E(FileStream fDecStream, TextWriter fEncStream, bool wDecLog, bool wEncLog) : this()
            {
                _fDecStream = null;
                _fEncStream = null;

                writeDecLog = false;

                if ((fDecStream != null) && (fDecStream.CanRead))
                {
                    _fDecStream = fDecStream;
                    writeDecLog = wDecLog;
                }
                //
                _fEncStream = fEncStream;
                
                writeEncLog = wEncLog;
            }

            /// <summary>
            /// Количество ошибок декодера
            /// </summary>
            public uint errorsCount
            {
                get
                {
                    return _errorsCount;
                }
            }

            /// <summary>
            /// Вызывается при инициализации декодера
            /// </summary>
            override public void Reset()
            {
                _iStep = 0;
                _bt = 0;
                _bufI = 0;
                _msgLen = 0;
                _errorsCount = 0;
                _firstMsg = true;
            }


            /// <summary>
            /// Формируем сообщение об ошибке и отпарвляем его в делегат
            /// </summary>
            /// <param name="buf">буфер, содержащий ошибку</param>
            /// <param name="bufPos">позиция в буфере, где обнаружена ошибка</param>
            /// <param name="bLen">длина буфера</param>
            private void makeErrorMsg(byte[] buf, int bufPos, int bLen)
            {
                Array.Copy(buf, _tmpErrMsg.Data, bLen);
                _tmpErrMsg.ErrorPos = (uint)bufPos;
                OnProtocolError(_tmpErrMsg);
            }

            /// <summary>
            /// Декодируем весь буфер
            /// </summary>
            /// <param name="buf">входной буфер сообщения</param>
            override public void Decode(byte[] buf, int bufSize)
            {
                int i = 0;
                //
                if (writeDecLog && (_fDecStream != null))
                {
                    _fDecStream.Write(buf, 0, bufSize);
                }
                //
                while (i < bufSize)
                { 
                    _bt = buf[i];
                    switch (_iStep) {
                        case 0 : if (_bt != 0x7C) {
                                if (!_firstMsg)                      // если после предыдущего сообщения сразу не встречается 0x7C - считаем ошибкой
                                {
                                    _firstMsg = true;
                                    _errorsCount++;
                                    if (_errorsCount < MAX_ERRORS_COUNT)
                                    {
                                        makeErrorMsg(buf, i, bufSize);     // сформируем сообщение об ошибке и передадим его делегату
                                    }
                                }
                                _iStep = -1;
                            }
                            break;
                        case 1 : if (_bt != 0x6E) {
                                _iStep = 0;
                                _errorsCount++;
                                if (_errorsCount < MAX_ERRORS_COUNT)
                                {
                                    makeErrorMsg(buf, i, bufSize);         // сформируем сообщение об ошибке и передадим его делегату
                                }
                                _iStep = -1;
                            }
                            break;
                        case 2 : _tmpMsg.Addr = _bt;
                            break;
                        case 3 : if (_bt == 0) {
                                _msgLen = 256;
                            }
                            else {
                                _msgLen = _bt;
                            }
                            _tmpMsg.DataLen = _msgLen;
                            break;
                        default :
                            if (bufSize - i >= _msgLen)                //  в текущем буфере есть вся наша посылка, просто копируем из буфера в сообщение
                            {
                                Array.Copy(buf, i, _tmpMsg.Data, _bufI, _msgLen);
                                OnProtocolMsg(_tmpMsg);
                                _firstMsg = false;
                                _iStep = -1;
                                _bufI = 0;
                                i += _msgLen - 1;//? _tmpMsg.len;
                            }
                            else                                    // копируем только часть, до конца буфера 
                            {
                                _dt = bufSize - i;
                                Array.Copy(buf, i, _tmpMsg.Data, _bufI, _dt);
                                _bufI += _dt;
                                _msgLen -= _dt;
                                i += _dt;
                            }
                            
                            // медленный вариант, для проверки
                            //_tmpMsg.data[_bufI++] = _bt;
                            //if (--_msgLen == 0)
                            //{
                            //    if (onMessage != null)
                            //    {
                            //        onMessage(_tmpMsg);
                            //    }
                            //    _iStep = -1;
                            //    _bufI = 0;
                            //}
                            
                            break;
                    }
                    _iStep++;
                    i++;
                }
            }

            /// <summary>
            /// Кодируем сообщение в соответствии с протоколом
            /// Можем закодировать только сообщение длиной не больше 256 байт
            /// Адрес должен быть не больше 255
            /// </summary>
            /// <param name="addr">адрес, по которому нужно передать данные</param>
            /// <param name="buf">данные (максимум 256 байт)</param>
            /// <param name="bufOut">выходной буфер</param>
            override public bool Encode(uint addr, byte[] buf, out byte[] bufOut)
            {
                bufOut = null; 
                if ((buf.Length > 256) || (addr > 255))
                {
                    return false;
                }
                byte bufLen = (buf.Length == 256) ? (byte)0 : (byte)buf.Length;

                bufOut = new byte[buf.Length+4];
                bufOut[0] = 0x7C;
                bufOut[1] = 0x6E;
                bufOut[2] = (byte)addr; 
                bufOut[3] = bufLen;
                Array.Copy(buf, 0, bufOut, 4, buf.Length);

                if (writeEncLog && (_fEncStream != null)) 
                {
                    _fEncStream.WriteLine(BitConverter.ToString(bufOut));
                }
                return true;
            }

        }
}
