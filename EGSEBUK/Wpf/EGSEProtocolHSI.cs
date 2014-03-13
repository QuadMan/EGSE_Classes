//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolHsi.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using EGSE.Utilites;

    /// <summary>
    /// Декодер протокола ВСИ.
    /// </summary>
    public class ProtocolHsi
    {
        /// <summary>
        /// Адресный байт "ВСИ".
        /// </summary>
        private readonly uint _dataAddr;

        /// <summary>
        /// Текущий шаг декодера.
        /// </summary>
        private int _decodeState = 0;

        /// <summary>
        /// Количество ошибок декодера.
        /// Примечание: 
        /// Считает количество не распознанных байт.
        /// </summary>
        private int _errorCount;

        /// <summary>
        /// Текущий флаг кадра.
        /// </summary>
        private byte _flag;

        /// <summary>
        /// Текущий размер кадра.
        /// </summary>
        private short _size = -1;

        /// <summary>
        /// Текущий байт HI.
        /// </summary>
        private byte _hi;

        /// <summary>
        /// Текущий кадр.
        /// </summary>
        private byte[] _buf;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolHsi" />.
        /// </summary>
        /// <param name="dataAddr">The data addr.</param>
        public ProtocolHsi(uint dataAddr)
        {
            _dataAddr = dataAddr;
        }

        /// <summary>
        /// Обработка сообщений протокола ВСИ.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="HsiMsgEventArgs"/> instance containing the event data.</param>
        public delegate void HsiMsgEventHandler(object sender, HsiMsgEventArgs e);

        /// <summary>
        /// Происходит, когда [сформировано сообщение ВСИ].
        /// </summary>
        public event HsiMsgEventHandler GotHsiMsg;

        /// <summary>
        /// Метод, обрабатывающий сообщения от декодера USB.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">Сообщение для обработки.</param>
        public void OnMessageFunc(object sender, ProtocolMsgEventArgs msg)
        {
            new { msg }.CheckNotNull(); 
            if (_dataAddr != msg.Addr)
            {
                return;
            }

            for (int i = 0; i < msg.DataLen; i++)
            {
                switch (_decodeState)
                {
                    case 0:
                        if (0xA4 != msg.Data[i])
                        {
                            _errorCount++;
                            _decodeState = -1;
                        }

                        break;
                    case 1:
                        {
                            _flag = msg.Data[i];
                        }

                        break;
                    case 2:
                        {
                            _hi = msg.Data[i];
                        }

                        break;
                    case 3:
                        {
                            _size = (short)((short)(_hi << 8) & 0x7FFF | msg.Data[i]);
                            _buf = new byte[_size + 4];
                            _buf[0] = 0xa4;
                            _buf[1] = _flag;
                            _buf[2] = _hi;
                            _buf[3] = msg.Data[i];                            
                        }

                        break;
                    default:
                        {
                            _buf[_buf.Length - _size] = msg.Data[i];
                            _size--;
                        }

                        break;
                }

                _decodeState++;
                if (0 == _size)
                {
                    HsiMsgEventArgs _msg = new HsiMsgEventArgs(_buf, _buf.Length);
                    OnHsiMsg(this, _msg);
                    _decodeState = 0;
                    _size = -1;
                }
            }                  
        }

        /// <summary>
        /// Обертка события: возникновение сообщения протокола в декодере ВСИ.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий сообщение протокола.</param>
        protected virtual void OnHsiMsg(object sender, HsiMsgEventArgs e)
        {
            if (this.GotHsiMsg != null)
            {
                this.GotHsiMsg(sender, e);
            }
        }
    }
    
    /// <summary>
    /// Предоставляет аргументы для события, созданного полученным сообщением по протоколу ВСИ.
    /// </summary>
    public class HsiMsgEventArgs : MsgBase
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="HsiMsgEventArgs" />.
        /// </summary>
        /// <param name="data">Данные кадра.</param>
        /// <param name="length">Длина кадра.</param>
        public HsiMsgEventArgs(byte[] data, int length)
        {
            if (4 <= data.Length)
            {
                Data = new byte[length - 4];
                Array.Copy(data, 4, Data, 0, length - 4);
                DataLen = length - 4;
                Flag = data[1];
                Line = (HsiLine)(data[2] & (1 << 7));
                Size = ((data[2] << 8) | data[3]) & 0x7FFF;
            }
            else
            {
                Data = new byte[length];
                Array.Copy(data, Data, length);
                DataLen = length;
            }
        }

        /// <summary>
        /// Линия передачи ВСИ.
        /// </summary>
        public enum HsiLine
        {
            /// <summary>
            /// Основная линия ВСИ.
            /// </summary>
            Main = 0x00,

            /// <summary>
            /// Резервная линия ВСИ.
            /// </summary>
            Resv = 0x01
        }

        /// <summary>
        /// Получает или задает флаг кадра ВСИ.
        /// </summary>
        /// <value>
        /// Флаг кадра ВСИ.
        /// </value>
        public byte Flag { get; set; }

        /// <summary>
        /// Получает или задает линию передачи кадра.
        /// </summary>
        /// <value>
        /// Линия передачи кадра.
        /// </value>
        public HsiLine Line { get; set; }

        /// <summary>
        /// Получает или задает размер кадра ВСИ.
        /// </summary>
        /// <value>
        /// Размер кадра в байтах.
        /// </value>
        public int Size { get; set; }
    }
}
