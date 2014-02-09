//-----------------------------------------------------------------------
// <copyright file="EGSEDeviceBUK.cs" company="IKI RSSI, laboratory №711">
//     Copyright (c) IKI RSSI, laboratory №711. All rights reserved.
// </copyright>
// <author>Семенов Александр, Коробейщиков Иван</author>
//-----------------------------------------------------------------------

namespace EGSE.Devices
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using EGSE;
    using EGSE.Constants;
    using EGSE.Defaults;
    using EGSE.Protocols;
    using EGSE.Telemetry;
    using EGSE.USB;
    using EGSE.Utilites;

    /// <summary>
    /// Конкретный класс устройства КИА.
    /// Содержит команды и протокол управления прибором по USB.
    /// </summary>
    public class DeviceBUK : Device
    {
        /// <summary>
        /// Адресный байт "Сброс адреса записи времени".
        /// </summary>
        private const int TimeResetAddr = 0x01;

        /// <summary>
        /// Адресный байт "Запись данных времени".
        /// </summary>
        private const int TimeDataAddr = 0x02;

        /// <summary>
        /// Адресный байт "Бит установки времени".
        /// </summary>
        private const int TimeSetAddr = 0x03;

        /// <summary>
        /// Адресный байт "Релейные команды [15:8]".
        /// </summary>
        private const int PowerHiAddr = 0x41;

        /// <summary>
        /// Адресный байт "Релейные команды [7:0]".
        /// </summary>
        private const int PowerLoAddr = 0x40;

        /// <summary>
        /// Адресный байт "Бит выдачи релейных команд".
        /// </summary>
        private const int PowerSetAddr = 0x42;

        /// <summary>
        /// Обеспечивает доступ к интерфейсу устройства. 
        /// </summary>
        private readonly IBUK _intfBUK;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DeviceBUK" />.
        /// </summary>
        /// <param name="serial">Уникальный идентификатор USB.</param>
        /// <param name="dec">Экземпляр декодера USB для данного устройства.</param>
        /// <param name="intfBUK">Интерфейс управления данным устройством.</param>
        public DeviceBUK(string serial, ProtocolUSBBase dec, IBUK intfBUK)
            : base(serial, dec, new USBCfg(10))
        {
            _intfBUK = intfBUK;
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК1 БУСК.
        /// </summary>
        /// <param name="buskPower1"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdBUSKPower1(uint buskPower1)
        {
            byte buf = 0;
            if (_intfBUK.IsBUSKLineA)
            {
                if (0 == buskPower1)
                {
                    buf |= 1 << 2;
                }
                else
                {
                    buf |= 1 << 0;
                }
            }

            if (_intfBUK.IsBUSKLineB)
            {
                if (0 == buskPower1)
                {
                    buf |= 1 << 3;
                }
                else
                {
                    buf |= 1 << 1;
                }
            }

            SendToUSB(PowerLoAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК2 БУСК.
        /// </summary>
        /// <param name="buskPower2"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdBUSKPower2(uint buskPower2)
        {
            byte buf = 0;
            if (_intfBUK.IsBUSKLineA)
            {
                if (0 == buskPower2)
                {
                    buf |= 1 << 6;
                }
                else
                {
                    buf |= 1 << 4;
                }
            }

            if (_intfBUK.IsBUSKLineB)
            {
                if (0 == buskPower2)
                {
                    buf |= 1 << 7;
                }
                else
                {
                    buf |= 1 << 5;
                }
            }

            SendToUSB(PowerLoAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК1 БУНД.
        /// </summary>
        /// <param name="bundPower1"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdBUNDPower1(uint bundPower1)
        {
            byte buf = 0;
            if (_intfBUK.IsBUNDLineA)
            {
                if (0 == bundPower1)
                {
                    buf |= 1 << 1;
                }
                else
                {
                    buf |= 1 << 5;
                }
            }

            if (_intfBUK.IsBUNDLineB)
            {
                if (0 == bundPower1)
                {
                    buf |= 1 << 3;
                }
                else
                {
                    buf |= 1 << 7;
                }
            }

            SendToUSB(PowerHiAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Отправляет команду включить/выключить питание ПК2 БУНД.
        /// </summary>
        /// <param name="bundPower2"><c>1</c> если [включить]; иначе, <c>0</c>.</param>
        public void CmdBUNDPower2(uint bundPower2)
        {
            byte buf = 0;
            if (_intfBUK.IsBUNDLineA)
            {
                if (0 == bundPower2)
                {
                    buf |= 1 << 0;
                }
                else
                {
                    buf |= 1 << 4;
                }
            }

            if (_intfBUK.IsBUNDLineB)
            {
                if (0 == bundPower2)
                {
                    buf |= 1 << 2;
                }
                else
                {
                    buf |= 1 << 6;
                }
            }

            SendToUSB(PowerHiAddr, new byte[1] { buf });
            SendToUSB(PowerSetAddr, new byte[1] { 1 });
        }

        /// <summary>
        /// Команда установки внутреннего времени устройства.
        /// </summary>
        public void CmdSetDeviceTime()
        {
            EGSETime time = new EGSETime();
            time.Encode();
            byte[] buf = new byte[1] { 1 };
            SendToUSB(TimeResetAddr, buf);
            SendToUSB(TimeDataAddr, time.Data);
            SendToUSB(TimeSetAddr, buf);
        }
    }

    /// <summary>
    /// Общий экземпляр, позволяющий управлять прибором (принимать данные, выдавать команды).
    /// </summary>
    public class IBUK : INotifyPropertyChanged
    {
        /// <summary>
        /// Адресный байт "Статус".
        /// </summary>
        private const int TimeDataAddr = 0x01;

        /// <summary>
        /// Адресный байт "Телеметрия".
        /// </summary>
        private const int TeleDataAddr = 0x15;

        /// <summary>
        /// Экземпляр декодера протокола USB.
        /// </summary>
        private ProtocolUSB7C6E _decoder;

        /// <summary>
        /// Текущее состояние подключения устройства.
        /// </summary>
        private bool _connected;

        /// <summary>
        /// Питание первого полукомплекта БУСК.
        /// </summary>
        private bool _buskPower1;

        /// <summary>
        /// Питание второго полукомплекта БУСК.
        /// </summary>
        private bool _buskPower2;

        /// <summary>
        /// Питание первого полукомплекта БУНД.
        /// </summary>
        private bool _bundPower1;

        /// <summary>
        /// Питание второго полукомплекта БУНД.
        /// </summary>
        private bool _bundPower2;

        /// <summary>
        /// Передавать релейные команды БУСК по линии A.
        /// </summary>
        private bool _isBUSKLineA;

        /// <summary>
        /// Передавать релейные команды БУСК по линии B.
        /// </summary>
        private bool _isBUSKLineB;

        /// <summary>
        /// Передавать релейные команды БУНД по линии A.
        /// </summary>
        private bool _isBUNDLineA;

        /// <summary>
        /// Передавать релейные команды БУНД по линии B.
        /// </summary>
        private bool _isBUNDLineB;

        /// <summary>
        /// Записывать данные от прибора в файл.
        /// </summary>
        private bool _isWriteDevDataToFile;

        /// <summary>
        /// Экземпляр класса, представляющий файл для записи данных от прибора.
        /// </summary>
        private FileStream _devDataLogStream;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="IBUK" />.
        /// </summary>
        public IBUK()
        {
            Connected = false;
            ControlValuesList = new List<ControlValue>();
            ControlValuesList.Add(new ControlValue()); // PowerControl = 0

            _decoder = new ProtocolUSB7C6E(null, LogsClass.LogUSB, false, true);
            _decoder.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(OnMessageFunc);
            _decoder.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(OnErrorFunc);
            ETime = new EGSETime();

            Device = new DeviceBUK(BUKConst.DeviceSerial, _decoder, this);
            Device.ChangeStateEvent = OnChangeConnection;
            _devDataLogStream = null;
            _isWriteDevDataToFile = false;

            Tele = new TelemetryBUK();

            ControlValuesList[BUKConst.PowerControl].AddProperty(BUKConst.PropertyBUSKPower1, 7, 1, Device.CmdBUSKPower1, delegate(uint value) { BUSKPower1 = 1 == value; });
            ControlValuesList[BUKConst.PowerControl].AddProperty(BUKConst.PropertyBUSKPower2, 6, 1, Device.CmdBUSKPower2, delegate(uint value) { BUSKPower2 = 1 == value; });
            ControlValuesList[BUKConst.PowerControl].AddProperty(BUKConst.PropertyBUNDPower1, 4, 1, Device.CmdBUNDPower1, delegate(uint value) { BUNDPower1 = 1 == value; });
            ControlValuesList[BUKConst.PowerControl].AddProperty(BUKConst.PropertyBUNDPower2, 5, 1, Device.CmdBUNDPower2, delegate(uint value) { BUNDPower2 = 1 == value; });
        }

        /// <summary>
        /// Вызывается, когда меняется значение свойства.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Получает или задает доступ к USB прибора.
        /// </summary>
        public DeviceBUK Device { get; set; }

        /// <summary>
        /// Получает значение, показывающее, [подключен] ли прибор.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [подключен]; иначе, <c>false</c>.
        /// </value>
        public bool Connected
        {
            get
            {
                return _connected;
            }

            private set
            {
                _connected = value;
                FirePropertyChangedEvent("Connected");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, есть ли [питание первого полукомплекта БУСК].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [питание первого полукомплекта БУСК]; иначе, <c>false</c>.
        /// </value>
        public bool BUSKPower1
        {
            get
            {
                return _buskPower1;
            }

            private set
            {
                _buskPower1 = value;
                FirePropertyChangedEvent("BUSKPower1");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, есть ли [питание второго полукомплекта БУСК].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [питание второго полукомплекта БУСК]; иначе, <c>false</c>.
        /// </value>
        public bool BUSKPower2
        {
            get
            {
                return _buskPower2;
            }

            private set
            {
                _buskPower2 = value;
                FirePropertyChangedEvent("BUSKPower2");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, есть ли [питание первого полукомплекта БУНД].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [питание первого полукомплекта БУНД]; иначе, <c>false</c>.
        /// </value>
        public bool BUNDPower1
        {
            get
            {
                return _bundPower1;
            }

            private set
            {
                _bundPower1 = value;
                FirePropertyChangedEvent("BUSKPower1");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, есть ли [питание второго полукомплекта БУНД].
        /// </summary>
        /// <value>
        ///   <c>true</c> если [питание второго полукомплекта БУНД]; иначе, <c>false</c>.
        /// </value>
        public bool BUNDPower2
        {
            get
            {
                return _bundPower2;
            }

            private set
            {
                _bundPower2 = value;
                FirePropertyChangedEvent("BUNDPower2");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУСК по линии A.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUSKLineA
        {
            get
            {
                return _isBUSKLineA;
            }

            private set
            {
                _isBUSKLineA = value;
                FirePropertyChangedEvent("IsBUSKLineA");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУСК по линии B.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUSKLineB
        {
            get
            {
                return _isBUSKLineB;
            }

            private set
            {
                _isBUSKLineB = value;
                FirePropertyChangedEvent("IsBUSKLineB");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУНД по линии A.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUNDLineA
        {
            get
            {
                return _isBUNDLineA;
            }

            private set
            {
                _isBUNDLineA = value;
                FirePropertyChangedEvent("IsBUNDLineA");
            }
        }

        /// <summary>
        /// Получает значение, показывающее, нужно ли [передавать релейную команду] БУНД по линии B.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [передавать релейную команду]; иначе, <c>false</c>.
        /// </value>
        public bool IsBUNDLineB
        {
            get
            {
                return _isBUNDLineB;
            }

            private set
            {
                _isBUNDLineB = value;
                FirePropertyChangedEvent("IsBUNDLineB");
            }
        }

        /// <summary>
        /// Получает или задает значение, показывающее, нужно ли [записывать данные от прибора в файл].
        /// </summary>
        /// <value>
        /// <c>true</c> если [записывать данные от прибора в файл]; иначе, <c>false</c>.
        /// </value>
        public bool IsWriteDevDataToFile
        {
            get 
            { 
                return _isWriteDevDataToFile; 
            }

            set
            {
                _isWriteDevDataToFile = value;
                WriteDevData(value);
                FirePropertyChangedEvent("IsWriteDevDataToFile");
            }
        }

        /// <summary>
        /// Получает размер файла данных от прибора.
        /// </summary>
        /// <value>
        /// Размер файла (в байтах).
        /// </value>
        public long DevDataFileSize
        {
            get 
            {
                if (_devDataLogStream != null)
                {
                    return _devDataLogStream.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Получает имя файла данных от прибора.  
        /// </summary>
        /// <value>
        /// Имя файла.
        /// </value>
        public string DevDataFileName
        {
            get 
            {
                if (_devDataLogStream != null)
                {
                    return _devDataLogStream.Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Получает или задает время, пришедшее от прибора.
        /// </summary>       
        public EGSETime ETime { get; set; }

        /// <summary>
        /// Получает или задает экземпляр декодера телеметрии.
        /// </summary>
        public TelemetryBUK Tele { get; set; }

        /// <summary>
        /// Получает или задает список управляющих элементов.
        /// </summary>
        public List<ControlValue> ControlValuesList { get; set; }

        /// <summary>
        /// Для каждого элемента управления тикаем временем.
        /// </summary>
        public void TickAllControlsValues()
        {
            Debug.Assert(ControlValuesList != null, "ControlValuesList не должны быть равны null!");

            foreach (ControlValue cv in ControlValuesList)
            {
                cv.TimerTick();
            }
        }

        /// <summary>
        /// Метод вызывается, когда прибор подсоединяется или отсоединяется.
        /// </summary>
        /// <param name="isConnected">Если установлено <c>true</c> [прибор подключен].</param>
        public void OnChangeConnection(bool isConnected)
        {
            Connected = isConnected;
            if (Connected)
            {
                Device.CmdSetDeviceTime();
                RefreshAllControlsValues();
                LogsClass.LogMain.LogText = Resource.Get(@"stDeviceName") + Resource.Get(@"stConnected");
            }
            else
            {
                LogsClass.LogMain.LogText = Resource.Get(@"stDeviceName") + Resource.Get(@"stDisconnected");
            }
        }

        /// <summary>
        /// Указываем какой файл использовать для записи данных от прибора и по какому каналу.
        /// </summary>
        /// <param name="stream">Поток для записи данных.</param>
        public void SetFileForLogDevData(FileStream stream)
        {
            _devDataLogStream = stream;
        }

        /// <summary>
        /// Записывает данные от прибора в файл.
        /// </summary>
        /// <param name="startWrite">Если установлено <c>true</c> [идет запись].</param>
        public void WriteDevData(bool startWrite)
        {
            if (startWrite)
            {
                string dataLogDir = Directory.GetCurrentDirectory().ToString() + "\\DATA\\";
                Directory.CreateDirectory(dataLogDir);
                string fileName = dataLogDir + Resource.Get(@"stDevLogName") + "_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".dat";
                _devDataLogStream = new FileStream(fileName, System.IO.FileMode.Create);
            }
            else 
            {
                if (_devDataLogStream != null)
                {
                    _devDataLogStream.Close();
                    _devDataLogStream = null;
                }
            }
        }

        /// <summary>
        /// Вызывается при подключении прибора, чтобы все элементы управления обновили свои значения.
        /// </summary>
        private void RefreshAllControlsValues()
        {
            Debug.Assert(ControlValuesList != null, Resource.Get(@"eNotAssigned"));
            foreach (ControlValue cv in ControlValuesList)
            {
                cv.RefreshGetValue();
            }
        }

        /// <summary>
        /// Метод, обрабатывающий сообщения от декодера USB.
        /// </summary>
        /// <param name="msg">Сообщение для обработки.</param>
        private void OnMessageFunc(ProtocolMsgEventArgs msg)
        {
            if (msg != null)
            {
                switch (msg.Addr)
                {
                    case TimeDataAddr:
                        Array.Copy(msg.Data, 0, ETime.Data, 0, 6);
                        break;
                    case TeleDataAddr:
                        Tele.Update(msg.Data);
                        ControlValuesList[BUKConst.PowerControl].UsbValue = msg.Data[3];
                        break;
                }
            }
        }

        /// <summary>
        /// Обработчик ошибок протокола декодера USB.
        /// </summary>
        /// <param name="msg">Сообщение об ошибке.</param>
        private void OnErrorFunc(ProtocolErrorEventArgs msg)
        {
            string bufferStr = Converter.ByteArrayToHexStr(msg.Data);
            LogsClass.LogErrors.LogText = string.Format(Resource.Get(@"stOnErrorUSBMsg"), msg.Msg, bufferStr, msg.ErrorPos.ToString());
        }

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Имя свойства.</param>
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
