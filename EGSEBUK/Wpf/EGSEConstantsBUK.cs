//-----------------------------------------------------------------------
// <copyright file="EGSEConstantsBUK.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Constants
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using EGSE.Utilites;

    /// <summary>
    /// Основные значения константы устройства.
    /// </summary>
    public static class BUKConst
    {
        /// <summary>
        /// Индекс свойств в ControlValues.
        /// </summary>
        public const int PropertyPower = 10;

        /// <summary>
        /// Индекс объекта управления XSAN в массиве ControlValuesList
        /// </summary>
        public const int BUKCTRL = 0;

        /// <summary>
        /// Индекс объекта управления POWER в массиве ControlValuesList
        /// </summary>
        public const int PowerCTRL = 2;

        /// <summary>
        /// Заголовок главного окна.
        /// </summary>
        public static readonly string ShowCaption;

        /// <summary>
        /// Название КИА.
        /// </summary>
        public static readonly string DeviceName;

        /// <summary>
        /// Уникальный идентификатор USB.
        /// </summary>
        public static readonly string DeviceSerial;
        
        /// <summary>
        /// Инициализирует статические поля класса <see cref="BUKConst" />.
        /// </summary>
        static BUKConst()
        { 
            ShowCaption = Resource.Get("stShowCaption");
            DeviceName = Resource.Get("stDeviceName");
            DeviceSerial = Resource.Get("stDeviceSerial");
        }
    }
}
