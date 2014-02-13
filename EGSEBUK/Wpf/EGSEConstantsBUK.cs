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
    /// Основные значения констант прибора.
    /// </summary>
    public static class BUKConst
    {
        /// <summary>
        /// Индекс свойства [Питание ПК1 БУСК].
        /// </summary>
        public const int PropertyBUSKPower1 = 0;

        /// <summary>
        /// Индекс свойства [Питание ПК2 БУСК].
        /// </summary>
        public const int PropertyBUSKPower2 = 1;

        /// <summary>
        /// Индекс свойства [Питание ПК1 БУНД].
        /// </summary>
        public const int PropertyBUNDPower1 = 2;

        /// <summary>
        /// Индекс свойства [Питание ПК2 БУНД].
        /// </summary>
        public const int PropertyBUNDPower2 = 3;

        /// <summary>
        /// Индекс свойства [Выбор канала SpaceWire2].
        /// </summary>
        public const int PropertySpaceWire2Channel = 0;

        /// <summary>
        /// Индекс свойства [D].
        /// </summary>
        public const int PropertySpaceWire2IntfOn = 1;

        /// <summary>
        /// Индекс объекта управления POWER в массиве ControlValuesList.
        /// </summary>
        public const int PowerControl = 0;

        /// <summary>
        /// Индекс объекта управления SpaceWire2 в массиве ControlValuesList.
        /// </summary>
        public const int SpaceWire2 = 1;

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
