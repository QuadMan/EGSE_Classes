//-----------------------------------------------------------------------
// <copyright file="EGSETelemetryBUK.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using EGSE.Utilites;
    using EGSE.Utilites.ADC;

    /// <summary>
    /// Телеметрия устройства.
    /// </summary>
    public class TelemetryBUK
    {
        /// <summary>
        /// Получает значение, показывающее, есть ли питание у [ПК1 БУСК].
        /// </summary>
        /// <value>
        ///   <c>true</c>  если питание [ПК1 БУСК] есть; иначе, <c>false</c>.
        /// </value>
        public bool BUSKPower1 { get; private set; }

        /// <summary>
        /// Получает значение, показывающее, есть ли питание у [ПК2 БУСК].
        /// </summary>
        /// <value>
        ///   <c>true</c> если питание [ПК2 БУСК] есть; иначе, <c>false</c>.
        /// </value>
        public bool BUSKPower2 { get; private set; }

        /// <summary>
        /// Получает значение, показывающее, есть ли питание у [ПК1 БУНД].
        /// </summary>
        /// <value>
        ///   <c>true</c> если питание [ПК1 БУНД] есть; иначе, <c>false</c>.
        /// </value>
        public bool BUNDPower1 { get; private set; }

        /// <summary>
        /// Получает значение, показывающее, есть ли питание у [ПК2 БУНД].
        /// </summary>
        /// <value>
        ///   <c>true</c> если питание [ПК2 БУНД] есть; иначе, <c>false</c>.
        /// </value>
        public bool BUNDPower2 { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TelemetryBUK" />.
        /// </summary>
        public TelemetryBUK()
        {
            BUSKPower1 = false;
            BUSKPower2 = false;
            BUNDPower1 = false;
            BUNDPower2 = false;
        }

        /// <summary>
        /// Обрабатываем данные телеметрии.
        /// </summary>
        /// <param name="buf">Буфер с данными</param>
        public void Update(byte[] buf)
        {
            BUSKPower1 = (buf[3] & 0x80) == 0x80;
            BUSKPower2 = (buf[3] & 0x40) == 0x40;
            BUNDPower1 = (buf[3] & 0x20) == 0x20;
            BUNDPower2 = (buf[3] & 0x10) == 0x10;
        }
    }
}
