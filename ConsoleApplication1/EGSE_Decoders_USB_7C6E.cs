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
**
*/

using System;
using EGSE.Decoders.USB;

namespace EGSE.Decoders.USB {

    /// <summary>
    /// </summary>
        public class USB_7C6EDecoder : USBProtocolBase
        {
            private const uint DECODER_MAX_DATA_LEN = 256;
            // текущее состояние декодера
            private uint _iStep = 0;
            // текущий байт при декодировании посылки
            private byte _bt = 0;
            // сколько ошибок протокола зафиксировал декодер
            private uint _errorsCount = 0;
            // сообщение, которое мы собираем и при окончании, пересылаем дальше в делегат
            private USBProtocolMsg _tmpMsg;
            // сообщение об ошибке в протоколе
            private USBProtocolErrorMsg _tmpErrMsg;
            // внутренние переменные
            private int _dt = 0;
            private int _bufI = 0;
            private int _msgLen = 0;
            private bool _firstMsg = true;
            
            /// <summary>
            /// Создаем декодер
            /// </summary>
            public USB_7C6EDecoder()
            {
                _tmpMsg = new USBProtocolMsg(DECODER_MAX_DATA_LEN);
                _tmpErrMsg = new USBProtocolErrorMsg(DECODER_MAX_DATA_LEN);
                reset();
            }
            
            /// <summary>
            /// Вызывается при инициализации декодера
            /// </summary>
            override public void reset()
            {
                _iStep = 0;
                _bt = 0;
                _bufI = 0;
                _msgLen = 0;
                _errorsCount = 0;
                _firstMsg = true;
                _tmpMsg.clear();
            }


            /// <summary>
            /// Формируем сообщение об ошибке и отпарвляем его в делегат
            /// </summary>
            /// <param name="buf">буфер, содержащий ошибку</param>
            /// <param name="bufPos">позиция в буфере, где обнаружена ошибка</param>
            /// <param name="bLen">длина буфера</param>
            private void makeErrorMsg(byte[] buf, int bufPos, int bLen)
            {
                if (onProtocolError != null)    
                {
                    Array.Copy(buf, _tmpErrMsg.data, bLen);
                    _tmpErrMsg.bufPos = (uint)bufPos;
                    onProtocolError(_tmpErrMsg);
                }
            }

            /// <summary>
            /// Декодируем весь буфер
            /// </summary>
            /// <param name="buf">входной буфер сообщения</param>
            override public void decode(byte[] buf)
            {
                int bLen = buf.Length;
                int i = 0;

                while (i < bLen) { 
                    _bt = buf[i++];
                    switch (_iStep) {
                        case 0 : if (_bt != 0x7C) {
                                continue;
                            }
                            else {
                                if (!_firstMsg)                      // если после предыдущего сообщения сразу не встречается 0x7C - считаем ошибкой
                                {
                                    _firstMsg = true;
                                    _errorsCount++;
                                    makeErrorMsg(buf, i, bLen);     // сформируем сообщение об ошибке и передадим его делегату
                                }     
                            }
                            break;
                        case 1 : if (_bt != 0x6E) {
                                _iStep = 0;
                                continue;
                            }
                            else { 
                                _errorsCount++;
                                makeErrorMsg(buf, i, bLen);         // сформируем сообщение об ошибке и передадим его делегату
                            }
                            break;
                        case 2 : _tmpMsg.addr = _bt;
                            break;
                        case 3 : if (_bt == 0) {
                                _msgLen = 256;
                            }
                            else {
                                _msgLen = _bt;
                            }
                            _tmpMsg.dataLen = _msgLen;
                            break;
                        default :
                            if (bLen - i >= _msgLen)                //  в текущем буфере есть вся наша посылка, просто копируем из буфера в сообщение
                            {
                                Array.Copy(buf, i, _tmpMsg.data, _bufI, _msgLen); 
                                if (onMessage != null)
                                {
                                    onMessage(_tmpMsg);
                                }
                                _firstMsg = false;
                                _iStep = 0;
                                _bufI = 0;
                                i += _msgLen;//? _tmpMsg.len;
                            }
                            else                                    // копируем только часть, до конца буфера 
                            {
                                _dt = bLen-i;
                                Array.Copy(buf,i,_tmpMsg.data, _bufI, _dt);
                                _bufI += _dt;
                                _msgLen -= _dt;
                                i += _dt;
                            }
                            break;
                    }
                    _iStep++;
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
            override public bool encode(uint addr, byte[] buf, out byte[] bufOut)
            {
                bufOut = null;
                if ((buf.Length > 256) || (addr > 255))
                {
                    return false;
                }
                byte bufLen = (buf.Length == 256) ? (byte)0 : checked ((byte)buf.Length);

                bufOut = new byte[buf.Length+4];
                bufOut[0] = 0x7C;
                bufOut[1] = 0x6E;
                bufOut[2] = checked ((byte)addr);
                bufOut[3] = bufLen;
                Array.Copy(buf, 0, bufOut, 4, buf.Length);

                return true;
            }

        }
}
