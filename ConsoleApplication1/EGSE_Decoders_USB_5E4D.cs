/*
 * EDGE_Decoders_USB_5E4D.cs
 * 
 * Copyright(c) 2013 IKI RSSI, laboratory №711
 * 
 * Author: Коробейщиков Иван
 * Project: EDGE
 * Module: EDGE_Decoders_USB_5E4D
 */

using System;
using EGSE.Decoders.USB;

namespace EGSE.Decoders.USB
{
    /// <summary>
    /// 
    /// </summary>
    public class USB_5E4DDecoder : USBProtocolBase
    {
        private const uint MAX_BYTE = 256;
        private enum State
        {
            s5E, s4D, sADDR, sNBH, sNBL, sDATA, sCRCH, sCRC
        }
        private State _State = State.s5E;
        /// <summary>
        /// 
        /// </summary>
        public USB_5E4DDecoder()
        {
            reset();
        }
        /// <summary>
        /// 
        /// </summary>
        override public void reset()
        {
            _State = State.s5E;
        }
        /// <summary>
        /// Декодируем буфер
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="bufSize"></param>
        override public void decode(byte[] buf, int bufSize)
        {
            uint tmpNow = 0;
            uint tmpLen = 0;
            byte tmpAddr;
            while (tmpNow < bufSize)
            {
                switch (_State)
                {
                    case State.s5E:
                        if (0x5E == buf[tmpNow])
                        {
                            _State = State.s4D;
                        }
                        break;                    
                    case State.s4D:
                        if (0x4D == buf[tmpNow])
                        {
                            _State = State.sADDR;
                        }
                        break;
                    case State.sADDR:
                        tmpAddr = buf[tmpNow];
                        _State = State.sNBH;
                        break;
                    case State.sNBH:
                        tmpLen = buf[tmpNow] * MAX_BYTE;
                        _State = State.sNBL;
                        break;
                    case State.sNBL:
                        tmpLen += buf[tmpNow];
                        _State = State.sCRCH;
                        break;
                    case State.sDATA:
                    case State.sCRC:
                    case State.sCRCH:
                    default:                        
                        break;                        
                }
                tmpNow++;
            }
        }
        /// <summary>
        /// Кодируем буфер
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="buf"></param>
        /// <param name="bufOut"></param>
        /// <returns></returns>
        override public bool encode(uint addr, byte[] buf, out byte[] bufOut)
        {
            bufOut = new byte[0];
            return true;
        }
    }
}
