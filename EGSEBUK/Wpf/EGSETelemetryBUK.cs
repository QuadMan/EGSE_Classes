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
        /// Питание устройства.
        /// </summary>
        private bool _isPowerOn;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TelemetryBUK" />.
        /// </summary>
        public TelemetryBUK()
        {
            _isPowerOn = false;
        }

        /// <summary>
        /// Получает значение, показывающее, подано ли питание устройству.
        /// </summary>
        public bool IsPowerOn
        {
            get
            {
                return _isPowerOn;
            }
        }

        /// <summary>
        /// Обрабатываем данные телеметрии.
        /// </summary>
        /// <param name="buf">Буфер с данными</param>
        public void Update(byte[] buf)
        {
            _isPowerOn = (buf[6] & 1) == 1;
        }
    }
}
