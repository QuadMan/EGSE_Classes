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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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

        private enum DecodeState
        {
            Start,
            Flag,
            Hi,
            Lo,
            Data
        }

        private DecodeState _decodeState = DecodeState.Start;
        private byte _flag;
        private short _size = 0;
        private byte _lo;
        private byte _hi;
        private byte _line;
        private byte[] _buf;
        /// <summary>
        /// Метод, обрабатывающий сообщения от декодера USB.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">Сообщение для обработки.</param>
        public void OnMessageFunc(object sender, ProtocolMsgEventArgs msg)
        {
            if (msg != null)
            {
                if (_dataAddr == msg.Addr)
                {
                    int i = 0;
                    byte bt;
                    while (i < msg.DataLen)
                    {
                        bt = msg.Data[i];
                        switch (_decodeState)
                        {
                            case DecodeState.Start: if (0xA4 == bt)
                                {
                                    _decodeState = DecodeState.Flag;                                    
                                }
                                break;
                            case DecodeState.Flag: 
                                {
                                    _decodeState = DecodeState.Hi;
                                    _flag = bt;
                                }
                                break;
                            case DecodeState.Hi:
                                {
                                    _decodeState = DecodeState.Lo;
                                    _hi = bt;
                                    _size = (short)((short)(bt << 8) & 0x7FFF);
                                    _line = (byte)(bt & (1 << 7));
                                }
                                break;
                            case DecodeState.Lo:
                                {
                                    _decodeState = DecodeState.Data;
                                    _lo = bt;
                                    _size |= (short)bt;
                                    _buf = new byte[_size + 4];
                                    if (_size == 0)
                                    {
                                        _buf[0] = 0xa4;
                                        _buf[1] = _flag;
                                        _buf[2] = _hi;
                                        _buf[3] = _lo;
                                        HsiMsgEventArgs _msg = new HsiMsgEventArgs(_buf, _buf.Length);
                                        OnHsiMsg(this, _msg);
                                        _decodeState = DecodeState.Start;
                                    }
                                }
                                break;
                            default:
                                if (_size > 0)
                                {
                                    _buf[_buf.Length - _size] = bt;
                                    _size--;
                                    if (_size == 0)
                                    {
                                        HsiMsgEventArgs _msg = new HsiMsgEventArgs(_buf, _buf.Length);
                                        OnHsiMsg(this, _msg);
                                        _decodeState = DecodeState.Start;
                                    }
                                }
                                else
                                {
                                    HsiMsgEventArgs _msg = new HsiMsgEventArgs(_buf, _buf.Length);
                                    OnHsiMsg(this, _msg);
                                    _decodeState = DecodeState.Start;
                                }
                                break;
                        }

                        i++;
                    }                  
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
        /// <param name="data">The data.</param>
        public HsiMsgEventArgs(byte[] data, int length)
        {
            if (4 <= data.Length)
            {
                Data = new byte[length - 4];
                Array.Copy(data, Data, length - 4);
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

        public enum HsiLine
        {
            Main = 0x00,
            Resv = 0x01
        }

        public byte Flag { get; set; }

        public HsiLine Line { get; set; }

        public int Size { get; set; }
    }
}
