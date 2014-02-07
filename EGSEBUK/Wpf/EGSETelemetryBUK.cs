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
        /// Gets a value indicating whether [busk power a1].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [busk power a1]; otherwise, <c>false</c>.
        /// </value>
        public bool BUSKPowerA1 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [busk power b1].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [busk power b1]; otherwise, <c>false</c>.
        /// </value>
        public bool BUSKPowerB1 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [busk power a2].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [busk power a2]; otherwise, <c>false</c>.
        /// </value>
        public bool BUSKPowerA2 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [busk power b2].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [busk power b2]; otherwise, <c>false</c>.
        /// </value>
        public bool BUSKPowerB2 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [bund power a1].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [bund power a1]; otherwise, <c>false</c>.
        /// </value>
        public bool BUNDPowerA1 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [bund power b1].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [bund power b1]; otherwise, <c>false</c>.
        /// </value>
        public bool BUNDPowerB1 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [bund power a2].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [bund power a2]; otherwise, <c>false</c>.
        /// </value>
        public bool BUNDPowerA2 { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [bund power b2].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [bund power b2]; otherwise, <c>false</c>.
        /// </value>
        public bool BUNDPowerB2 { get; private set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TelemetryBUK" />.
        /// </summary>
        public TelemetryBUK()
        {
            BUSKPowerA1 = false;
            BUSKPowerA2 = false;
            BUSKPowerB1 = false;
            BUSKPowerB2 = false;
            BUNDPowerA1 = false;
            BUNDPowerA2 = false;
            BUNDPowerB1 = false;
            BUNDPowerB2 = false;
        }

        /// <summary>
        /// Обрабатываем данные телеметрии.
        /// </summary>
        /// <param name="buf">Буфер с данными</param>
        public void Update(byte[] buf)
        {
            BUSKPowerA1 = (buf[6] & 1) == 1;
        }
    }
}
