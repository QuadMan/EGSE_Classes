//-----------------------------------------------------------------------
// <copyright file="EGSEUtilitesControlValues.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр</author>
//-----------------------------------------------------------------------

namespace EGSE.Utilites
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Класс, позволяющий задавать и отслеживать изменения значений различных настроек и параметров
    /// Например, есть регистр, содержащий параметры интерфейса (включен/выключен, скорость, и т.д.), необходимо синхронизировать настройки, сделанные пользователем
    /// и полученные по USB
    /// Для этого есть два свойства: UsbValue и UiValue (приватное), которые устанавливаются соответственно при получении данных с USB и из пользовательского интерфейса.
    /// Далее, раз в секунду необходимо вызывать метод ControlValue.TimeTick(), который проверяет, в случае необходимости значения этий свойств (если свойства не совпадают,
    /// срабатывает событие, определенное для данного свойства).
    /// По-умолчанию, свойство сверяется с данными из USB после установки свойства из пользовательского интерфейса, через 2 отсчета TimeTick.
    /// Если значения различаются (установленные через пользовательский интерфейс и USB), в качестве основного устанавливается значение, полученное из USB.
    /// Так как в одном байте обычно записано несколько свойств, для ControlValue доступен метод AddProperty, который позволяет
    /// указывать побитно, какие биты отвечают за какое свойство.
    /// К примеру,
    /// ControlValue.AddProperty(0, 4, 1, SetFunction, delegate(UInt32 value) { KvvImitatorReady = (value == 1); });
    /// Этим методом мы говорим, что создаем свойство с индексом 0, начинающееся с 4-го бита, длиной в 1 бит. При установке этого свойства вызывается функция SetFunction,
    /// Если значения UsbValue и UiValue не совпали при проверке, вызывается делегат (или функция), описанная в последнем параметре.
    /// При получении значения из Usb, необходимо взвать метод ControlValue.UsbValue = value;
    /// При установке значения из UI: ControlValue.SetProperty(0,1) - устанавливаем свойство с индексом 0 значением, равным 1.
    /// </summary>
    public class ControlValue
    {
        /// <summary>
        /// Счетчик TimerTick.
        /// Примечание:
        /// По истечению лимита сравниваются UsbValue и UiValue.
        /// </summary>
        private const int LimitUiUpdateTick = 3;
        
        /// <summary>
        /// Список свойств у значения управления.
        /// </summary>
        private Dictionary<string, CVProperty> _dictionaryCV = new Dictionary<string, CVProperty>();

        /// <summary>
        /// Значение, которое получаем из USB.
        /// </summary>
        private int _usbValue;

        /// <summary>
        /// Значение, которое уставливается из интерфейса.
        /// </summary>
        private int _uiValue;

        /// <summary>
        /// Значение счетчика времени до проверки совпадения GetValue и SetValue.
        /// </summary>
        private int _timerCnt;

        /// <summary>
        /// Значение по-умолчанию, которое накладывается всегда на устанавливаемое значение.
        /// </summary>
        private int _defaultValue;

        /// <summary>
        /// Флаг говорящий о том, что не нужно записывать значения в USB, используется при первой инициализации.
        /// </summary>
        private bool _refreshFlag;

        /// <summary>
        /// Флаг, сообщает что, экземпляр содержит индикаторные свойства.
        /// </summary>
        private bool _hasIndicate = false;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ControlValue" />.
        /// </summary>
        /// <param name="defaultValue">Можем задать значение по-умолчанию, если нужно, чтобы определенные биты всегда были установлены</param>
        public ControlValue(int defaultValue = 0)
        {
            _usbValue = 0;
            _uiValue = 0;
            _timerCnt = 0;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Делегат, использующийся при описании функции для отправки значения в USB и вызова метода при несовпадении значений UsbValue и UiValue.
        /// </summary>
        /// <param name="value">TODO описание</param>
        public delegate void ControlValueEventHandler(int value);

        /// <summary>
        /// Получает или задает значение, полученное из USB.
        /// </summary>
        public int UsbValue
        {
            get
            {
                return _usbValue;
            }

            set
            {
                _usbValue = value;   
                if (_hasIndicate)
                {
                    CheckPropertiesForChanging(true);
                }
            }
        }

        /// <summary>
        /// Получает или задает значение, установленное из интерфейса пользователем.
        /// </summary>
        public int UIValue
        {
            get
            {
                return _uiValue;
            }

            set
            {
                _uiValue = value;
                _timerCnt = LimitUiUpdateTick;       // проверим значение из USB через некоторое время
            }
        }

        /// <summary>
        /// Добавляет новое свойство.
        /// </summary>
        /// <param name="idx">Индекс свойства (должно быть уникально).</param>
        /// <param name="bitIdx">Индекс бита, с которого свойство начинается.</param>
        /// <param name="bitLen">Длина в битах свойства.</param>
        /// <param name="setUsbEvent">Функция, которая должна вызываться при установке свойства.</param>
        /// <param name="changeEvent">Функция, которая должна вызываться при изменении свойства.</param>
        /// <returns>Если <c>true</c> свойство добавлено успешно.</returns>
        public bool AddProperty(string idx, ushort bitIdx, ushort bitLen, ControlValueEventHandler setUsbEvent, ControlValueEventHandler changeEvent)
        {
            if (_dictionaryCV.ContainsKey(idx) || (bitLen == 0))
            {
                return false;
            }

            _dictionaryCV.Add(idx, new CVProperty(bitIdx, bitLen, setUsbEvent, changeEvent));
            return true;
        }

        /// <summary>
        /// Добавляет новое свойство.
        /// </summary>
        /// <param name="idx">Индекс свойства (должно быть уникально).</param>
        /// <param name="bitIdx">Индекс бита, с которого свойство начинается.</param>
        /// <param name="bitLen">Длина в битах свойства.</param>
        /// <param name="setUsbEvent">Функция, которая должна вызываться при установке свойства.</param>
        /// <param name="changeEvent">Функция, которая должна вызываться при изменении свойства.</param>
        /// <param name="isIndicate">Если установлено <c>true</c> [свойство является индикаторным].</param>
        /// <returns>Если <c>true</c> свойство добавлено успешно.</returns>
        public bool AddProperty(string idx, ushort bitIdx, ushort bitLen, ControlValueEventHandler setUsbEvent, ControlValueEventHandler changeEvent, bool isIndicate)
        {
            if (_dictionaryCV.ContainsKey(idx) || (bitLen == 0))
            {
                return false;
            }

            _dictionaryCV.Add(idx, new CVProperty(bitIdx, bitLen, setUsbEvent, changeEvent, isIndicate));            
            _hasIndicate = true;
            return true;
        }

        /// <summary>
        /// Задаем значение свойства для величины pValue.
        /// </summary>
        /// <param name="propertyIdx">Index of the property.</param>
        /// <param name="valueP">Значение величины, к которому нужно применть установку свойства</param>
        /// <param name="autoSendValue">Автоматически отправлять значение (вызовом делегата SetUsbEvent)</param>
        /// <returns>
        /// True - если выполнено успешно
        /// </returns>
        public bool SetProperty(string propertyIdx, int valueP, bool autoSendValue = true)
        {
            if (!_dictionaryCV.ContainsKey(propertyIdx))
            {
                return false;
            }

            CVProperty cv = _dictionaryCV[propertyIdx];

            int mask = 0;
            for (ushort i = 0; i < cv.BitLen; i++)
            {
                mask |= 1 << (cv.BitIdx + i);
            }

            valueP &= mask >> cv.BitIdx;
            _uiValue &= ~mask;
            _uiValue |= valueP << cv.BitIdx;
            if (_refreshFlag)
            {
                return true;
            }

            if (autoSendValue)
            {
                _timerCnt = LimitUiUpdateTick;
                cv.SetUsbEvent(_uiValue);
            }

            return true;
        }

        /// <summary>
        /// Метод для принудительного вызова через определнное время проверки
        /// установленного и полученного значений
        /// </summary>
        public void RefreshGetValue()
        {
            _timerCnt = LimitUiUpdateTick;
            _refreshFlag = true;
        }

        /// <summary>
        /// Один тик таймера (вызывается из внешнего прерывания и может быть любым, хоть 1 секунда, хоть 500 мс)
        /// </summary>
        public void TimerTick()
        {
            if ((_timerCnt > 0) && (--_timerCnt == 0))
            {
                // пришло время для проверки Get и Set Value
                if ((_uiValue != _usbValue) || _refreshFlag)
                {
                    CheckPropertiesForChanging(false);
                }
            }
        }

        /// <summary>
        /// Получаем значение свойства из величины value.
        /// </summary>
        /// <param name="cv">Описание свойства</param>
        /// <param name="value">Значение величины, для которого нужно взять свойство</param>
        /// <returns>Величина value свойства</returns>
        private int GetCVProperty(CVProperty cv, int value)
        {
            if (cv == null)
            {
                return -1;
            }

            int mask = 0;
            for (ushort i = 0; i < cv.BitLen; i++)
            {
                mask |= 1 << i;
            }

            value >>= cv.BitIdx;
            value &= mask;

            return value;
        }

        /// <summary>
        /// Проверяем UsbVal и UiVal на изменения в свойствах.
        /// Какие свойства изменились, для таких свойств вызваем делегаты ChangeEvent.
        /// </summary>
        /// <param name="checkIndicate">если установлено <c>true</c> [свойство является индикаторным].</param>
        private void CheckPropertiesForChanging(bool checkIndicate)
        {
            int usbVal;
            int uiVal;

            foreach (KeyValuePair<string, CVProperty> pair in _dictionaryCV)
            {
                usbVal = GetCVProperty(pair.Value, _usbValue);
                uiVal = GetCVProperty(pair.Value, _uiValue);
                
                if (((usbVal != -1) && (uiVal != -1) && (usbVal != uiVal)) || _refreshFlag)
                {
                    if (checkIndicate && !(pair.Value as CVProperty).IsIndicate)
                    {
                        continue;
                    }

                    pair.Value.ChangeEvent(usbVal);
                }
            }

            _refreshFlag = false;
        }

        /// <summary>
        /// Класс свойства.
        /// </summary>
        private class CVProperty
        {
            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="CVProperty" />.
            /// </summary>
            /// <param name="bitIdx">Позиция первого бита.</param>
            /// <param name="bitLen">Длина свойства(в битах).</param>
            /// <param name="setUsbEvent">Делегат, вызывается принеобходимости ищменить свойство.</param>
            /// <param name="changeEvent">Если через 2 TickTime значения от USB не совпадают, вызывается этот делегат.</param>
            /// <param name="indicate">Если задан <c>true</c> [свойство является индикатором].</param>
            public CVProperty(ushort bitIdx, ushort bitLen, ControlValueEventHandler setUsbEvent, ControlValueEventHandler changeEvent, bool indicate = false)
            {
                BitIdx = bitIdx;
                BitLen = bitLen;
                SetUsbEvent = setUsbEvent;
                ChangeEvent = changeEvent;
                IsIndicate = indicate;
            }

            /// <summary>
            /// Получает или задает индекс (в битах) с которого начинается значение.
            /// </summary>
            public ushort BitIdx { get; set; }

            /// <summary>
            /// Получает или задает длину (в битах) значения.
            /// </summary>
            public ushort BitLen { get; set; }

            /// <summary>
            /// Получает или задает делегат, вызваемый, когда необходимо записать значение в USB.
            /// </summary>
            public ControlValueEventHandler SetUsbEvent { get; set; }

            /// <summary>
            /// Получает или задает делегат, вызываемый, когда значения из USB и заданные пользователем несовпадают.
            /// </summary>
            public ControlValueEventHandler ChangeEvent { get; set; }

            /// <summary>
            /// Получает или задает значение, показывающее, что [свойство индикаторное] (не нуждается в отправке в USB, автоматически обновляется).
            /// </summary>
            /// <value>
            ///   <c>true</c> если [свойство индикаторное]; иначе, <c>false</c>.
            /// </value>
            public bool IsIndicate { get; set; }
        }
    }
}