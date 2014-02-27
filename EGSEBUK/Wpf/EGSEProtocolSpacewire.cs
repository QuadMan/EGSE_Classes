//-----------------------------------------------------------------------
// <copyright file="EGSEProtocolSpacewire.cs" company="IKI RSSI, laboratory №711">
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
    using EGSE.Protocols;

    /// <summary>
    /// Класс декодера по протоколу Spacewire.
    /// </summary>
    public class ProtocolSpacewire
    {
        /// <summary>
        /// Адресный байт "Данные".
        /// </summary>
        private readonly uint _data;

        /// <summary>
        /// Адресный байт "EOP или EEP".
        /// </summary>
        private readonly uint _eop;

        /// <summary>
        /// Адресный байт "Time tick 1".
        /// </summary>
        private readonly uint _time1;

        /// <summary>
        /// Адресный байт "Time tick 2".
        /// </summary>
        private readonly uint _time2;

        /// <summary>
        /// Текущий буфер, формируемого сообщения протокола spacewire.
        /// </summary>
        private List<byte> _buf;

        /// <summary>
        /// Текущее значение Tick time 1.
        /// </summary>
        private byte _currentTime1;

        /// <summary>
        /// Текущее значение Tick time 2.
        /// </summary>
        private byte _currentTime2;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProtocolSpacewire" />.
        /// </summary>
        /// <param name="data">Адресный байт "Данные".</param>
        /// <param name="eop">Адресный байт "EOP или EEP".</param>
        /// <param name="time1">Адресный байт "Time tick 1".</param>
        /// <param name="time2">Адресный байт "Time tick 2".</param>
        public ProtocolSpacewire(uint data, uint eop, uint time1, uint time2)
        {
            _data = data;
            _eop = eop;
            _time1 = time1;
            _time2 = time2;
            _buf = new List<byte>();
        }

        /// <summary>
        /// Объявление делегата обработки сообщений протокола spacewire.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpacewireSptpMsgEventArgs"/> instance containing the event data.</param>
        public delegate void SpacewireMsgEventHandler(object sender, SpacewireSptpMsgEventArgs e);

        /// <summary>
        /// Происходит, когда [сформировано сообщение spacewire].
        /// </summary>
        public event SpacewireMsgEventHandler GotSpacewireMsg;

        /// <summary>
        /// Метод, обрабатывающий сообщения от декодера USB.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">Сообщение для обработки.</param>
        public void OnMessageFunc(object sender, ProtocolMsgEventArgs msg)
        {
            if (msg != null)
            {             
                if (_data == msg.Addr)
                { 
                   for (int i = 0; i < msg.DataLen; i++)
                   {
                       _buf.Add(msg.Data[i]); 
                   }
                }
                else if (_eop == msg.Addr)
                {
                    SpacewireSptpMsgEventArgs _msg = new SpacewireIcdMsgEventArgs(_buf.ToArray(), _currentTime1, _currentTime2, msg.Data[0]);
                    OnSpacewireMsg(this, _msg);
                    _buf.Clear();
                }
                else if (_time1 == msg.Addr)
                {
                    _currentTime1 = msg.Data[0];
                }
                else if (_time2 == msg.Addr)
                {
                    _currentTime2 = msg.Data[0];
                }
            }
        }

        /// <summary>
        /// Обертка события: возникновение сообщения протокола в декодере spacewire.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">Класс описывающий сообщение протокола.</param>
        protected virtual void OnSpacewireMsg(object sender, SpacewireSptpMsgEventArgs e)
        {
            if (this.GotSpacewireMsg != null)
            {
                this.GotSpacewireMsg(sender, e);
            }
        }
    }
}
