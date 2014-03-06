namespace EGSE.Protocols
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ProtocolHsi
    {
        /// <summary>
        /// Адресный байт "ВСИ".
        /// </summary>
        private readonly uint _dataAddr;

        public ProtocolHsi(uint dataAddr)
        {
            _dataAddr = dataAddr;
        }

        public delegate void HsiMsgEventHandler(object sender, HsiMsgEventArgs e);

        public event HsiMsgEventHandler GotHsiMsg;

        protected virtual void OnHsiMsg(object sender, HsiMsgEventArgs e)
        {
            if (this.GotHsiMsg != null)
            {
                this.GotHsiMsg(sender, e);
            }
        }

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

    }

    public class HsiMsgEventArgs : MsgBase
    {
        public HsiMsgEventArgs(byte[] data)
        {
            Data = new byte[data.Length];
            Array.Copy(data, Data, data.Length);
            DataLen = Data.Length;
        }
    }
}
