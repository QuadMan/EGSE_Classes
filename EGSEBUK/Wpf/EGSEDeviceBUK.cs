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
    /// Прописываются команды управления прибором по USB.
    /// </summary>
    public class DeviceBUK : Device
    {
        /// <summary>
        /// Адресный байт "Сброс адреса записи времени"
        /// </summary>
        private const int TimeResetAddr = 0x01;

        /// <summary>
        /// Адресный байт "Запись данных времени"
        /// </summary>
        private const int TimeDataAddr = 0x02;

        /// <summary>
        /// Адресный байт "Бит установки времени"
        /// </summary>
        private const int TimeSetAddr = 0x03;

        /// <summary>
        /// Адресный байт "Релейные команды [15:8]"
        /// </summary>
        private const int PowerHiAddr = 0x41;

        /// <summary>
        /// Адресный байт "Релейные команды [7:0]"
        /// </summary>
        private const int PowerLoAddr = 0x40;

        /// <summary>
        /// Адресный байт "Бит выдачи релейных команд"
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

        /*public void CmdHSIBUNIControl(UInt32 HSIImitControl)
        {
            buf = new byte[1] { (byte)HSIImitControl };
            base.SendCmd(HSI_BUNI_CTRL_ADDR, buf);
        }*/

        /*public void CmdHSIXSANControl(UInt32 HSIControl)
        {
            int frameSize = 496;
            buf = new byte[3] { (byte)HSIControl, (byte)(frameSize >> 8), (byte)frameSize };
            base.SendCmd(HSI_XSAN_CTRL_ADDR, buf);
        }*/

        /////// <summary>
        /////// Commands the send uks.
        /////// </summary>
        /////// <param name="UKSBuf">данные (УКС) для передачи в USB</param>
        ////public void CmdSendUKS(byte[] UKSBuf)
        ////{
        ////    SendCmd(HSI_UKS_ADDR, UKSBuf);
        ////}

        /// <summary>
        /// Отправляет команду включить питание ПК1 БУСК.
        /// </summary>
        /// <param name="buskPower1">Передаваемый параметр</param>
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
        /// Отправляет команду включить питание ПК2 БУСК.
        /// </summary>
        /// <param name="buskPower2">Передаваемый параметр</param>
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
        /// Отправляет команду включить питание ПК1 БУНД.
        /// </summary>
        /// <param name="bundPower1">Передаваемый параметр</param>
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
        /// Отправляет команду включить питание ПК2 БУНД.
        /// </summary>
        /// <param name="bundPower2">Передаваемый параметр</param>
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
        /// Команда установки времени.
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

        // private bool _writeBUKDataToFile;

        /// <summary>
        /// Экземпляр класса, представляющий файл для записи данных с устройства.
        /// </summary>
        private FileStream _bukDataLogStream;

        ////// с какого канала записываются данные (основного или резервного)
        ////private uint _bukChannelForWriting;

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

            _bukDataLogStream = null;
            ////_bukChannelForWriting = 0;
            ////_writeBUKDataToFile = false;

            Tele = new TelemetryBUK();

            ControlValuesList[BUKConst.PowerControl].AddProperty(BUKConst.PropertyBUSKPower1, 7, 1, Device.CmdBUSKPower1, delegate(uint value) { BUSKPower1 = 1 == value; });
            ControlValuesList[BUKConst.PowerControl].AddProperty(BUKConst.PropertyBUSKPower2, 6, 1, Device.CmdBUSKPower2, delegate(uint value) { BUSKPower2 = 1 == value; });
            ControlValuesList[BUKConst.PowerControl].AddProperty(BUKConst.PropertyBUNDPower1, 4, 1, Device.CmdBUNDPower1, delegate(uint value) { BUNDPower1 = 1 == value; });
            ControlValuesList[BUKConst.PowerControl].AddProperty(BUKConst.PropertyBUNDPower2, 5, 1, Device.CmdBUNDPower2, delegate(uint value) { BUNDPower2 = 1 == value; });
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Получает или задает доступ к USB устройству.
        /// </summary>
        public DeviceBUK Device { get; set; }

        /// <summary>
        /// Получает значение, показывающее, [подключено] ли устройство.
        /// </summary>
        /// <value>
        ///   <c>true</c> если [подключено]; иначе, <c>false</c>.
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
        /// Получает значение, показывающее, есть ли питание первого полукомплекта БУСК.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [busk power1]; otherwise, <c>false</c>.
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
        /// Получает значение, показывающее, есть ли питание второго полукомплекта БУСК.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [busk power2]; otherwise, <c>false</c>.
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
        /// Получает значение, показывающее, есть ли питание первого полукомплекта БУНД.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [bund power1]; otherwise, <c>false</c>.
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
        /// Получает значение, показывающее, есть ли питание второго полукомплекта БУНД.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [bund power2]; otherwise, <c>false</c>.
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
        /// Получает значение, показывающее, нужно ли передавать релейную команду БУСК по линии A.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is busk line a]; otherwise, <c>false</c>.
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
        /// Получает значение, показывающее, нужно ли передавать релейную команду БУСК по линии B.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is busk line b]; otherwise, <c>false</c>.
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
        /// Получает значение, показывающее, нужно ли передавать релейную команду БУНД по линии A.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is bund line a]; otherwise, <c>false</c>.
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
        /// Получает значение, показывающее, нужно ли передавать релейную команду БУНД по линии B.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is bund line b]; otherwise, <c>false</c>.
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

        /*public int BuniImitatorCmdChannel
        {
            get { return _buniImitatorCmdChannel; }
            set
            {
                _buniImitatorCmdChannel = value;
                ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_CMD_CH_IDX, value); 
                FirePropertyChangedEvent("BuniImitatorCmdChannel");
            }
        }

        public int BuniImitatorDatChannel
        {
            get { return _buniImitatorDatChannel; }
            set
            {
                _buniImitatorDatChannel = value;
                ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_DAT_CH_IDX, value); 
                FirePropertyChangedEvent("BuniImitatorDatChannel");
            }
        }

        public bool BuniImitatorOn
        {
            get { return _buniImitatorOn; }
            set 
            { 
                _buniImitatorOn = value;
                ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_ON_IDX, Convert.ToInt32(value)); 
                FirePropertyChangedEvent("BuniImitatorOn"); 
            }
        }

        public bool BuniImitatorTimeStampOn
        {
            get { return _buniImitatorTimeStampOn; }
            set
            {
                _buniImitatorTimeStampOn = value;
                ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_HZ_IDX, Convert.ToInt32(value)); 
                FirePropertyChangedEvent("BuniImitatorTimeStampOn");
            }
        }

        public bool BuniImitatorObtOn
        {
            get { return _buniImitatorObtOn; }
            
            set
            {
                _buniImitatorObtOn = value;
                ControlValuesList[BUKConst.BUNI_CTRL_IDX].SetProperty(BUKConst.PROPERTY_BUNI_KBV_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("BuniImitatorObtOn");
            }
        }

        public int XsanImitatorCmdChannel
        {
            get { return _xsanImitatorCmdChannel; }
            set
            {
                _xsanImitatorCmdChannel = value;
                ControlValuesList[BUKConst.XSAN_CTRL_IDX].SetProperty(BUKConst.PROPERTY_XSAN_CMD_CH_IDX, value);
                FirePropertyChangedEvent("XsanImitatorCmdChannel");
            }
        }

        public int XsanImitatorDatChannel
        {
            get { return _xsanImitatorDatChannel; }
            set
            {
                _xsanImitatorDatChannel = value;
                ControlValuesList[BUKConst.XSAN_CTRL_IDX].SetProperty(BUKConst.PROPERTY_XSAN_DAT_CH_IDX, value);
                FirePropertyChangedEvent("XsanImitatorDatChannel");
            }
        }

        public bool XsanImitatorReady
        {
            get { return _xsanImitatorReady; }
            set
            {
                _xsanImitatorReady = value;
                ControlValuesList[BUKConst.XSAN_CTRL_IDX].SetProperty(BUKConst.PROPERTY_XSAN_READY_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("XsanImitatorReady");
            }
        }

        public bool XsanImitatorBusyOn
        {
            get { return _xsanImitatorBusyOn; }
            set
            {
                _xsanImitatorBusyOn = value;
                ControlValuesList[BUKConst.XSAN_CTRL_IDX].SetProperty(BUKConst.PROPERTY_XSAN_BUSY_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("XsanImitatorBusyOn");
            }
        }

        public bool XsanImitatorMeOn
        {
            get { return _xsanImitatorMeOn; }
            set
            {
                _xsanImitatorMeOn = value;
                ControlValuesList[BUKConst.XSAN_CTRL_IDX].SetProperty(BUKConst.PROPERTY_XSAN_ME_IDX, Convert.ToInt32(value));
                FirePropertyChangedEvent("XsanImitatorMeOn");
            }
        }*/

        ////public bool WriteBUKDataToFile
        ////{
        ////    get 
        ////    { 
        ////        return _writeBUKDataToFile; 
        ////    }

        ////    set
        ////    {
        ////        _writeBUKDataToFile = value;
        ////        WriteBUKData(value);
        ////        FirePropertyChangedEvent("WriteBUKDataToFile");
        ////    }
        ////}

        /*public long XsanFileSize
        {
            get 
            {
                if (_xsanDataLogStream != null)
                {
                    return _xsanDataLogStream.Length;
                }
                else return 0;
            }
            //private set { _xsanDataFileSize = value; }
        }*/

        ////public string BUKFileName
        ////{
        ////    get 
        ////    {
        ////        if (_bukDataLogStream != null)
        ////        {
        ////            return _bukDataLogStream.Name;
        ////        }
        ////        else return string.Empty;
        ////    }
        ////}

        /// <summary>
        /// Получает или задает время, пришедшее от КИА.
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
        /// Указываем какой файл использовать для записи данных от прибора XSAN и по какому каналу.
        /// </summary>
        /// <param name="stream">Поток для записи данных.</param>
        /// <param name="channel">По какому каналу.</param>
        public void SetFileAndChannelForLogBUKData(FileStream stream, uint channel)
        {
            _bukDataLogStream = stream;
            ////_bukChannelForWriting = channel;
        }

        /// <summary>
        /// Записывать данные от устройства.
        /// </summary>
        /// <param name="startWrite">if set to <c>true</c> [start write].</param>
        public void WriteBUKData(bool startWrite)
        {
            if (startWrite)
            {
                string dataLogDir = Directory.GetCurrentDirectory().ToString() + "\\DATA\\";
                Directory.CreateDirectory(dataLogDir);
                string fileName = dataLogDir + "buk_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".dat";
                _bukDataLogStream = new FileStream(fileName, System.IO.FileMode.Create);

                // выбираем, по какому каналу записываем данные (по комбобоксу выбора приема данных)
                /* switch (_buniImitatorDatChannel)
                 {
                     case 1: _xsanChannelForWriting = 0;
                         break;
                     case 2: _xsanChannelForWriting = 1;
                         break;
                     case 3:
                         _xsanChannelForWriting = 0;
                         break;
                     default:
                         _xsanChannelForWriting = 0;
                         break;
                 }*/
            }
            else
            {
                if (_bukDataLogStream != null)
                {
                    _bukDataLogStream.Close();
                    _bukDataLogStream = null;
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
        /// Метод обрабатывающий сообщения от декодера USB
        /// </summary>
        /// <param name="msg">Сообщение для обработки</param>
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
                    /*case HSI_XSAN_DATA_GET:
                        HSIInt.XSANStat.Update(msg1.Data, msg1.DataLen);
                        break;
                    case HSI_BUNI_DATA_GET:
                        HSIInt.BUNIStat.Update(msg1.Data, _xsanDataLogStream, _xsanChannelForWriting);
                        break;
                    case HSI_BUNI_CTRL_GET:
                        ControlValuesList[BUKConst.BUNI_CTRL_IDX].UsbValue = msg1.Data[0];
                        break;
                    case HSI_XSAN_CTRL_GET:
                        ControlValuesList[BUKConst.XSAN_CTRL_IDX].UsbValue = msg1.Data[0];
                        break;*/
                }
            }
        }

        /// <summary>
        /// Обработчик ошибок протокола декодера USB.
        /// </summary>
        /// <param name="msg">Сообщение об ошибке</param>
        private void OnErrorFunc(ProtocolErrorEventArgs msg)
        {
            string bufferStr = Converter.ByteArrayToHexStr(msg.Data);
            LogsClass.LogErrors.LogText = msg.Msg + " (" + bufferStr + ", на позиции: " + msg.ErrorPos.ToString() + ")";
        }

        /// <summary>
        /// Fires the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
