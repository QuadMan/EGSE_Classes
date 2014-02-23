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
    /// Телеметрия прибора КИА.
    /// </summary>
    public class TelemetryBUK
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TelemetryBUK" />.
        /// </summary>
        public TelemetryBUK()
        {
            PowerBusk1 = false;
            PowerBusk2 = false;
            PowerBund1 = false;
            PowerBund2 = false;
        }

        /// <summary>
        /// Получает значение, показывающее, есть ли питание у [ПК1 БУСК].
        /// </summary>
        /// <value>
        ///   <c>true</c>  если питание [ПК1 БУСК] есть; иначе, <c>false</c>.
        /// </value>
        public bool PowerBusk1 { get; private set; }

        /// <summary>
        /// Получает значение, показывающее, есть ли питание у [ПК2 БУСК].
        /// </summary>
        /// <value>
        ///   <c>true</c> если питание [ПК2 БУСК] есть; иначе, <c>false</c>.
        /// </value>
        public bool PowerBusk2 { get; private set; }

        /// <summary>
        /// Получает значение, показывающее, есть ли питание у [ПК1 БУНД].
        /// </summary>
        /// <value>
        ///   <c>true</c> если питание [ПК1 БУНД] есть; иначе, <c>false</c>.
        /// </value>
        public bool PowerBund1 { get; private set; }

        /// <summary>
        /// Получает значение, показывающее, есть ли питание у [ПК2 БУНД].
        /// </summary>
        /// <value>
        ///   <c>true</c> если питание [ПК2 БУНД] есть; иначе, <c>false</c>.
        /// </value>
        public bool PowerBund2 { get; private set; }

        /// <summary>
        /// Обрабатываем данные телеметрии.
        /// </summary>
        /// <param name="buf">Буфер с данными.</param>
        public void Update(byte[] buf)
        {
            PowerBusk1 = 0x80 == (buf[3] & 0x80);
            PowerBusk2 = 0x40 == (buf[3] & 0x40);
            PowerBund1 = 0x10 == (buf[3] & 0x10);
            PowerBund2 = 0x20 == (buf[3] & 0x20);
        }
    }
}
