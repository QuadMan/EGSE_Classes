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
                    HsiMsgEventArgs _msg = new HsiMsgEventArgs(msg.Data);
                    OnHsiMsg(this, _msg);
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
        public HsiMsgEventArgs(byte[] data)
        {
            Data = new byte[data.Length];
            Array.Copy(data, Data, data.Length);
            DataLen = Data.Length;
        }
    }
}
