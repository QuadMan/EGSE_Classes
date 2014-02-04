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
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.ComponentModel;
    using System.Diagnostics;
    using EGSE;
    using EGSE.Protocols;
    using EGSE.USB;
    using EGSE.Utilites;
    using EGSE.Constants;
    using EGSE.Defaults;
    using EGSE.Telemetry;

    /// <summary>
    /// Прописываются команды управления прибором по USB.
    /// </summary>
    public class DeviceBUK : Device
    {
        private const int TIME_RESET_ADDR = 0x01;
        private const int TIME_DATA_ADDR = 0x02;
        private const int TIME_SET_ADDR = 0x03;
        private const int TIME_OBT_ADDR = 0x12;
        //
        private const int POWER_SET_ADDR = 0x05;
        private const int HSI_BUNI_CTRL_ADDR = 0x06;
        private const int HSI_XSAN_CTRL_ADDR = 0x09;
        private const int HSI_UKS_ADDR = 0x08;

        private byte[] buf;

        public DeviceBUK(string Serial, ProtocolUSBBase dec)
            : base(Serial, dec, new USBCfg(10))
        {
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

        public void CmdSendUKS(byte[] UKSBuf)
        {
            base.SendCmd(HSI_UKS_ADDR, UKSBuf);
        }

        public void CmdPowerOnOff(UInt32 turnOn)
        {
            turnOn &= 1;
            base.SendCmd(POWER_SET_ADDR, new byte[1] { (byte)turnOn });
        }

        /// <summary>
        /// Стандарная команда установки времени
        /// </summary>
        public void CmdSendTime()
        {
            EgseTime time = new EgseTime();
            time.Encode();

            byte[] OBTData = new byte[5];
            OBTData[0] = 0;
            OBTData[1] = 0;
            OBTData[2] = 0;
            OBTData[3] = 0;
            OBTData[4] = 10;

            buf = new byte[1] { 1 };

            base.SendCmd(TIME_RESET_ADDR, buf);
            base.SendCmd(TIME_DATA_ADDR, time.Data);
            base.SendCmd(TIME_OBT_ADDR, OBTData);
            base.SendCmd(TIME_SET_ADDR, buf);
        }
    }

    /// <summary>
    /// Общий экземпляр, позволяющий управлять прибором (принимать данные, выдавать команды).
    /// </summary>
    public class IBUK : INotifyPropertyChanged
    {
        private const int TIME_ADDR_GET = 0x04;
        private const int TM_DATA_GET = 5;

        private ProtocolUSB5E4D _decoder;

        // приватные поля для свойств, управляющих интерфейсом
        private bool _connected;

        private bool _writeXsanDataToFile;

        // куда записываем данные с XSAN
        private FileStream _xsanDataLogStream;
        // с какого канала записываются данные (основного или резервного)
        private uint _xsanChannelForWriting;

        /// <summary>
        /// Доступ к USB устройству
        /// </summary>
        public DeviceBUK Device;

        public bool Connected
        {
            get { return _connected; }
            private set
            {
                _connected = value;
                FirePropertyChangedEvent("Connected");
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

        public bool WriteXsanDataToFile
        {
            get { return _writeXsanDataToFile; }
            set
            {
                _writeXsanDataToFile = value;
                WriteXsanData(value);
                FirePropertyChangedEvent("WriteXsanDataToFile");
            }
        }

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

        public string BUKFileName
        {
            get 
            {
                if (_xsanDataLogStream != null)
                {
                    return _xsanDataLogStream.Name;
                }
                else return string.Empty;
            }
        }

        /// <summary>
        /// Время, пришедшее от КИА
        /// </summary>       
        public EgseTime ETime;

        /// <summary>
        /// Модуль декодера телеметрии
        /// </summary>
        public TelemetryBUK Tm;

        /// <summary>
        /// cписок управляющих элементов
        /// </summary>
        public List<ControlValue> ControlValuesList = new List<ControlValue>();

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="IBUK" />.
        /// </summary>
        public IBUK()
        {
            Connected = false;
            ControlValuesList.Add(new ControlValue()); // XSAN_CTRL_IDX
            ControlValuesList.Add(new ControlValue()); // BUNI_CTRL_IDX
            ControlValuesList.Add(new ControlValue()); // POWER_CTRL_IDX

            _decoder = new ProtocolUSB5E4D(null, LogsClass.LogUSB, false, true);
            _decoder.GotProtocolMsg += new ProtocolUSBBase.ProtocolMsgEventHandler(onMessageFunc);
            _decoder.GotProtocolError += new ProtocolUSBBase.ProtocolErrorEventHandler(onErrorFunc);

            ETime = new EgseTime();

            Device = new DeviceBUK(BUKConst.DeviceSerial, _decoder);
            Device.ChangeStateEvent = onChangeConnection;
            
            _xsanDataLogStream = null;
            _xsanChannelForWriting = 0;
            _writeXsanDataToFile = false;

            Tm = new TelemetryBUK();
            //
            /*ControlValuesList[BUKConst.XSAN_CTRL_IDX].AddProperty(BUKConst.PROPERTY_XSAN_READY_IDX, 4, 1, Device.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorReady = (value == 1);
            });
            ControlValuesList[BUKConst.XSAN_CTRL_IDX].AddProperty(BUKConst.PROPERTY_XSAN_BUSY_IDX, 5, 1, Device.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorBusyOn = (value == 1);
            });
            ControlValuesList[BUKConst.XSAN_CTRL_IDX].AddProperty(BUKConst.PROPERTY_XSAN_ME_IDX, 6, 1, Device.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorMeOn = (value == 1);
            });
            ControlValuesList[BUKConst.XSAN_CTRL_IDX].AddProperty(BUKConst.PROPERTY_XSAN_CMD_CH_IDX, 0, 2, Device.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorCmdChannel = (int)value;

            });
            ControlValuesList[BUKConst.XSAN_CTRL_IDX].AddProperty(BUKConst.PROPERTY_XSAN_DAT_CH_IDX, 2, 2, Device.CmdHSIXSANControl, delegate(UInt32 value)
            {
                XsanImitatorDatChannel = (int)value;

            });
            //
            ControlValuesList[BUKConst.BUNI_CTRL_IDX].AddProperty(BUKConst.PROPERTY_BUNI_ON_IDX, 0, 1, Device.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorOn = (value == 1);
            });
            ControlValuesList[BUKConst.BUNI_CTRL_IDX].AddProperty(BUKConst.PROPERTY_BUNI_CMD_CH_IDX, 1, 1, Device.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorCmdChannel = (int)value;
            });
            ControlValuesList[BUKConst.BUNI_CTRL_IDX].AddProperty(BUKConst.PROPERTY_BUNI_DAT_CH_IDX, 2, 2, Device.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorDatChannel = (int)value;
            });
            ControlValuesList[BUKConst.BUNI_CTRL_IDX].AddProperty(BUKConst.PROPERTY_BUNI_HZ_IDX, 4, 1, Device.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorTimeStampOn = (value == 1);
            });
            ControlValuesList[BUKConst.BUNI_CTRL_IDX].AddProperty(BUKConst.PROPERTY_BUNI_KBV_IDX, 5, 1, Device.CmdHSIBUNIControl, delegate(UInt32 value)
            {
                BuniImitatorObtOn = (value == 1);
            });
            //
            ControlValuesList[BUKConst.POWER_CTRL_IDX].AddProperty(BUKConst.PROPERTY_POWER_IDX, 0, 1, Device.CmdPowerOnOff, delegate(UInt32 value) { });*/
        }

        /// <summary>
        /// Для каждого элемента управления тикаем временем
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
        /// Вызывается при подключении прибора, чтобы все элементы управления обновили свои значения.
        /// </summary>
        private void refreshAllControlsValues()
        {
            Debug.Assert(ControlValuesList != null, "ControlValuesList не должны быть равны null!");
            foreach (ControlValue cv in ControlValuesList)
            {
                cv.RefreshGetValue();
            }
        }

        /// <summary>
        /// Метод вызывается, когда прибор подсоединяется или отсоединяется.
        /// </summary>
        /// <param name="state">Текущее состояние прибора TRUE - подключен, FALSE - отключен</param>
        void onChangeConnection(bool isConnected)
        {
            Connected = isConnected;
            if (Connected)
            {
                Device.CmdSendTime();
                refreshAllControlsValues();
                LogsClass.LogMain.LogText = "КИА БУК подключен";
            }
            else
            {
                LogsClass.LogMain.LogText = "КИА БУК отключен";
            }
        }

        /// <summary>
        /// Указываем какой файл использовать для записи данных от прибора XSAN и по какому каналу
        /// </summary>
        /// <param name="fStream">Поток для записи данных</param>
        /// <param name="channel">По какому каналу</param>
        public void SetFileAndChannelForLogXSANData(FileStream fStream, uint channel)
        {
            _xsanDataLogStream = fStream;
            _xsanChannelForWriting = channel;
        }

        public void WriteXsanData(bool StartWrite)
        {
            if (StartWrite)
            {
                string dataLogDir = Directory.GetCurrentDirectory().ToString() + "\\DATA\\";
                Directory.CreateDirectory(dataLogDir);
                string fileName = dataLogDir + "xsan_" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".dat";
                _xsanDataLogStream = new FileStream(fileName, System.IO.FileMode.Create);

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
                if (_xsanDataLogStream != null)
                {
                    _xsanDataLogStream.Close();
                    _xsanDataLogStream = null;
                }
            }
        }

        /// <summary>
        /// Метод обрабатывающий сообщения от декодера USB
        /// </summary>
        /// <param name="msg">Сообщение</param>
        private void onMessageFunc(MsgBase msg)
        {
            ProtocolMsgEventArgs msg1 = msg as ProtocolMsgEventArgs;
            if (msg1 != null)
            {
                switch (msg1.Addr)
                {
                    case TIME_ADDR_GET:
                        Array.Copy(msg1.Data, 0, ETime.Data, 0, 6);
                        break;
                    case TM_DATA_GET:
                        Tm.Update(msg1.Data);
                        ControlValuesList[BUKConst.PowerCTRL].UsbValue = msg1.Data[6];
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
        private void onErrorFunc(ProtocolErrorEventArgs msg)
        {
            string bufferStr = Converter.ByteArrayToHexStr(msg.Data);
            LogsClass.LogErrors.LogText =  msg.Msg + " (" + bufferStr + ", на позиции: " + msg.ErrorPos.ToString() + ")";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChangedEvent(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
